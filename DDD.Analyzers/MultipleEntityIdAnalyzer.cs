using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DDD.Analyzers
{
	/// <summary>
	/// DDD013: Una Entity o AggregateRoot no puede tener más de una propiedad con [EntityId].
	/// En DDD, la identidad es única e inmutable — múltiples [EntityId] es un anti-patrón.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class MultipleEntityIdAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(DiagnosticDescriptors.MultipleEntityIds);

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

			if (classSymbol == null) return;

			// Solo aplicar a Entity o AggregateRoot
			var isEntity = classSymbol.GetAttributes().Any(a =>
				a.AttributeClass?.Name == "EntityAttribute" &&
				a.AttributeClass?.ContainingNamespace?.ToString() == "DDD.Abstractions");

			var isAggregateRoot = classSymbol.GetAttributes().Any(a =>
				a.AttributeClass?.Name == "AggregateRootAttribute" &&
				a.AttributeClass?.ContainingNamespace?.ToString() == "DDD.Abstractions");

			if (!isEntity && !isAggregateRoot) return;

			// Recopilar todas las propiedades con [EntityId]
			var entityIdProperties = classSymbol.GetMembers()
				.OfType<IPropertySymbol>()
				.Where(p => p.GetAttributes().Any(a =>
					a.AttributeClass?.Name == "EntityIdAttribute" &&
					a.AttributeClass?.ContainingNamespace?.ToString() == "DDD.Abstractions"))
				.ToList();

			if (entityIdProperties.Count <= 1) return;

			// Construir lista de nombres para el mensaje: "Id, Name"
			var names = string.Join(", ", entityIdProperties.Select(p => p.Name));

			context.ReportDiagnostic(Diagnostic.Create(
				DiagnosticDescriptors.MultipleEntityIds,
				classDeclaration.Identifier.GetLocation(),
				classSymbol.Name,
				entityIdProperties.Count,
				names));
		}
	}
}
