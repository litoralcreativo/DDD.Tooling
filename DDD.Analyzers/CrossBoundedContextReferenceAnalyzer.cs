using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DDD.Analyzers
{
	/// <summary>
	/// DDD011: Detecta referencias directas entre clases de distintos Bounded Contexts en propiedades no-privadas,
	/// incluyendo colecciones genéricas (List&lt;T&gt;, IReadOnlyCollection&lt;T&gt;, etc.).
	/// DDD012: Detecta uso de tipos de otro BC en campos privados (Info — acoplamiento implícito).
	/// Ignora campos privados en DDD011 ya que son detalles de implementación internos del BC.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class CrossBoundedContextReferenceAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(
				DiagnosticDescriptors.NoCrossContextDirectReference,
				DiagnosticDescriptors.NoCrossContextEntityReference,
				DiagnosticDescriptors.NoCrossContextValueObjectReference,
				DiagnosticDescriptors.CrossContextInternalUsage);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
		}

		private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
		{
			var classDeclaration = (ClassDeclarationSyntax)context.Node;
			var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);

			if (classSymbol == null)
				return;

			// La clase debe tener [BoundedContext] o [SharedKernel] para ser analizada
			var ownerBoundedContext = GetBoundedContextName(classSymbol);
			var ownerIsSharedKernel = classSymbol.GetAttributes()
				.Any(a => a.AttributeClass?.Name == "SharedKernelAttribute");

			if (ownerBoundedContext == null && !ownerIsSharedKernel)
				return;

			// Analizar solo propiedades no-privadas (campos privados son implementación interna)
			foreach (var property in classDeclaration.Members.OfType<PropertyDeclarationSyntax>())
			{
				// Ignorar propiedades privadas
				if (property.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)))
					continue;

				var propertySymbol = context.SemanticModel.GetDeclaredSymbol(property);
				if (propertySymbol == null)
					continue;

				// Extraer los tipos DDD a verificar (resuelve genéricos: List<T> → T)
				var dddTypesToCheck = ExtractDddTypes(propertySymbol.Type);

				foreach (var (typeSymbol, isAggregateRoot, isEntity) in dddTypesToCheck)
				{
					var referencedBc = GetBoundedContextName(typeSymbol);
					var referencedIsSharedKernel = typeSymbol.GetAttributes()
						.Any(a => a.AttributeClass?.Name == "SharedKernelAttribute");

					// Cualquier BC puede referenciar [SharedKernel] libremente
					if (referencedIsSharedKernel)
						continue;

					// Solo nos interesa si el tipo referenciado tiene BC declarado
					if (referencedBc == null)
						continue;

					// Regla 1: [SharedKernel] no puede referenciar tipos de un BC específico
					if (ownerIsSharedKernel)
					{
						context.ReportDiagnostic(Diagnostic.Create(
							GetDescriptor(isAggregateRoot, isEntity),
							property.GetLocation(),
							classSymbol.Name, "SharedKernel",
							typeSymbol.Name, referencedBc));
						continue;
					}

					// Regla 2: BCs distintos no pueden referenciarse directamente
					if (ownerBoundedContext != referencedBc)
					{
						context.ReportDiagnostic(Diagnostic.Create(
							GetDescriptor(isAggregateRoot, isEntity),
							property.GetLocation(),
							classSymbol.Name, ownerBoundedContext,
							typeSymbol.Name, referencedBc));
					}
				}
			}

			// DDD012: campos privados que usan tipos de otro BC (Info — acoplamiento implícito)
			foreach (var field in classDeclaration.Members.OfType<FieldDeclarationSyntax>())
			{
				// Solo campos privados
				if (!field.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)))
					continue;

				var fieldType = context.SemanticModel.GetTypeInfo(field.Declaration.Type).Type;
				if (fieldType == null)
					continue;

				var dddTypesToCheck = ExtractDddTypes(fieldType);

				foreach (var (typeSymbol, _, _) in dddTypesToCheck)
				{
					var referencedBc = GetBoundedContextName(typeSymbol);
					var referencedIsSharedKernel = typeSymbol.GetAttributes()
						.Any(a => a.AttributeClass?.Name == "SharedKernelAttribute");

					if (referencedIsSharedKernel || referencedBc == null)
						continue;

					var ownerBc = ownerIsSharedKernel ? "SharedKernel" : ownerBoundedContext;
					if (ownerBc == referencedBc)
						continue;

					foreach (var variable in field.Declaration.Variables)
					{
						context.ReportDiagnostic(Diagnostic.Create(
							DiagnosticDescriptors.CrossContextInternalUsage,
							field.GetLocation(),
							variable.Identifier.Text,
							classSymbol.Name, ownerBc,
							typeSymbol.Name, referencedBc));
					}
				}
			}
		}

		/// <summary>
		/// Extrae los tipos DDD relevantes de un tipo de propiedad.
		/// Para tipos genéricos (List&lt;T&gt;, IReadOnlyCollection&lt;T&gt;, IEnumerable&lt;T&gt;, etc.)
		/// analiza los argumentos de tipo en lugar del contenedor.
		/// Devuelve tuplas (tipo, esAggregateRoot, esEntity).
		/// </summary>
		private static IEnumerable<(INamedTypeSymbol Type, bool IsAggregateRoot, bool IsEntity)> ExtractDddTypes(ITypeSymbol type)
		{
			if (type is INamedTypeSymbol namedType)
			{
				// Tipo genérico: List<Course>, IReadOnlyCollection<Course>, IEnumerable<Course>...
				if (namedType.IsGenericType)
				{
					foreach (var typeArg in namedType.TypeArguments.OfType<INamedTypeSymbol>())
					{
						var (isAr, isEn) = GetDddKind(typeArg);
						if (isAr || isEn || IsValueObject(typeArg))
							yield return (typeArg, isAr, isEn);
					}
				}
				else
				{
					// Tipo directo: Course, Address...
					var (isAr, isEn) = GetDddKind(namedType);
					if (isAr || isEn || IsValueObject(namedType))
						yield return (namedType, isAr, isEn);
				}
			}
		}

		private static (bool IsAggregateRoot, bool IsEntity) GetDddKind(INamedTypeSymbol typeSymbol)
		{
			var attrs = typeSymbol.GetAttributes();
			bool isAr = attrs.Any(a => a.AttributeClass?.Name == "AggregateRootAttribute");
			bool isEn = attrs.Any(a => a.AttributeClass?.Name == "EntityAttribute");
			return (isAr, isEn);
		}

		private static bool IsValueObject(INamedTypeSymbol typeSymbol) =>
			typeSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "ValueObjectAttribute");

		private static string GetBoundedContextName(INamedTypeSymbol typeSymbol)
		{
			var bcAttribute = typeSymbol.GetAttributes()
				.FirstOrDefault(a => a.AttributeClass?.Name == "BoundedContextAttribute");

			return bcAttribute?.ConstructorArguments.FirstOrDefault().Value?.ToString();
		}

		private static DiagnosticDescriptor GetDescriptor(bool isAggregateRoot, bool isEntity)
		{
			if (isAggregateRoot)
				return DiagnosticDescriptors.NoCrossContextDirectReference;
			if (isEntity)
				return DiagnosticDescriptors.NoCrossContextEntityReference;
			return DiagnosticDescriptors.NoCrossContextValueObjectReference;
		}
	}
}
