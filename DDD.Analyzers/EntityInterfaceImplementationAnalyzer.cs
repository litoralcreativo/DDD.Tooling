using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DDD.Analyzers
{
	/// <summary>
	/// DDD018: Valida que las clases decoradas con [Entity] implementen IEntity{TId}
	/// (directa o indirectamente a través de Entity{TId}).
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class EntityInterfaceImplementationAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(DiagnosticDescriptors.EntityMustImplementInterface);

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

			// Verificar si la clase tiene el atributo [Entity]
			var hasEntityAttribute = classSymbol.GetAttributes()
				.Any(attr => attr.AttributeClass?.Name == "EntityAttribute" &&
							attr.AttributeClass?.ContainingNamespace?.ToString() == "DDD.Abstractions");

			if (!hasEntityAttribute)
				return;

			// Verificar que implemente IEntity<TId> (directa o a través de herencia)
			var implementsIEntity = classSymbol.AllInterfaces.Any(i =>
				i.OriginalDefinition.Name == "IEntity" &&
				i.OriginalDefinition.ContainingNamespace?.ToString() == "DDD.Abstractions");

			if (!implementsIEntity)
			{
				var diagnostic = Diagnostic.Create(
					DiagnosticDescriptors.EntityMustImplementInterface,
					classDeclaration.Identifier.GetLocation(),
					classSymbol.Name);

				context.ReportDiagnostic(diagnostic);
			}
		}
	}
}
