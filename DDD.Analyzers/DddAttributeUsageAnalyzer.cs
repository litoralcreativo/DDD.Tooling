using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DDD.Analyzers
{
	/// <summary>
	/// Analizador que valida el uso correcto de los atributos DDD:
	/// DDD003: [EntityId] solo puede aplicarse a propiedades
	/// DDD005: una clase no puede ser [Entity] y [ValueObject] simultáneamente
	/// DDD006: una clase no puede ser [AggregateRoot] y [ValueObject] simultáneamente
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DddAttributeUsageAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(
				DiagnosticDescriptors.EntityIdOnlyOnProperties,
				DiagnosticDescriptors.CannotBeEntityAndValueObject,
				DiagnosticDescriptors.CannotBeAggregateRootAndValueObject);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
			context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
		}

		private void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
		{
			var fieldDeclaration = (FieldDeclarationSyntax)context.Node;

			// Verificar si alguna variable del campo tiene [EntityId]
			var hasEntityIdAttribute = fieldDeclaration.AttributeLists
				.SelectMany(al => al.Attributes)
				.Any(attr =>
				{
					var name = attr.Name.ToString();
					return name == "EntityId" || name == "EntityIdAttribute";
				});

			if (!hasEntityIdAttribute) return;

			// Reportar en cada variable declarada en el campo
			foreach (var variable in fieldDeclaration.Declaration.Variables)
			{
				context.ReportDiagnostic(Diagnostic.Create(
					DiagnosticDescriptors.EntityIdOnlyOnProperties,
					fieldDeclaration.GetLocation(),
					variable.Identifier.Text));
			}
		}

		private void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
		{
			var classDeclaration = (ClassDeclarationSyntax)context.Node;
			var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);

			if (classSymbol == null)
				return;

			var attributes = classSymbol.GetAttributes();

			var hasEntity = attributes.Any(attr =>
				attr.AttributeClass?.Name == "EntityAttribute" &&
				attr.AttributeClass?.ContainingNamespace?.ToString() == "DDD.Abstractions");

			var hasAggregateRoot = attributes.Any(attr =>
				attr.AttributeClass?.Name == "AggregateRootAttribute" &&
				attr.AttributeClass?.ContainingNamespace?.ToString() == "DDD.Abstractions");

			var hasValueObject = attributes.Any(attr =>
				attr.AttributeClass?.Name == "ValueObjectAttribute" &&
				attr.AttributeClass?.ContainingNamespace?.ToString() == "DDD.Abstractions");

			// Validar Entity + ValueObject
			if (hasEntity && hasValueObject)
			{
				var diagnostic = Diagnostic.Create(
					DiagnosticDescriptors.CannotBeEntityAndValueObject,
					classDeclaration.Identifier.GetLocation(),
					classSymbol.Name);

				context.ReportDiagnostic(diagnostic);
			}

			// Validar AggregateRoot + ValueObject
			if (hasAggregateRoot && hasValueObject)
			{
				var diagnostic = Diagnostic.Create(
					DiagnosticDescriptors.CannotBeAggregateRootAndValueObject,
					classDeclaration.Identifier.GetLocation(),
					classSymbol.Name);

				context.ReportDiagnostic(diagnostic);
			}
		}
	}
}
