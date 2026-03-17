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
	/// Code Fix para DDD015: agrega automáticamente la propiedad OccurredOn
	/// de tipo DateTime a un DomainEvent.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DomainEventOccurredOnCodeFixProvider)), Shared]
	public class DomainEventOccurredOnCodeFixProvider : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds =>
			ImmutableArray.Create("DDD015");

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			if (root == null) return;

			var diagnostic = context.Diagnostics.First();
			var span = diagnostic.Location.SourceSpan;

			var classDeclaration = root.FindToken(span.Start)
				.Parent?.AncestorsAndSelf()
				.OfType<ClassDeclarationSyntax>()
				.FirstOrDefault();

			if (classDeclaration == null) return;

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Agregar propiedad OccurredOn (DateTime)",
					createChangedDocument: ct => AddOccurredOnPropertyAsync(context.Document, classDeclaration, ct),
					equivalenceKey: "DDD015_OccurredOn"),
				diagnostic);
		}

		private static async Task<Document> AddOccurredOnPropertyAsync(
			Document document,
			ClassDeclarationSyntax classDeclaration,
			CancellationToken ct)
		{
			var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
			if (root == null) return document;

			var newProperty = GenerateOccurredOnProperty();
			if (newProperty == null) return document;

			// Inserta la propiedad como primer miembro de la clase
			var newMembers = classDeclaration.Members.Insert(0, newProperty);
			var newClass = classDeclaration
				.WithMembers(newMembers)
				.WithAdditionalAnnotations(Formatter.Annotation);

			var rootWithClass = root.ReplaceNode(classDeclaration, newClass);

			// Agrega 'using System;' si no existe
			var compilationUnit = rootWithClass as CompilationUnitSyntax;
			if (compilationUnit != null)
			{
				var hasSystemUsing = compilationUnit.Usings
					.Any(u => u.Name?.ToString() == "System");

				if (!hasSystemUsing)
				{
					var systemUsing = SyntaxFactory
						.UsingDirective(SyntaxFactory.ParseName("System"))
						.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

					rootWithClass = compilationUnit.WithUsings(
						compilationUnit.Usings.Insert(0, systemUsing));
				}
			}

			return document.WithSyntaxRoot(rootWithClass);
		}

		private static PropertyDeclarationSyntax GenerateOccurredOnProperty()
		{
			const string code = "\n\tpublic DateTime OccurredOn { get; }\n";
			return SyntaxFactory.ParseMemberDeclaration(code) as PropertyDeclarationSyntax;
		}
	}
}
