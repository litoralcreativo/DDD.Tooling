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
	/// Code Fix para agregar un Factory Method estático Create y hacer privado el constructor público
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EntityFactoryMethodCodeFixProvider)), Shared]
	public class EntityFactoryMethodCodeFixProvider : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds =>
			ImmutableArray.Create("DDD009");

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
					title: "Agregar Factory Method 'Create' y hacer el constructor privado",
					createChangedDocument: c => AddFactoryMethodAsync(context.Document, classDeclaration, c),
					equivalenceKey: nameof(EntityFactoryMethodCodeFixProvider)),
				diagnostic);
		}

		private async Task<Document> AddFactoryMethodAsync(
			Document document,
			ClassDeclarationSyntax classDeclaration,
			CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken);
			if (classSymbol == null)
				return document;

			// Obtener el primer constructor público no implícito
			var publicConstructor = classDeclaration.Members
				.OfType<ConstructorDeclarationSyntax>()
				.FirstOrDefault(c => c.Modifiers.Any(SyntaxKind.PublicKeyword));

			if (publicConstructor == null)
				return document;

			// Paso 1: hacer privado el constructor público
			var privateConstructor = MakeConstructorPrivate(publicConstructor);

			// Paso 2: generar el Factory Method Create con los mismos parámetros
			var factoryMethod = GenerateFactoryMethod(classDeclaration.Identifier.Text, publicConstructor);

			// Encontrar el índice del constructor público original ANTES de reemplazarlo
			var constructorIndex = classDeclaration.Members.IndexOf(publicConstructor);

			// Reemplazar el constructor público por el privado
			var newMembers = classDeclaration.Members
				.Replace(publicConstructor, privateConstructor);

			// Insertar el factory method justo antes del constructor (índice original)
			newMembers = newMembers.Insert(constructorIndex, factoryMethod);

			var newClassDeclaration = classDeclaration
				.WithMembers(newMembers)
				.WithAdditionalAnnotations(Formatter.Annotation);

			var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);
			return document.WithSyntaxRoot(newRoot);
		}

		private static ConstructorDeclarationSyntax MakeConstructorPrivate(ConstructorDeclarationSyntax constructor)
		{
			// Eliminar el modificador public y reemplazarlo con private
			var newModifiers = constructor.Modifiers
				.Where(m => !m.IsKind(SyntaxKind.PublicKeyword))
				.ToList();

			var privateModifier = SyntaxFactory.Token(SyntaxKind.PrivateKeyword)
				.WithTrailingTrivia(SyntaxFactory.Space);

			newModifiers.Insert(0, privateModifier);

			return constructor.WithModifiers(SyntaxFactory.TokenList(newModifiers));
		}

		private static MethodDeclarationSyntax GenerateFactoryMethod(
			string className,
			ConstructorDeclarationSyntax constructor)
		{
			// Construir la lista de parámetros igual que la del constructor
			var parameters = constructor.ParameterList;

			// Construir los argumentos para llamar al constructor (mismos nombres de parámetros)
			var arguments = SyntaxFactory.ArgumentList(
				SyntaxFactory.SeparatedList(
					parameters.Parameters.Select(p =>
						SyntaxFactory.Argument(SyntaxFactory.IdentifierName(p.Identifier)))));

			// new ClassName(params)
			var objectCreation = SyntaxFactory.ObjectCreationExpression(
				SyntaxFactory.IdentifierName(className),
				arguments,
				null);

			// return new ClassName(params);
			var returnStatement = SyntaxFactory.ReturnStatement(objectCreation)
				.WithLeadingTrivia(SyntaxFactory.Whitespace("\t\t\t"));

			// Cuerpo del método
			var body = SyntaxFactory.Block(returnStatement);

			// public static ClassName Create(params) { return new ClassName(params); }
			var method = SyntaxFactory.MethodDeclaration(
					SyntaxFactory.IdentifierName(className),
					SyntaxFactory.Identifier("Create"))
				.WithModifiers(SyntaxFactory.TokenList(
					SyntaxFactory.Token(SyntaxKind.PublicKeyword)
						.WithTrailingTrivia(SyntaxFactory.Space),
					SyntaxFactory.Token(SyntaxKind.StaticKeyword)
						.WithTrailingTrivia(SyntaxFactory.Space)))
				.WithParameterList(parameters)
				.WithBody(body)
				.WithAdditionalAnnotations(Formatter.Annotation);

			return method;
		}
	}
}
