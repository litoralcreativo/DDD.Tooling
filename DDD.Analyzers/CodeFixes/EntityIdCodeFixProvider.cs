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
	/// Code Fix para agregar automáticamente propiedad Id con [EntityId] a Entity y AggregateRoot
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EntityIdCodeFixProvider)), Shared]
	public class EntityIdCodeFixProvider : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds =>
			ImmutableArray.Create("DDD001", "DDD002");

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

			// Registrar Code Fix para agregar propiedad Id con [EntityId]
			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Agregar propiedad Id con [EntityId]",
					createChangedDocument: c => AddEntityIdPropertyAsync(context.Document, classDeclaration, c),
					equivalenceKey: nameof(EntityIdCodeFixProvider)),
				diagnostic);
		}

		private async Task<Document> AddEntityIdPropertyAsync(
			Document document,
			ClassDeclarationSyntax classDeclaration,
			CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			// Verificar si ya tiene alguna propiedad con [EntityId]
			var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken);
			if (classSymbol == null)
				return document;

			var hasEntityIdProperty = classSymbol.GetMembers()
				.OfType<IPropertySymbol>()
				.Any(p => p.GetAttributes().Any(a => a.AttributeClass?.Name == "EntityIdAttribute"));

			if (hasEntityIdProperty)
				return document; // Ya tiene EntityId, no hacer nada

			// Generar la nueva propiedad Id con [EntityId]
			var newProperty = GenerateEntityIdProperty();

			// Agregar la propiedad al inicio de la clase (después de los campos si existen)
			var newMembers = classDeclaration.Members.Insert(0, newProperty);
			var newClassDeclaration = classDeclaration.WithMembers(newMembers)
				.WithAdditionalAnnotations(Formatter.Annotation);

			// Verificar si necesitamos agregar usings
			var needsDddUsing = !root.DescendantNodes()
				.OfType<UsingDirectiveSyntax>()
				.Any(u => u.Name.ToString() == "DDD.Abstractions");

			var needsSystemUsing = !root.DescendantNodes()
				.OfType<UsingDirectiveSyntax>()
				.Any(u => u.Name.ToString() == "System");

			// Primero reemplazamos la clase en el árbol original
			var rootWithClass = root.ReplaceNode(classDeclaration, newClassDeclaration);

			// Luego agregamos los usings si hace falta
			SyntaxNode newRoot;
			var compilationUnit = rootWithClass as CompilationUnitSyntax;
			if ((needsDddUsing || needsSystemUsing) && compilationUnit != null)
			{
				var newUsings = compilationUnit.Usings;
				if (needsSystemUsing)
				{
					newUsings = newUsings.Add(
						SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System"))
							.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
				}
				if (needsDddUsing)
				{
					newUsings = newUsings.Add(
						SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("DDD.Abstractions"))
							.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed));
				}
				newRoot = compilationUnit.WithUsings(newUsings);
			}
			else
			{
				newRoot = rootWithClass;
			}

			return document.WithSyntaxRoot(newRoot);
		}

		private PropertyDeclarationSyntax GenerateEntityIdProperty()
		{
			var code = @"
	[EntityId]
	public Guid Id { get; private set; }";

			return SyntaxFactory.ParseMemberDeclaration(code) as PropertyDeclarationSyntax;
		}
	}
}
