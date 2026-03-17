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
	/// Code Fix para DDD010: ofrece dos opciones al desarrollador:
	/// 1. Agregar [BoundedContext("NombreBC")] inferido desde el namespace
	/// 2. Agregar [SharedKernel]
	/// Code Fix para DDD016: mismo comportamiento para DomainEvents.
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BoundedContextDeclarationCodeFixProvider)), Shared]
	public class BoundedContextDeclarationCodeFixProvider : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds =>
			ImmutableArray.Create("DDD010", "DDD016");

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

			// Inferir el nombre del BC desde el namespace (último segmento relevante)
			var namespaceName = GetNamespaceName(classDeclaration);
			var inferredBcName = InferBoundedContextName(namespaceName);

			// Opción 1: [BoundedContext("NombreInferido")]
			var bcTitle = inferredBcName != null
				? $"Agregar [BoundedContext(\"{inferredBcName}\")]"
				: "Agregar [BoundedContext(\"NombreBC\")]";

			context.RegisterCodeFix(
				CodeAction.Create(
					title: bcTitle,
					createChangedDocument: c => AddAttributeAsync(
						context.Document, classDeclaration, "BoundedContext",
						inferredBcName ?? "NombreBC", c),
					equivalenceKey: nameof(BoundedContextDeclarationCodeFixProvider) + "_BC"),
				diagnostic);

			// Opción 2: [SharedKernel]
			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Agregar [SharedKernel]",
					createChangedDocument: c => AddSharedKernelAttributeAsync(
						context.Document, classDeclaration, c),
					equivalenceKey: nameof(BoundedContextDeclarationCodeFixProvider) + "_SK"),
				diagnostic);
		}

		private async Task<Document> AddAttributeAsync(
			Document document,
			ClassDeclarationSyntax classDeclaration,
			string attributeName,
			string argumentValue,
			CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			// Construir: [BoundedContext("NombreBC")]
			var argument = SyntaxFactory.AttributeArgument(
				SyntaxFactory.LiteralExpression(
					SyntaxKind.StringLiteralExpression,
					SyntaxFactory.Literal(argumentValue)));

			var attribute = SyntaxFactory.Attribute(
				SyntaxFactory.IdentifierName(attributeName),
				SyntaxFactory.AttributeArgumentList(
					SyntaxFactory.SingletonSeparatedList(argument)));

			var attributeList = SyntaxFactory.AttributeList(
				SyntaxFactory.SingletonSeparatedList(attribute))
				.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

			// Insertar como primer atributo de la clase
			var newClassDeclaration = classDeclaration
				.WithAttributeLists(classDeclaration.AttributeLists.Insert(0, attributeList))
				.WithAdditionalAnnotations(Formatter.Annotation);

			// Agregar using DDD.Abstractions si no existe
			var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);
			newRoot = EnsureUsingDirective(newRoot, "DDD.Abstractions");

			return document.WithSyntaxRoot(newRoot);
		}

		private async Task<Document> AddSharedKernelAttributeAsync(
			Document document,
			ClassDeclarationSyntax classDeclaration,
			CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			// Construir: [SharedKernel]
			var attribute = SyntaxFactory.Attribute(
				SyntaxFactory.IdentifierName("SharedKernel"));

			var attributeList = SyntaxFactory.AttributeList(
				SyntaxFactory.SingletonSeparatedList(attribute))
				.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

			var newClassDeclaration = classDeclaration
				.WithAttributeLists(classDeclaration.AttributeLists.Insert(0, attributeList))
				.WithAdditionalAnnotations(Formatter.Annotation);

			var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);
			newRoot = EnsureUsingDirective(newRoot, "DDD.Abstractions");

			return document.WithSyntaxRoot(newRoot);
		}

		/// <summary>
		/// Infiere el nombre del BC desde el namespace.
		/// Ej: "MyApp.Catalog.Aggregates" → "Catalog"
		///     "MyApp.StudentManagement"  → "StudentManagement"
		///     "MyApp.SharedKernel"       → "SharedKernel"
		/// Toma el primer segmento después del namespace raíz (el de mayor nivel que no sea el último).
		/// </summary>
		private static string InferBoundedContextName(string namespaceName)
		{
			if (string.IsNullOrEmpty(namespaceName))
				return null;

			var parts = namespaceName.Split('.');

			// Necesitamos al menos 2 segmentos: "RootNS.BoundedContext"
			if (parts.Length < 2)
				return null;

			// El BC es el segundo segmento (índice 1), después del namespace raíz
			return parts[1];
		}

		private static string GetNamespaceName(ClassDeclarationSyntax classDeclaration)
		{
			var namespaceDeclaration = classDeclaration
				.Ancestors()
				.OfType<NamespaceDeclarationSyntax>()
				.FirstOrDefault();

			return namespaceDeclaration?.Name?.ToString();
		}

		private static SyntaxNode EnsureUsingDirective(SyntaxNode root, string namespaceName)
		{
			var compilationUnit = root as CompilationUnitSyntax;
			if (compilationUnit == null)
				return root;

			var alreadyExists = compilationUnit.Usings
				.Any(u => u.Name.ToString() == namespaceName);

			if (alreadyExists)
				return root;

			var usingDirective = SyntaxFactory.UsingDirective(
				SyntaxFactory.ParseName(namespaceName))
				.WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

			return compilationUnit.WithUsings(
				compilationUnit.Usings.Add(usingDirective));
		}
	}
}
