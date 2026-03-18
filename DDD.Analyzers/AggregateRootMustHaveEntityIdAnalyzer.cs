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

			// Si no tiene [AggregateRoot], tampoco aplica la regla DDD002
			if (!hasAggregateRootAttribute)
				return;

			// Buscar si alguna propiedad tiene el atributo [EntityId] (en la clase o en cualquier clase base)
			var hasEntityIdProperty = classSymbol.GetMembers()
				.OfType<IPropertySymbol>()
				.Any(prop => prop.GetAttributes()
					.Any(attr => attr.AttributeClass?.Name == "EntityIdAttribute" &&
								attr.AttributeClass?.ContainingNamespace?.ToString() == "DDD.Abstractions"));

			// También es válido si hereda de IAggregateRoot<TId>, AggregateRoot<TId> o IEntity<TId>
			if (!hasEntityIdProperty)
				hasEntityIdProperty = InheritsFromAggregateBase(classSymbol);

			if (!hasEntityIdProperty)
			{
				var diagnostic = Diagnostic.Create(
					DiagnosticDescriptors.AggregateRootMustHaveEntityId,
					classDeclaration.Identifier.GetLocation(),
					classSymbol.Name);

				context.ReportDiagnostic(diagnostic);
			}
		}

		/// <summary>
		/// Devuelve true si la clase hereda (directa o indirectamente) de
		/// <c>DDD.Abstractions.AggregateRoot&lt;TId&gt;</c> o implementa <c>DDD.Abstractions.IAggregateRoot&lt;TId&gt;</c>.
		/// </summary>
		private static bool InheritsFromAggregateBase(INamedTypeSymbol classSymbol)
		{
			// Comprobar interfaces implementadas (resuelve toda la cadena de herencia)
			if (classSymbol.AllInterfaces.Any(i =>
				(i.OriginalDefinition.Name == "IAggregateRoot" || i.OriginalDefinition.Name == "IEntity") &&
				i.OriginalDefinition.ContainingNamespace?.ToString() == "DDD.Abstractions"))
				return true;

			// Comprobar clase base (AggregateRoot<TId> o Entity<TId>)
			var baseType = classSymbol.BaseType;
			while (baseType != null)
			{
				if ((baseType.OriginalDefinition.Name == "AggregateRoot" || baseType.OriginalDefinition.Name == "Entity") &&
					baseType.OriginalDefinition.ContainingNamespace?.ToString() == "DDD.Abstractions")
					return true;
				baseType = baseType.BaseType;
			}

			return false;
		}
	}
}
