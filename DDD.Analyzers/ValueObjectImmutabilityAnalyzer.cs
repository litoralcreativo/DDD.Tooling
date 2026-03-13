using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DDD.Analyzers
{
	/// <summary>
	/// Analizador que valida que los ValueObjects sean inmutables y sobrescriban Equals/GetHashCode
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ValueObjectImmutabilityAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(
				DiagnosticDescriptors.ValueObjectShouldBeImmutable,
				DiagnosticDescriptors.ValueObjectMustOverrideEquals,
				DiagnosticDescriptors.ValueObjectMustOverrideGetHashCode);

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

			// 1. Verificar propiedades con setters públicos (inmutabilidad)
			// Excluimos init setters, ya que son válidos en ValueObjects (inmutables tras construcción)
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
					var diagnostic = Diagnostic.Create(
						DiagnosticDescriptors.ValueObjectShouldBeImmutable,
						propertySyntax.GetLocation(),
						property.Name,
						classSymbol.Name);

					context.ReportDiagnostic(diagnostic);
				}
			}

			// 2. Verificar que sobrescriba Equals(object)
			var overridesEquals = classSymbol.GetMembers("Equals")
				.OfType<IMethodSymbol>()
				.Any(m => m.IsOverride &&
						 m.Parameters.Length == 1 &&
						 m.Parameters[0].Type.SpecialType == SpecialType.System_Object);

			if (!overridesEquals)
			{
				var diagnostic = Diagnostic.Create(
					DiagnosticDescriptors.ValueObjectMustOverrideEquals,
					classDeclaration.Identifier.GetLocation(),
					classSymbol.Name);

				context.ReportDiagnostic(diagnostic);
			}

			// 3. Verificar que sobrescriba GetHashCode()
			var overridesGetHashCode = classSymbol.GetMembers("GetHashCode")
				.OfType<IMethodSymbol>()
				.Any(m => m.IsOverride && m.Parameters.Length == 0);

			if (!overridesGetHashCode)
			{
				var diagnostic = Diagnostic.Create(
					DiagnosticDescriptors.ValueObjectMustOverrideGetHashCode,
					classDeclaration.Identifier.GetLocation(),
					classSymbol.Name);

				context.ReportDiagnostic(diagnostic);
			}
		}
	}
}
