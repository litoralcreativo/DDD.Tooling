using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DDD.Analyzers
{
	/// <summary>
	/// Analizador que valida que las clases decoradas con [AggregateRoot] tengan al menos una propiedad con [EntityId]
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AggregateRootMustHaveEntityIdAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(DiagnosticDescriptors.AggregateRootMustHaveEntityId);

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

			// Verificar si la clase tiene el atributo [AggregateRoot]
			var hasAggregateRootAttribute = classSymbol.GetAttributes()
				.Any(attr => attr.AttributeClass?.Name == "AggregateRootAttribute" &&
							attr.AttributeClass?.ContainingNamespace?.ToString() == "DDD.Abstractions");

			if (!hasAggregateRootAttribute)
				return;

			// Buscar si alguna propiedad tiene el atributo [EntityId]
			var hasEntityIdProperty = classSymbol.GetMembers()
				.OfType<IPropertySymbol>()
				.Any(prop => prop.GetAttributes()
					.Any(attr => attr.AttributeClass?.Name == "EntityIdAttribute" &&
								attr.AttributeClass?.ContainingNamespace?.ToString() == "DDD.Abstractions"));

			if (!hasEntityIdProperty)
			{
				var diagnostic = Diagnostic.Create(
					DiagnosticDescriptors.AggregateRootMustHaveEntityId,
					classDeclaration.Identifier.GetLocation(),
					classSymbol.Name);

				context.ReportDiagnostic(diagnostic);
			}
		}
	}
}
