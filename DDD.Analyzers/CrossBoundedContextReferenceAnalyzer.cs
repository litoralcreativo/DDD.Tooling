using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DDD.Analyzers
{
	/// <summary>
	/// DDD011: Detecta referencias directas entre clases de distintos Bounded Contexts.
	/// Una clase de BC "Orders" no puede tener una propiedad cuyo tipo sea una clase de BC "Customers".
	/// En su lugar, debe referenciar solo por Id.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class CrossBoundedContextReferenceAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(DiagnosticDescriptors.NoCrossContextDirectReference);

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

			var ownerAttributes = classSymbol.GetAttributes();

			// La clase debe tener [BoundedContext] o [SharedKernel] para ser analizada
			var ownerBoundedContext = GetBoundedContextName(classSymbol);
			var ownerIsSharedKernel = ownerAttributes
				.Any(a => a.AttributeClass?.Name == "SharedKernelAttribute");

			if (ownerBoundedContext == null && !ownerIsSharedKernel)
				return;

			// Revisar todas las propiedades públicas
			foreach (var member in classDeclaration.Members.OfType<PropertyDeclarationSyntax>())
			{
				var propertySymbol = context.SemanticModel.GetDeclaredSymbol(member);
				if (propertySymbol == null)
					continue;

				var propertyType = propertySymbol.Type as INamedTypeSymbol;
				if (propertyType == null)
					continue;

				// El tipo referenciado debe ser [Entity], [AggregateRoot] o [ValueObject]
				var isDddType = propertyType.GetAttributes().Any(a =>
					a.AttributeClass?.Name == "EntityAttribute" ||
					a.AttributeClass?.Name == "AggregateRootAttribute" ||
					a.AttributeClass?.Name == "ValueObjectAttribute");

				if (!isDddType)
					continue;

				var referencedIsSharedKernel = propertyType.GetAttributes()
					.Any(a => a.AttributeClass?.Name == "SharedKernelAttribute");

				var referencedBoundedContext = GetBoundedContextName(propertyType);

				// Regla 1: [SharedKernel] no puede referenciar tipos de un BC específico
				if (ownerIsSharedKernel && referencedBoundedContext != null)
				{
					var diagnostic = Diagnostic.Create(
						DiagnosticDescriptors.NoCrossContextDirectReference,
						member.GetLocation(),
						classSymbol.Name,
						"SharedKernel",
						propertyType.Name,
						referencedBoundedContext);

					context.ReportDiagnostic(diagnostic);
					continue;
				}

				// Regla 2: cualquier BC puede referenciar [SharedKernel] libremente
				if (referencedIsSharedKernel)
					continue;

				// Regla 3: BCs distintos no pueden referenciarse directamente
				if (ownerBoundedContext != null && referencedBoundedContext != null &&
					ownerBoundedContext != referencedBoundedContext)
				{
					var diagnostic = Diagnostic.Create(
						DiagnosticDescriptors.NoCrossContextDirectReference,
						member.GetLocation(),
						classSymbol.Name,
						ownerBoundedContext,
						propertyType.Name,
						referencedBoundedContext);

					context.ReportDiagnostic(diagnostic);
				}
			}
		}

		private static string GetBoundedContextName(INamedTypeSymbol typeSymbol)
		{
			var bcAttribute = typeSymbol.GetAttributes()
				.FirstOrDefault(a => a.AttributeClass?.Name == "BoundedContextAttribute");

			if (bcAttribute == null)
				return null;

			return bcAttribute.ConstructorArguments.FirstOrDefault().Value?.ToString();
		}
	}
}
