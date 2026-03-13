using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DDD.Analyzers
{
	/// <summary>
	/// Analizador que valida que los ValueObjects sean inmutables (no tengan setters públicos)
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ValueObjectImmutabilityAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(DiagnosticDescriptors.ValueObjectShouldBeImmutable);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
		}

		private void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
		{
			var classDeclaration = (ClassDeclarationSyntax)context.Node;
			var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);

			if (classSymbol == null)
				return;

			// Verificar si la clase tiene el atributo [ValueObject]
			var hasValueObjectAttribute = classSymbol.GetAttributes()
				.Any(attr => attr.AttributeClass?.Name == "ValueObjectAttribute" &&
							attr.AttributeClass?.ContainingNamespace?.ToString() == "DDD.Abstractions");

			if (!hasValueObjectAttribute)
				return;

			// Verificar propiedades con setters públicos
			var propertiesWithPublicSetters = classSymbol.GetMembers()
				.OfType<IPropertySymbol>()
				.Where(prop => prop.SetMethod != null &&
							  prop.SetMethod.DeclaredAccessibility == Accessibility.Public);

			foreach (var property in propertiesWithPublicSetters)
			{
				var propertySyntax = property.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
				if (propertySyntax != null)
				{
					var diagnostic = Diagnostic.Create(
						DiagnosticDescriptors.ValueObjectShouldBeImmutable,
						propertySyntax.GetLocation(),
						property.Name,
						classSymbol.Name);

					context.ReportDiagnostic(diagnostic);
				}
			}
		}
	}
}
