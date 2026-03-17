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
	/// Code Fix para DDD014: convierte el setter público de una propiedad
	/// de un DomainEvent a 'private set' o 'init'.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DomainEventImmutabilityCodeFixProvider)), Shared]
	public class DomainEventImmutabilityCodeFixProvider : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds =>
			ImmutableArray.Create("DDD014");

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			if (root == null) return;

			var diagnostic = context.Diagnostics.First();
			var span = diagnostic.Location.SourceSpan;

			var property = root.FindToken(span.Start)
				.Parent?.AncestorsAndSelf()
				.OfType<PropertyDeclarationSyntax>()
				.FirstOrDefault();

			if (property == null) return;

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Cambiar a 'private set'",
					createChangedDocument: ct => MakePrivateSetAsync(context.Document, property, ct),
					equivalenceKey: "DDD014_PrivateSet"),
				diagnostic);

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Cambiar a 'init' (C# 9+)",
					createChangedDocument: ct => MakeInitSetAsync(context.Document, property, ct),
					equivalenceKey: "DDD014_Init"),
				diagnostic);
		}

		private static Task<Document> MakePrivateSetAsync(
			Document document, PropertyDeclarationSyntax property, CancellationToken ct) =>
			ReplaceSetterAsync(document, property, useInit: false, ct);

		private static Task<Document> MakeInitSetAsync(
			Document document, PropertyDeclarationSyntax property, CancellationToken ct) =>
			ReplaceSetterAsync(document, property, useInit: true, ct);

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
				newAccessor = SyntaxFactory
					.AccessorDeclaration(SyntaxKind.InitAccessorDeclaration)
					.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
					.WithLeadingTrivia(setter.GetLeadingTrivia())
					.WithTrailingTrivia(setter.GetTrailingTrivia());
			}
			else
			{
				var privateToken = SyntaxFactory
					.Token(SyntaxKind.PrivateKeyword)
					.WithTrailingTrivia(SyntaxFactory.Space);

				newAccessor = setter.WithModifiers(SyntaxFactory.TokenList(privateToken));
			}

			var newAccessorList = property.AccessorList.ReplaceNode(setter, newAccessor);
			var newProperty = property.WithAccessorList(newAccessorList);

			return document.WithSyntaxRoot(root.ReplaceNode(property, newProperty));
		}
	}
}
