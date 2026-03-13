using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DDD.Analyzers
{
	/// <summary>
	/// Analizador que sugiere el uso de Factory Methods en Entities
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class EntityFactoryMethodAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(DiagnosticDescriptors.EntityShouldUseFactoryMethod);

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

			// Verificar si tiene atributo [Entity] o [AggregateRoot]
			var hasEntityAttribute = classSymbol.GetAttributes()
				.Any(attr => attr.AttributeClass?.Name == "EntityAttribute");

			var hasAggregateRootAttribute = classSymbol.GetAttributes()
				.Any(attr => attr.AttributeClass?.Name == "AggregateRootAttribute");

			if (!hasEntityAttribute && !hasAggregateRootAttribute)
				return;

			// Verificar si tiene constructor público (no generado por compilador)
			var hasPublicConstructor = classSymbol.Constructors
				.Any(c => c.DeclaredAccessibility == Accessibility.Public && !c.IsImplicitlyDeclared);

			if (!hasPublicConstructor)
				return; // Ya tiene constructor privado/protected, ¡perfecto!

			// Verificar si ya tiene algún Factory Method estático público que devuelva la instancia
			var hasFactoryMethod = classSymbol.GetMembers()
				.OfType<IMethodSymbol>()
				.Any(m => m.IsStatic &&
						 m.DeclaredAccessibility == Accessibility.Public &&
						 SymbolEqualityComparer.Default.Equals(m.ReturnType, classSymbol));

			if (hasFactoryMethod)
				return; // Ya tiene Factory Method, ¡excelente!

			// Sugerir el uso de Factory Method
			var diagnostic = Diagnostic.Create(
				DiagnosticDescriptors.EntityShouldUseFactoryMethod,
				classDeclaration.Identifier.GetLocation(),
				classSymbol.Name);

			context.ReportDiagnostic(diagnostic);
		}
	}
}
