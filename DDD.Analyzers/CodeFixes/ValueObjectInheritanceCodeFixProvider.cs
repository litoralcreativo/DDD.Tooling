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
	/// Code Fix para DDD020: ValueObject decorado debe heredar de ValueObject
	/// Sugiere agregar la herencia de ValueObject
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ValueObjectInheritanceCodeFixProvider)), Shared]
	public class ValueObjectInheritanceCodeFixProvider : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds =>
			ImmutableArray.Create("DDD020");

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
					title: "Heredar de ValueObject",
					createChangedDocument: c => AddValueObjectInheritanceAsync(
						context.Document, classDeclaration, c),
					equivalenceKey: nameof(ValueObjectInheritanceCodeFixProvider)),
				diagnostic);
		}

		private static async Task<Document> AddValueObjectInheritanceAsync(
			Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
		{
			var baseType = SyntaxFactory.SimpleBaseType(
				SyntaxFactory.IdentifierName("ValueObject"));

			var baseList = SyntaxFactory.BaseList(
				SyntaxFactory.SingletonSeparatedList(baseType as BaseTypeSyntax));

			var newClassDeclaration = classDeclaration.WithBaseList(baseList);

			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);

			return document.WithSyntaxRoot(newRoot);
		}
	}
}
