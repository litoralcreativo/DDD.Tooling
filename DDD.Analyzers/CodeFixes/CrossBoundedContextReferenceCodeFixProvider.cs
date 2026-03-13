using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace DDD.Analyzers.CodeFixes
{
	/// <summary>
	/// Code Fix para DDD011: reemplaza una propiedad que referencia directamente un tipo de otro BC
	/// por una propiedad con el Id del tipo referenciado.
	/// 
	/// Caso 1 — Tipo simple:
	///   ANTES: public Course Course { get; set; }
	///   DESPUÉS: public Guid CourseId { get; set; }
	/// 
	/// Caso 2 — Colección genérica:
	///   ANTES: public List&lt;Course&gt; Courses { get; set; }
	///   DESPUÉS: public List&lt;Guid&gt; CourseIds { get; set; }
	/// 
	/// El tipo del Id se infiere buscando [EntityId] en la clase referenciada.
	/// Si no se encuentra, se usa Guid como fallback.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CrossBoundedContextReferenceCodeFixProvider)), Shared]
	public class CrossBoundedContextReferenceCodeFixProvider : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds =>
			ImmutableArray.Create("DDD011");

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			if (root == null) return;

			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			var property = root.FindToken(diagnosticSpan.Start)
				.Parent?.AncestorsAndSelf()
				.OfType<PropertyDeclarationSyntax>()
				.FirstOrDefault();

			if (property == null) return;

			var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
			if (semanticModel == null) return;

			var propertySymbol = semanticModel.GetDeclaredSymbol(property, context.CancellationToken);
			if (propertySymbol == null) return;

			var propertyType = propertySymbol.Type as INamedTypeSymbol;
			if (propertyType == null) return;

			if (propertyType.IsGenericType)
			{
				// Caso 2: colección genérica — reemplazar el argumento de tipo
				var dddTypeArg = propertyType.TypeArguments
					.OfType<INamedTypeSymbol>()
					.FirstOrDefault(t => t.GetAttributes().Any(a =>
						a.AttributeClass?.Name == "AggregateRootAttribute" ||
						a.AttributeClass?.Name == "EntityAttribute" ||
						a.AttributeClass?.Name == "ValueObjectAttribute"));

				if (dddTypeArg == null) return;

				var idTypeName = GetEntityIdTypeName(dddTypeArg);
				var containerTypeName = GetContainerTypeName(propertyType);
				var newPropertyName = BuildCollectionIdName(property.Identifier.Text, dddTypeArg.Name);
				var title = $"Reemplazar '{property.Identifier.Text}' por '{newPropertyName}' ({containerTypeName}<{idTypeName}>)";

				context.RegisterCodeFix(
					CodeAction.Create(
						title: title,
						createChangedDocument: c => ReplaceGenericWithIdCollectionAsync(
							context.Document, root, property, containerTypeName, idTypeName, newPropertyName, dddTypeArg, c),
						equivalenceKey: nameof(CrossBoundedContextReferenceCodeFixProvider) + "_Generic"),
					diagnostic);
			}
			else
			{
				// Caso 1: tipo simple — reemplazar directamente
				var idTypeName = GetEntityIdTypeName(propertyType);
				var newPropertyName = propertyType.Name + "Id";
				var title = $"Reemplazar '{property.Identifier.Text}' por '{newPropertyName}' ({idTypeName})";

				context.RegisterCodeFix(
					CodeAction.Create(
						title: title,
						createChangedDocument: c => ReplaceSimpleWithIdPropertyAsync(
							context.Document, root, property, idTypeName, newPropertyName, propertyType, c),
						equivalenceKey: nameof(CrossBoundedContextReferenceCodeFixProvider) + "_Simple"),
					diagnostic);
			}
		}

		// ─── Caso 1: tipo simple ──────────────────────────────────────────────────

		private static Task<Document> ReplaceSimpleWithIdPropertyAsync(
			Document document,
			SyntaxNode root,
			PropertyDeclarationSyntax property,
			string idTypeName,
			string newPropertyName,
			INamedTypeSymbol referencedType,
			CancellationToken cancellationToken)
		{
			var newTypeSyntax = SyntaxFactory.ParseTypeName(idTypeName)
				.WithTrailingTrivia(SyntaxFactory.Space);

			var newProperty = property
				.WithType(newTypeSyntax)
				.WithIdentifier(SyntaxFactory.Identifier(newPropertyName))
				.WithAdditionalAnnotations(Formatter.Annotation);

			var newRoot = root.ReplaceNode(property, newProperty);
			newRoot = EnsureUsingDirective(newRoot, idTypeName);
			newRoot = TryRemoveOrphanedUsing(newRoot, referencedType);

			return Task.FromResult(document.WithSyntaxRoot(newRoot));
		}

		// ─── Caso 2: colección genérica ──────────────────────────────────────────

		private static Task<Document> ReplaceGenericWithIdCollectionAsync(
			Document document,
			SyntaxNode root,
			PropertyDeclarationSyntax property,
			string containerTypeName,
			string idTypeName,
			string newPropertyName,
			INamedTypeSymbol dddTypeArg,
			CancellationToken cancellationToken)
		{
			// Construir el nuevo tipo: List<Guid>, IReadOnlyCollection<Guid>, etc.
			var newTypeSyntax = SyntaxFactory.ParseTypeName($"{containerTypeName}<{idTypeName}>")
				.WithTrailingTrivia(SyntaxFactory.Space);

			var newProperty = property
				.WithType(newTypeSyntax)
				.WithIdentifier(SyntaxFactory.Identifier(newPropertyName))
				.WithAdditionalAnnotations(Formatter.Annotation);

			var newRoot = root.ReplaceNode(property, newProperty);
			newRoot = EnsureUsingDirective(newRoot, idTypeName);
			newRoot = TryRemoveOrphanedUsing(newRoot, dddTypeArg);

			return Task.FromResult(document.WithSyntaxRoot(newRoot));
		}

		// ─── Helpers ─────────────────────────────────────────────────────────────

		/// <summary>
		/// Busca [EntityId] en el tipo referenciado y devuelve el nombre simple del tipo.
		/// Fallback: "Guid".
		/// </summary>
		private static string GetEntityIdTypeName(INamedTypeSymbol referencedType)
		{
			foreach (var member in referencedType.GetMembers().OfType<IPropertySymbol>())
			{
				if (member.GetAttributes().Any(a => a.AttributeClass?.Name == "EntityIdAttribute"))
					return member.Type.Name;
			}
			return "Guid";
		}

		/// <summary>
		/// Devuelve el nombre simple del tipo contenedor genérico.
		/// Ej: List&lt;Course&gt; → "List", IReadOnlyCollection&lt;Course&gt; → "IReadOnlyCollection"
		/// </summary>
		private static string GetContainerTypeName(INamedTypeSymbol genericType) =>
			genericType.Name;

		/// <summary>
		/// Construye el nombre de la propiedad de Ids para colecciones.
		/// Si ya termina en el nombre del tipo + "s" → reemplaza por tipo + "Ids"
		/// Ej: "Courses" con tipo "Course" → "CourseIds"
		/// Ej: "Items" con tipo "Course" → "CourseIds"
		/// </summary>
		private static string BuildCollectionIdName(string currentName, string typeName)
		{
			// Si el nombre actual es el tipo en plural (ej: Courses → Course + s)
			if (currentName.Equals(typeName + "s", System.StringComparison.OrdinalIgnoreCase) ||
				currentName.Equals(typeName + "es", System.StringComparison.OrdinalIgnoreCase))
				return typeName + "Ids";

			// Si el nombre ya termina en "Ids", dejarlo
			if (currentName.EndsWith("Ids"))
				return currentName;

			// Fallback: tipo + "Ids"
			return typeName + "Ids";
		}

		/// <summary>
		/// Agrega "using System" si el tipo lo requiere (Guid, DateTime, etc.).
		/// </summary>
		private static SyntaxNode EnsureUsingDirective(SyntaxNode root, string typeName)
		{
			var systemTypes = new[] { "Guid", "DateTime", "DateTimeOffset", "TimeSpan", "Decimal" };
			if (!systemTypes.Contains(typeName)) return root;

			var compilationUnit = root as CompilationUnitSyntax;
			if (compilationUnit == null) return root;

			if (compilationUnit.Usings.Any(u => u.Name.ToString() == "System"))
				return root;

			var usingDirective = SyntaxFactory.UsingDirective(
				SyntaxFactory.ParseName("System"))
				.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

			return compilationUnit.AddUsings(usingDirective);
		}

		/// <summary>
		/// Elimina el using del BC ajeno si el tipo referenciado ya no aparece en el archivo.
		/// </summary>
		private static SyntaxNode TryRemoveOrphanedUsing(SyntaxNode root, INamedTypeSymbol referencedType)
		{
			var compilationUnit = root as CompilationUnitSyntax;
			if (compilationUnit == null) return root;

			var referencedNamespace = referencedType.ContainingNamespace?.ToDisplayString();
			if (string.IsNullOrEmpty(referencedNamespace)) return root;

			var usingToRemove = compilationUnit.Usings
				.FirstOrDefault(u => u.Name.ToString() == referencedNamespace);

			if (usingToRemove == null) return root;

			// Si el nombre del tipo sigue en el código, no eliminar
			if (root.ToFullString().Contains(referencedType.Name))
				return root;

			return compilationUnit.RemoveNode(usingToRemove, SyntaxRemoveOptions.KeepLeadingTrivia);
		}
	}
}
