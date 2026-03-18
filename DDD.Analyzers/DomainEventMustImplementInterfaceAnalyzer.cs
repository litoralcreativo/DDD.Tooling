using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace DDD.Analyzers
{
	/// <summary>
	/// Analizador DDD019: Valida que las clases decoradas con [DomainEvent]
	/// implementen la interfaz IDomainEvent.
	/// </summary>
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DomainEventMustImplementInterfaceAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
			ImmutableArray.Create(DiagnosticDescriptors.DomainEventMustImplementInterface);

		public override void Initialize(AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();

			context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
		}

		private void AnalyzeSymbol(SymbolAnalysisContext context)
		{
			var classSymbol = (INamedTypeSymbol)context.Symbol;

			// Solo analizar clases (no interfaces, structs, enums, etc.)
			if (classSymbol.TypeKind != TypeKind.Class)
				return;

			// Verificar si la clase está decorada con [DomainEvent]
			if (!HasDomainEventAttribute(classSymbol))
				return;

			// Verificar si implementa IDomainEvent (directa o indirectamente)
			if (ImplementsIDomainEvent(classSymbol))
				return;

			// Si no implementa IDomainEvent, reportar diagnóstico
			var diagnostic = Diagnostic.Create(
				DiagnosticDescriptors.DomainEventMustImplementInterface,
				classSymbol.Locations[0],
				classSymbol.Name);

			context.ReportDiagnostic(diagnostic);
		}

		private bool HasDomainEventAttribute(INamedTypeSymbol classSymbol)
		{
			foreach (var attribute in classSymbol.GetAttributes())
			{
				if (attribute.AttributeClass?.Name == "DomainEventAttribute" &&
					attribute.AttributeClass?.ContainingNamespace?.ToString() == "DDD.Abstractions")
				{
					return true;
				}
			}
			return false;
		}

		private bool ImplementsIDomainEvent(INamedTypeSymbol classSymbol)
		{
			// Buscar en todas las interfaces implementadas (directa e indirectamente)
			foreach (var interfaceType in classSymbol.AllInterfaces)
			{
				if (interfaceType.Name == "IDomainEvent" &&
					interfaceType.ContainingNamespace?.ToString() == "DDD.Abstractions")
				{
					return true;
				}
			}
			return false;
		}
	}
}
