using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DDD.Analyzers
{
	/// <summary>
	/// DDD017: Valida que las clases decoradas con [AggregateRoot] implementen IAggregateRoot{TId}
	/// (directa o indirectamente a través de AggregateRoot{TId}).
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class AggregateRootInterfaceImplementationAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(DiagnosticDescriptors.AggregateRootMustImplementInterface);

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

			// Verificar que implemente IAggregateRoot<TId> (directa o a través de herencia)
			var implementsIAggregateRoot = classSymbol.AllInterfaces.Any(i =>
				i.OriginalDefinition.Name == "IAggregateRoot" &&
				i.OriginalDefinition.ContainingNamespace?.ToString() == "DDD.Abstractions");

			if (!implementsIAggregateRoot)
			{
				var diagnostic = Diagnostic.Create(
					DiagnosticDescriptors.AggregateRootMustImplementInterface,
					classDeclaration.Identifier.GetLocation(),
					classSymbol.Name);

				context.ReportDiagnostic(diagnostic);
			}
		}
	}
}
