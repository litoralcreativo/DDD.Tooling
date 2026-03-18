using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace DDD.Analyzers
{
	/// <summary>
	/// Analizador DDD020: Valida que las clases decoradas con [ValueObject]
	/// hereden de la clase base ValueObject.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ValueObjectMustInheritFromBaseAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(DiagnosticDescriptors.ValueObjectMustInheritFromBase);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
		}

		private void AnalyzeSymbol(SymbolAnalysisContext context)
		{
			var classSymbol = (INamedTypeSymbol)context.Symbol;

			// Solo analizar clases y structs
			if (classSymbol.TypeKind != TypeKind.Class && classSymbol.TypeKind != TypeKind.Struct)
				return;

			// Verificar si la clase está decorada con [ValueObject]
			if (!HasValueObjectAttribute(classSymbol))
				return;

			// Verificar si hereda de ValueObject
			if (InheritsFromValueObjectBase(classSymbol))
				return;

			// Si no hereda de ValueObject, reportar diagnóstico
			var diagnostic = Diagnostic.Create(
				DiagnosticDescriptors.ValueObjectMustInheritFromBase,
				classSymbol.Locations[0],
				classSymbol.Name);

			context.ReportDiagnostic(diagnostic);
		}

		private bool HasValueObjectAttribute(INamedTypeSymbol classSymbol)
		{
			foreach (var attribute in classSymbol.GetAttributes())
			{
				if (attribute.AttributeClass?.Name == "ValueObjectAttribute" &&
					attribute.AttributeClass?.ContainingNamespace?.ToString() == "DDD.Abstractions")
				{
					return true;
				}
			}
			return false;
		}

		private bool InheritsFromValueObjectBase(INamedTypeSymbol classSymbol)
		{
			// Recorrer la cadena de herencia
			var current = classSymbol.BaseType;
			while (current != null)
			{
				if (current.Name == "ValueObject" &&
					current.ContainingNamespace?.ToString() == "DDD.Abstractions")
				{
					return true;
				}
				current = current.BaseType;
			}
			return false;
		}
	}
}
