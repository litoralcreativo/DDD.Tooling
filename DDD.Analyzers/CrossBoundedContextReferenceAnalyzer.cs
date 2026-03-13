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

			var ownerBcLabel = ownerIsSharedKernel ? "SharedKernel" : ownerBoundedContext;

			// Recorremos todos los miembros: propiedades y campos
			foreach (var member in classDeclaration.Members)
			{
				ITypeSymbol memberType = null;
				Location memberLocation = null;
				bool isPrivateMember = false;
				string memberName = null;

				if (member is PropertyDeclarationSyntax property)
				{
					// Propiedad privada o protegida → DDD012 (Info)
					// Propiedad pública o internal → DDD011 (Error)
					isPrivateMember = property.Modifiers.Any(m =>
						m.IsKind(SyntaxKind.PrivateKeyword) || m.IsKind(SyntaxKind.ProtectedKeyword));

					var propSymbol = context.SemanticModel.GetDeclaredSymbol(property);
					if (propSymbol == null) continue;

					memberType = propSymbol.Type;
					memberLocation = property.GetLocation();
					memberName = property.Identifier.Text;
				}
				else if (member is FieldDeclarationSyntax field)
				{
					// Todos los campos → DDD012 (Info), son detalles de implementación
					isPrivateMember = true;
					memberType = context.SemanticModel.GetTypeInfo(field.Declaration.Type).Type;
					memberLocation = field.GetLocation();
					memberName = field.Declaration.Variables.FirstOrDefault()?.Identifier.Text;
				}

				if (memberType == null) continue;

				foreach (var (typeSymbol, isAggregateRoot, isEntity) in ExtractDddTypes(memberType))
				{
					var referencedBc = GetBoundedContextName(typeSymbol);

					// SharedKernel puede ser referenciado libremente
					if (typeSymbol.GetAttributes().Any(a => a.AttributeClass?.Name == "SharedKernelAttribute"))
						continue;

					// Solo tipos con BC declarado
					if (referencedBc == null)
						continue;

					// Mismo BC → OK
					if (!ownerIsSharedKernel && ownerBoundedContext == referencedBc)
						continue;

					if (isPrivateMember)
					{
						// DDD012 — Info: miembro privado/campo con tipo de otro BC
						context.ReportDiagnostic(Diagnostic.Create(
							DiagnosticDescriptors.CrossContextInternalUsage,
							memberLocation,
							memberName, classSymbol.Name, ownerBcLabel,
							typeSymbol.Name, referencedBc));
					}
					else
					{
						// DDD011 — Error: miembro público con tipo de otro BC
						context.ReportDiagnostic(Diagnostic.Create(
							GetDescriptor(isAggregateRoot, isEntity),
							memberLocation,
							classSymbol.Name, ownerBcLabel,
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
