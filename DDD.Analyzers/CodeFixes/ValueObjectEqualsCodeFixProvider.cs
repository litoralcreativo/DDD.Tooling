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
	/// Code Fix para agregar automáticamente Equals y GetHashCode a ValueObjects
	/// </summary>
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ValueObjectEqualsCodeFixProvider)), Shared]
	public class ValueObjectEqualsCodeFixProvider : CodeFixProvider
	{
		public sealed override ImmutableArray<string> FixableDiagnosticIds =>
			ImmutableArray.Create("DDD007", "DDD008");

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

			// Registrar Code Fix para agregar Equals y GetHashCode
			context.RegisterCodeFix(
				CodeAction.Create(
					title: "Agregar Equals y GetHashCode",
					createChangedDocument: c => AddEqualsAndGetHashCodeAsync(context.Document, classDeclaration, c),
					equivalenceKey: nameof(ValueObjectEqualsCodeFixProvider)),
				diagnostic);
		}

		private async Task<Document> AddEqualsAndGetHashCodeAsync(
			Document document,
			ClassDeclarationSyntax classDeclaration,
			CancellationToken cancellationToken)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken);

			if (classSymbol == null)
				return document;

			// Obtener todas las propiedades públicas de solo lectura
			var properties = classSymbol.GetMembers()
				.OfType<IPropertySymbol>()
				.Where(p => p.DeclaredAccessibility == Accessibility.Public && p.SetMethod == null)
				.ToList();

			var newMembers = classDeclaration.Members.ToList();

			// Verificar si ya tiene Equals
			var hasEquals = classSymbol.GetMembers("Equals")
				.OfType<IMethodSymbol>()
				.Any(m => m.IsOverride && m.Parameters.Length == 1);

			if (!hasEquals)
			{
				var equalsMethod = GenerateEqualsMethod(classSymbol.Name, properties);
				newMembers.Add(equalsMethod);
			}

			// Verificar si ya tiene GetHashCode
			var hasGetHashCode = classSymbol.GetMembers("GetHashCode")
				.OfType<IMethodSymbol>()
				.Any(m => m.IsOverride && m.Parameters.Length == 0);

			if (!hasGetHashCode)
			{
				var getHashCodeMethod = GenerateGetHashCodeMethod(properties);
				newMembers.Add(getHashCodeMethod);
			}

			var newClassDeclaration = classDeclaration.WithMembers(SyntaxFactory.List(newMembers))
				.WithAdditionalAnnotations(Formatter.Annotation);

			var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);
			return document.WithSyntaxRoot(newRoot);
		}

		private MethodDeclarationSyntax GenerateEqualsMethod(string className, System.Collections.Generic.List<IPropertySymbol> properties)
		{
			var propertyComparisons = string.Join(" && ",
				properties.Select(p => $"{p.Name} == other.{p.Name}"));

			if (string.IsNullOrEmpty(propertyComparisons))
				propertyComparisons = "true";

			var code = $@"
		public override bool Equals(object obj)
		{{
			if (obj is {className} other)
			{{
				return {propertyComparisons};
			}}
			return false;
		}}";

			return SyntaxFactory.ParseMemberDeclaration(code) as MethodDeclarationSyntax;
		}

		private MethodDeclarationSyntax GenerateGetHashCodeMethod(System.Collections.Generic.List<IPropertySymbol> properties)
		{
			var hashCombinations = string.Join("\n\t\t\t\t",
				properties.Select(p => GenerateHashCodeForProperty(p)));

			if (string.IsNullOrEmpty(hashCombinations))
				hashCombinations = "hash = hash * 23;";

			var code = $@"
		public override int GetHashCode()
		{{
			unchecked
			{{
				int hash = 17;
				{hashCombinations}
				return hash;
			}}
		}}";

			return SyntaxFactory.ParseMemberDeclaration(code) as MethodDeclarationSyntax;
		}

		private string GenerateHashCodeForProperty(IPropertySymbol property)
		{
			// Verificar si es un tipo de valor (struct) o nullable
			var typeInfo = property.Type;

			// Si es un tipo nullable (int?, decimal?, etc.)
			if (typeInfo.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
			{
				return $"hash = hash * 23 + ({property.Name}?.GetHashCode() ?? 0);";
			}

			// Si es un tipo de valor no nullable (int, decimal, DateTime, etc.)
			if (typeInfo.IsValueType)
			{
				return $"hash = hash * 23 + {property.Name}.GetHashCode();";
			}

			// Si es un tipo de referencia (string, clases, etc.)
			return $"hash = hash * 23 + ({property.Name}?.GetHashCode() ?? 0);";
		}
	}
}
