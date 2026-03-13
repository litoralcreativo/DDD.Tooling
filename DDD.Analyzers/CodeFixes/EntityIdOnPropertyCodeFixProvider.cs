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
	/// Code Fix para DDD003: convierte un campo con [EntityId] en una propiedad automática.
	/// Antes: [EntityId] private Guid _id;
	/// Después: [EntityId] public Guid Id { get; private set; }
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EntityIdOnPropertyCodeFixProvider)), Shared]
	public class EntityIdOnPropertyCodeFixProvider : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds =>
			ImmutableArray.Create("DDD003");

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			if (root == null) return;

			var diagnostic = context.Diagnostics.First();
			var span = diagnostic.Location.SourceSpan;

			var fieldDecl = root.FindToken(span.Start)
				.Parent?.AncestorsAndSelf()
				.OfType<FieldDeclarationSyntax>()
				.FirstOrDefault();

			if (fieldDecl == null) return;

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Convertir campo en propiedad con [EntityId]",
					createChangedDocument: ct => ConvertFieldToPropertyAsync(context.Document, fieldDecl, ct),
					equivalenceKey: "DDD003_ConvertToProperty"),
				diagnostic);
		}

		private static async Task<Document> ConvertFieldToPropertyAsync(
			Document document,
			FieldDeclarationSyntax fieldDecl,
			CancellationToken ct)
		{
			var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);

			// Puede haber varios declaradores: [EntityId] Guid _id, _otherId;
			// Generamos una propiedad por cada variable declarada
			var newNodes = fieldDecl.Declaration.Variables
				.Select(variable => BuildProperty(fieldDecl, variable))
				.ToArray();

			// Reemplazamos el campo por la primera propiedad y, si hay más, las insertamos a continuación
			SyntaxNode newRoot;
			if (newNodes.Length == 1)
			{
				newRoot = root.ReplaceNode(fieldDecl, newNodes[0]);
			}
			else
			{
				// Insertar todas las propiedades y quitar el campo original
				var parent = fieldDecl.Parent;
				var members = ((TypeDeclarationSyntax)parent).Members;
				var index = members.IndexOf(fieldDecl);

				var newMembers = members
					.RemoveAt(index)
					.InsertRange(index, newNodes);

				var newClass = ((TypeDeclarationSyntax)parent).WithMembers(newMembers);
				newRoot = root.ReplaceNode(parent, newClass);
			}

			return document.WithSyntaxRoot(newRoot);
		}

		private static PropertyDeclarationSyntax BuildProperty(
			FieldDeclarationSyntax fieldDecl,
			VariableDeclaratorSyntax variable)
		{
			// Nombre: _myId → MyId  |  myId → MyId  |  id → Id
			var rawName = variable.Identifier.Text.TrimStart('_');
			var propertyName = char.ToUpper(rawName[0]) + rawName.Substring(1);

			// Tipo del campo
			var typeSyntax = fieldDecl.Declaration.Type
				.WithoutLeadingTrivia()
				.WithoutTrailingTrivia();

			// Atributos: conservamos todos los que tenía el campo (incluido [EntityId])
			var attributeLists = fieldDecl.AttributeLists;

			// Accesores: { get; private set; }
			var accessorList = SyntaxFactory.AccessorList(
				SyntaxFactory.List(new[]
				{
					SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
						.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
					SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
						.WithModifiers(SyntaxFactory.TokenList(
							SyntaxFactory.Token(SyntaxKind.PrivateKeyword)))
						.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken))
				}));

			return SyntaxFactory
				.PropertyDeclaration(typeSyntax, propertyName)
				.WithAttributeLists(attributeLists)
				.WithModifiers(SyntaxFactory.TokenList(
					SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
				.WithAccessorList(accessorList)
				.WithLeadingTrivia(fieldDecl.GetLeadingTrivia())
				.WithTrailingTrivia(fieldDecl.GetTrailingTrivia())
				.WithAdditionalAnnotations(Formatter.Annotation);
		}
	}
}
