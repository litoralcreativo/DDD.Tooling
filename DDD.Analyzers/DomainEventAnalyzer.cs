using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DDD.Analyzers
{
	/// <summary>
	/// Analizador que valida las reglas de los Domain Events:
	/// - DDD014: DomainEvent debe ser inmutable (sin setters públicos)
	/// - DDD015: DomainEvent debe tener propiedad OccurredOn (DateTime/DateTimeOffset)
	/// - DDD016: DomainEvent debe declarar su Bounded Context
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DomainEventAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(
				DiagnosticDescriptors.DomainEventMustBeImmutable,
				DiagnosticDescriptors.DomainEventMustHaveOccurredOn,
				DiagnosticDescriptors.DomainEventMustDeclareBoundedContext);

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

			var attributes = classSymbol.GetAttributes();

			// Solo aplica a clases con [DomainEvent]
			var hasDomainEvent = attributes
				.Any(a => a.AttributeClass?.Name == "DomainEventAttribute" &&
						  a.AttributeClass?.ContainingNamespace?.ToString() == "DDD.Abstractions");

			if (!hasDomainEvent)
				return;

			// DDD014 — Verificar propiedades con setters públicos (inmutabilidad)
			// Los accessors 'init' son válidos (inmutables tras construcción)
			var propertiesWithPublicSetters = classSymbol.GetMembers()
				.OfType<IPropertySymbol>()
				.Where(prop => prop.SetMethod != null &&
							   prop.SetMethod.DeclaredAccessibility == Accessibility.Public &&
							   !prop.SetMethod.IsInitOnly);

			foreach (var property in propertiesWithPublicSetters)
			{
				var propertySyntax = property.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
				if (propertySyntax != null)
				{
					context.ReportDiagnostic(Diagnostic.Create(
						DiagnosticDescriptors.DomainEventMustBeImmutable,
						propertySyntax.GetLocation(),
						property.Name,
						classSymbol.Name));
				}
			}

			// DDD015 — Verificar que tenga propiedad OccurredOn de tipo DateTime/DateTimeOffset
			var hasOccurredOn = classSymbol.GetMembers("OccurredOn")
				.OfType<IPropertySymbol>()
				.Any(p =>
				{
					var typeName = p.Type.ToDisplayString();
					return typeName == "System.DateTime" ||
						   typeName == "System.DateTimeOffset" ||
						   typeName == "System.DateTime?" ||
						   typeName == "System.DateTimeOffset?";
				});

			if (!hasOccurredOn)
			{
				context.ReportDiagnostic(Diagnostic.Create(
					DiagnosticDescriptors.DomainEventMustHaveOccurredOn,
					classDeclaration.Identifier.GetLocation(),
					classSymbol.Name));
			}

			// DDD016 — Verificar que tenga [BoundedContext] o [SharedKernel]
			var hasBoundedContext = attributes
				.Any(a => a.AttributeClass?.Name == "BoundedContextAttribute");

			var isSharedKernel = attributes
				.Any(a => a.AttributeClass?.Name == "SharedKernelAttribute");

			if (!hasBoundedContext && !isSharedKernel)
			{
				context.ReportDiagnostic(Diagnostic.Create(
					DiagnosticDescriptors.DomainEventMustDeclareBoundedContext,
					classDeclaration.Identifier.GetLocation(),
					classSymbol.Name));
			}
		}
	}
}
