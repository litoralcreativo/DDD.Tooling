using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DDD.Analyzers
{
	/// <summary>
	/// DDD010: Detecta clases [Entity], [AggregateRoot] o [ValueObject] que no tienen [BoundedContext].
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class BoundedContextDeclarationAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(DiagnosticDescriptors.DddTypeMustDeclareBoundedContext);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
		}

		private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
		{
			var classDeclaration = (ClassDeclarationSyntax)context.Node;
			var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);

			if (classSymbol == null)
				return;

			var attributes = classSymbol.GetAttributes();

			// Detectar qué atributo DDD tiene
			var dddAttribute = attributes.FirstOrDefault(a =>
				a.AttributeClass?.Name == "EntityAttribute" ||
				a.AttributeClass?.Name == "AggregateRootAttribute" ||
				a.AttributeClass?.Name == "ValueObjectAttribute");

			if (dddAttribute == null)
				return;

			// [SharedKernel] exime de declarar [BoundedContext]
			var isSharedKernel = attributes
				.Any(a => a.AttributeClass?.Name == "SharedKernelAttribute");

			if (isSharedKernel)
				return;

			// Verificar si tiene [BoundedContext]
			var hasBoundedContext = attributes
				.Any(a => a.AttributeClass?.Name == "BoundedContextAttribute");

			if (hasBoundedContext)
				return;

			// Obtener el nombre del atributo DDD para el mensaje (sin "Attribute")
			var attributeName = dddAttribute.AttributeClass.Name.Replace("Attribute", "");

			var diagnostic = Diagnostic.Create(
				DiagnosticDescriptors.DddTypeMustDeclareBoundedContext,
				classDeclaration.Identifier.GetLocation(),
				classSymbol.Name,
				attributeName);

			context.ReportDiagnostic(diagnostic);
		}
	}
}
