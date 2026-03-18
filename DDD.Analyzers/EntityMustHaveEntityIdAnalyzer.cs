using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DDD.Analyzers
{
	/// <summary>
	/// Analizador que valida que las clases decoradas con [Entity] tengan al menos una propiedad con [EntityId]
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class EntityMustHaveEntityIdAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(DiagnosticDescriptors.EntityMustHaveEntityId);

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

			// Si no tiene [Entity], tampoco aplica la regla DDD001
			if (!hasEntityAttribute)
				return;

			// Buscar si alguna propiedad tiene el atributo [EntityId]
			var hasEntityIdProperty = classSymbol.GetMembers()
				.OfType<IPropertySymbol>()
				.Any(prop => prop.GetAttributes()
					.Any(attr => attr.AttributeClass?.Name == "EntityIdAttribute" &&
								attr.AttributeClass?.ContainingNamespace?.ToString() == "DDD.Abstractions"));

			// También es válido si hereda de IEntity<TId> o de una clase base que lo implemente
			if (!hasEntityIdProperty)
				hasEntityIdProperty = InheritsFromEntityBase(classSymbol);

			if (!hasEntityIdProperty)
			{
				var diagnostic = Diagnostic.Create(
					DiagnosticDescriptors.EntityMustHaveEntityId,
					classDeclaration.Identifier.GetLocation(),
					classSymbol.Name);

				context.ReportDiagnostic(diagnostic);
			}
		}

		/// <summary>
		/// Devuelve true si la clase hereda (directa o indirectamente) de
		/// <c>DDD.Abstractions.Entity&lt;TId&gt;</c> o implementa <c>DDD.Abstractions.IEntity&lt;TId&gt;</c>.
		/// </summary>
		private static bool InheritsFromEntityBase(INamedTypeSymbol classSymbol)
		{
			// Comprobar interfaces implementadas (resuelve toda la cadena de herencia)
			if (classSymbol.AllInterfaces.Any(i =>
				i.OriginalDefinition.Name == "IEntity" &&
				i.OriginalDefinition.ContainingNamespace?.ToString() == "DDD.Abstractions"))
				return true;

			// Comprobar clase base (Entity<TId>)
			var baseType = classSymbol.BaseType;
			while (baseType != null)
			{
				if (baseType.OriginalDefinition.Name == "Entity" &&
					baseType.OriginalDefinition.ContainingNamespace?.ToString() == "DDD.Abstractions")
					return true;
				baseType = baseType.BaseType;
			}

			return false;
		}
	}
}
