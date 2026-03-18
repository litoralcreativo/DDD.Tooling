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
	/// Code Fix para DDD017: AggregateRoot decorado debe heredar de AggregateRoot&lt;TId&gt;
	/// Sugiere agregar la herencia de AggregateRoot&lt;Guid&gt;
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AggregateRootInheritanceCodeFixProvider)), Shared]
	public class AggregateRootInheritanceCodeFixProvider : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds =>
			ImmutableArray.Create("DDD017");

		public sealed override FixAllProvider GetFixAllProvider() =>
			WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			var classDeclaration = root.FindToken(diagnosticSpan.Start)
				.Parent.AncestorsAndSelf()
				.OfType<ClassDeclarationSyntax>()
				.First();

			// Si ya tiene base class, no sugerimos porque puede ser más complejo
			if (classDeclaration.BaseList != null && classDeclaration.BaseList.Types.Any())
				return;

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Heredar de AggregateRoot<Guid>",
					createChangedDocument: c => AddAggregateRootInheritanceAsync(
						context.Document, classDeclaration, c),
					equivalenceKey: nameof(AggregateRootInheritanceCodeFixProvider)),
				diagnostic);
		}

		private static async Task<Document> AddAggregateRootInheritanceAsync(
			Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
		{
			var baseType = SyntaxFactory.SimpleBaseType(
				SyntaxFactory.GenericName(
					SyntaxFactory.Identifier("AggregateRoot"))
				.WithTypeArgumentList(
					SyntaxFactory.TypeArgumentList(
						SyntaxFactory.SingletonSeparatedList<TypeSyntax>(
							SyntaxFactory.IdentifierName("Guid")))));

			var baseList = SyntaxFactory.BaseList(
				SyntaxFactory.SingletonSeparatedList(baseType as BaseTypeSyntax));

			var newClassDeclaration = classDeclaration.WithBaseList(baseList);

			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);

			return document.WithSyntaxRoot(newRoot);
		}
	}
}
