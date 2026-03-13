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
	/// Code Fix para implementar correctamente el patrón Factory Method en Entities:
	/// constructor privado + método estático público que devuelva la instancia.
	/// Cubre tres escenarios:
	/// 1. Constructor público sin factory method → hace privado el constructor y agrega Create estático
	/// 2. Constructor privado sin factory method estático → agrega Create estático
	/// 3. Método Create existente pero no estático → le agrega el modificador static
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

			var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
			var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, context.CancellationToken);
			if (classSymbol == null)
				return;

			// Detectar si hay un método de instancia (no estático) que devuelva la clase
			var nonStaticFactoryMethod = classDeclaration.Members
				.OfType<MethodDeclarationSyntax>()
				.FirstOrDefault(m =>
					!m.Modifiers.Any(SyntaxKind.StaticKeyword) &&
					m.Modifiers.Any(SyntaxKind.PublicKeyword) &&
					m.ReturnType.ToString() == classDeclaration.Identifier.Text);

			if (nonStaticFactoryMethod != null)
			{
				// Caso 3: existe un método Create no estático → agregar static
				context.RegisterCodeFix(
					CodeAction.Create(
						title: $"Hacer estático el método '{nonStaticFactoryMethod.Identifier.Text}'",
						createChangedDocument: c => MakeFactoryMethodStaticAsync(context.Document, classDeclaration, nonStaticFactoryMethod, c),
						equivalenceKey: nameof(EntityFactoryMethodCodeFixProvider) + "_MakeStatic"),
					diagnostic);
			}

			var publicConstructor = classDeclaration.Members
				.OfType<ConstructorDeclarationSyntax>()
				.FirstOrDefault(c => c.Modifiers.Any(SyntaxKind.PublicKeyword));

			var anyConstructor = classDeclaration.Members
				.OfType<ConstructorDeclarationSyntax>()
				.FirstOrDefault();

			// Caso 1 y 2: agregar factory method Create + hacer privado el constructor si es público
			var constructorForFactory = publicConstructor ?? anyConstructor;
			if (constructorForFactory != null)
			{
				var title = publicConstructor != null
					? "Agregar Factory Method 'Create' estático y hacer el constructor privado"
					: "Agregar Factory Method 'Create' estático";

				context.RegisterCodeFix(
					CodeAction.Create(
						title: title,
						createChangedDocument: c => AddFactoryMethodAsync(context.Document, classDeclaration, constructorForFactory, publicConstructor != null, c),
						equivalenceKey: nameof(EntityFactoryMethodCodeFixProvider) + "_AddFactory"),
					diagnostic);
			}
		}

		private async Task<Document> MakeFactoryMethodStaticAsync(
			Document document,
			ClassDeclarationSyntax classDeclaration,
			MethodDeclarationSyntax method,
			CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			// Agregar modificador static después de public
			var staticToken = SyntaxFactory.Token(SyntaxKind.StaticKeyword)
				.WithTrailingTrivia(SyntaxFactory.Space);

			var newModifiers = method.Modifiers.Add(staticToken);
			var newMethod = method.WithModifiers(newModifiers)
				.WithAdditionalAnnotations(Formatter.Annotation);

			var newRoot = root.ReplaceNode(method, newMethod);
			return document.WithSyntaxRoot(newRoot);
		}

		private async Task<Document> AddFactoryMethodAsync(
			Document document,
			ClassDeclarationSyntax classDeclaration,
			ConstructorDeclarationSyntax targetConstructor,
			bool makeConstructorPrivate,
			CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

			// Generar el Factory Method Create con los parámetros del constructor
			var factoryMethod = GenerateFactoryMethod(classDeclaration.Identifier.Text, targetConstructor);

			// Encontrar el índice del constructor ANTES de cualquier reemplazo
			var constructorIndex = classDeclaration.Members.IndexOf(targetConstructor);

			var newMembers = classDeclaration.Members;

			if (makeConstructorPrivate)
			{
				// Reemplazar el constructor público por uno privado
				var privateConstructor = MakeConstructorPrivate(targetConstructor);
				newMembers = newMembers.Replace(targetConstructor, privateConstructor);
			}

			// Insertar el factory method justo antes del constructor
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
