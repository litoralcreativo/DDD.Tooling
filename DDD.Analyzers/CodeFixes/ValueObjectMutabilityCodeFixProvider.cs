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

namespace DDD.Analyzers.CodeFixes
{
	/// <summary>
	/// Code Fix para DDD004: convierte setter público a private set o init.
	/// Code Fix para DDD005: elimina [Entity] o [ValueObject] cuando coexisten.
	/// Code Fix para DDD006: elimina [AggregateRoot] o [ValueObject] cuando coexisten.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ValueObjectMutabilityCodeFixProvider)), Shared]
	public class ValueObjectMutabilityCodeFixProvider : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds =>
			ImmutableArray.Create("DDD004", "DDD005", "DDD006");

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			if (root == null) return;

			var diagnostic = context.Diagnostics.First();
			var span = diagnostic.Location.SourceSpan;

			// ── DDD004 ────────────────────────────────────────────────────────────
			if (diagnostic.Id == "DDD004")
			{
				var property = root.FindToken(span.Start)
					.Parent?.AncestorsAndSelf()
					.OfType<PropertyDeclarationSyntax>()
					.FirstOrDefault();

				if (property == null) return;

				context.RegisterCodeFix(
					CodeAction.Create(
						title: "Cambiar a 'private set'",
						createChangedDocument: ct => MakePrivateSetAsync(context.Document, property, ct),
						equivalenceKey: "DDD004_PrivateSet"),
					diagnostic);

				context.RegisterCodeFix(
					CodeAction.Create(
						title: "Cambiar a 'init' (C# 9+)",
						createChangedDocument: ct => MakeInitSetAsync(context.Document, property, ct),
						equivalenceKey: "DDD004_Init"),
					diagnostic);
			}

			// ── DDD005 ────────────────────────────────────────────────────────────
			else if (diagnostic.Id == "DDD005")
			{
				var classDecl = root.FindToken(span.Start)
					.Parent?.AncestorsAndSelf()
					.OfType<ClassDeclarationSyntax>()
					.FirstOrDefault();

				if (classDecl == null) return;

				context.RegisterCodeFix(
					CodeAction.Create(
						title: "Mantener como Entity (quitar [ValueObject])",
						createChangedDocument: ct => RemoveAttributeAsync(context.Document, classDecl, "ValueObject", ct),
						equivalenceKey: "DDD005_KeepEntity"),
					diagnostic);

				context.RegisterCodeFix(
					CodeAction.Create(
						title: "Mantener como ValueObject (quitar [Entity])",
						createChangedDocument: ct => RemoveAttributeAsync(context.Document, classDecl, "Entity", ct),
						equivalenceKey: "DDD005_KeepValueObject"),
					diagnostic);
			}

			// ── DDD006 ────────────────────────────────────────────────────────────
			else if (diagnostic.Id == "DDD006")
			{
				var classDecl = root.FindToken(span.Start)
					.Parent?.AncestorsAndSelf()
					.OfType<ClassDeclarationSyntax>()
					.FirstOrDefault();

				if (classDecl == null) return;

				context.RegisterCodeFix(
					CodeAction.Create(
						title: "Mantener como AggregateRoot (quitar [ValueObject])",
						createChangedDocument: ct => RemoveAttributeAsync(context.Document, classDecl, "ValueObject", ct),
						equivalenceKey: "DDD006_KeepAggregateRoot"),
					diagnostic);

				context.RegisterCodeFix(
					CodeAction.Create(
						title: "Mantener como ValueObject (quitar [AggregateRoot])",
						createChangedDocument: ct => RemoveAttributeAsync(context.Document, classDecl, "AggregateRoot", ct),
						equivalenceKey: "DDD006_KeepValueObject"),
					diagnostic);
			}
		}

		// ── DDD004: private set ────────────────────────────────────────────────────

		private static Task<Document> MakePrivateSetAsync(
			Document document,
			PropertyDeclarationSyntax property,
			CancellationToken ct)
		{
			return ReplaceSetterAsync(document, property, useInit: false, ct);
		}

		private static Task<Document> MakeInitSetAsync(
			Document document,
			PropertyDeclarationSyntax property,
			CancellationToken ct)
		{
			return ReplaceSetterAsync(document, property, useInit: true, ct);
		}

		private static async Task<Document> ReplaceSetterAsync(
			Document document,
			PropertyDeclarationSyntax property,
			bool useInit,
			CancellationToken ct)
		{
			var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);

			var setter = property.AccessorList?.Accessors
				.FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));

			if (setter == null) return document;

			AccessorDeclarationSyntax newAccessor;

			if (useInit)
			{
				// init; — sin modificadores, sintaxis nativa de C# 9
				newAccessor = SyntaxFactory
					.AccessorDeclaration(SyntaxKind.InitAccessorDeclaration)
					.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
					.WithLeadingTrivia(setter.GetLeadingTrivia())
					.WithTrailingTrivia(setter.GetTrailingTrivia());
			}
			else
			{
				// private set; — agrega el modificador "private" al setter existente
				var privateToken = SyntaxFactory
					.Token(SyntaxKind.PrivateKeyword)
					.WithTrailingTrivia(SyntaxFactory.Space);

				newAccessor = setter
					.WithModifiers(SyntaxFactory.TokenList(privateToken));
			}

			var newAccessorList = property.AccessorList.ReplaceNode(setter, newAccessor);
			var newProperty = property.WithAccessorList(newAccessorList);

			return document.WithSyntaxRoot(root.ReplaceNode(property, newProperty));
		}

		// ── DDD005 / DDD006: quitar atributo ──────────────────────────────────────

		private static async Task<Document> RemoveAttributeAsync(
			Document document,
			ClassDeclarationSyntax classDecl,
			string attributeShortName,
			CancellationToken ct)
		{
			var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);

			// Buscar la lista de atributos que contiene el atributo a eliminar
			AttributeListSyntax targetList = null;
			AttributeSyntax targetAttr = null;

			foreach (var attrList in classDecl.AttributeLists)
			{
				foreach (var attr in attrList.Attributes)
				{
					var name = attr.Name.ToString();
					// Acepta "Entity", "EntityAttribute", "ValueObject", etc.
					if (name == attributeShortName || name == attributeShortName + "Attribute")
					{
						targetList = attrList;
						targetAttr = attr;
						break;
					}
				}
				if (targetAttr != null) break;
			}

			if (targetAttr == null) return document;

			SyntaxNode newRoot;

			if (targetList.Attributes.Count == 1)
			{
				// La lista solo tiene este atributo → eliminar toda la lista [Xxx]
				newRoot = root.RemoveNode(targetList, SyntaxRemoveOptions.KeepLeadingTrivia);
			}
			else
			{
				// La lista tiene varios atributos → eliminar solo este
				var newList = targetList.RemoveNode(targetAttr, SyntaxRemoveOptions.KeepLeadingTrivia);
				newRoot = root.ReplaceNode(targetList, newList);
			}

			return document.WithSyntaxRoot(newRoot);
		}
	}
}
