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
	/// Code Fix para DDD019: DomainEvent decorado debe implementar IDomainEvent
	/// Sugiere agregar la implementación de la interfaz IDomainEvent
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DomainEventInterfaceCodeFixProvider)), Shared]
	public class DomainEventInterfaceCodeFixProvider : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds =>
			ImmutableArray.Create("DDD019");

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

			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Implementar IDomainEvent",
					createChangedDocument: c => AddIDomainEventImplementationAsync(
						context.Document, classDeclaration, c),
					equivalenceKey: nameof(DomainEventInterfaceCodeFixProvider)),
				diagnostic);
		}

		private static async Task<Document> AddIDomainEventImplementationAsync(
			Document document, ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken)
		{
			var interfaceType = SyntaxFactory.SimpleBaseType(
				SyntaxFactory.IdentifierName("IDomainEvent"));

			BaseListSyntax baseList;
			if (classDeclaration.BaseList == null)
			{
				baseList = SyntaxFactory.BaseList(
					SyntaxFactory.SingletonSeparatedList(interfaceType as BaseTypeSyntax));
			}
			else
			{
				baseList = classDeclaration.BaseList.AddTypes(interfaceType as BaseTypeSyntax);
			}

			var newClassDeclaration = classDeclaration.WithBaseList(baseList);

			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);

			return document.WithSyntaxRoot(newRoot);
		}
	}
}
