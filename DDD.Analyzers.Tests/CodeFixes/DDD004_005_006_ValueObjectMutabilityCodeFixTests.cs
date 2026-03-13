using System.Threading.Tasks;
using DDD.Analyzers.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace DDD.Analyzers.Tests.CodeFixes
{
	/// <summary>
	/// Tests para ValueObjectMutabilityCodeFixProvider:
	///   DDD004 — setter público en ValueObject → propertySyntax.GetLocation() (propiedad completa)
	///   DDD005 — [Entity]+[ValueObject] → classDeclaration.Identifier
	///   DDD006 — [AggregateRoot]+[ValueObject] → classDeclaration.Identifier
	/// </summary>
	public class DDD004_005_006_ValueObjectMutabilityCodeFixTests
	{
		private const string DddAttributes = Helpers.AnalyzerTestHelper.DddAttributesSource;

		// ── DDD004 ────────────────────────────────────────────────────────────────

		[Fact]
		public async Task DDD004_SetterPublico_CambiaAPrivateSet()
		{
			// Usamos clase con Equals+GetHashCode implementados para evitar DDD007/DDD008 en el TestState
			var before = @"
using DDD.Abstractions;
namespace Domain
{
    [ValueObject]
    public class Money
    {
        {|DDD004:public decimal Amount { get; set; }|}
        public override bool Equals(object obj) => obj is Money other && Amount == other.Amount;
        public override int GetHashCode() => Amount.GetHashCode();
    }
}";
			var after = @"
using DDD.Abstractions;
namespace Domain
{
    [ValueObject]
    public class Money
    {
        public decimal Amount { get; private set; }
        public override bool Equals(object obj) => obj is Money other && Amount == other.Amount;
        public override int GetHashCode() => Amount.GetHashCode();
    }
}";
			var test = new CSharpCodeFixTest<ValueObjectImmutabilityAnalyzer, ValueObjectMutabilityCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes } },
				FixedState = { Sources = { after, DddAttributes } },
				CodeActionEquivalenceKey = "DDD004_PrivateSet"
			};
			await test.RunAsync();
		}

		[Fact]
		public async Task DDD004_SetterPublico_CambiaAInit()
		{
			// Para usar 'init' en el test, necesitamos definir IsExternalInit (C# 9 en netstandard2.0)
			const string isExternalInit = @"
namespace System.Runtime.CompilerServices { public class IsExternalInit {} }";

			var before = @"
using DDD.Abstractions;
namespace Domain
{
    [ValueObject]
    public class Money
    {
        {|DDD004:public decimal Amount { get; set; }|}
        public override bool Equals(object obj) => obj is Money other && Amount == other.Amount;
        public override int GetHashCode() => Amount.GetHashCode();
    }
}";
			var after = @"
using DDD.Abstractions;
namespace Domain
{
    [ValueObject]
    public class Money
    {
        public decimal Amount { get; init; }
        public override bool Equals(object obj) => obj is Money other && Amount == other.Amount;
        public override int GetHashCode() => Amount.GetHashCode();
    }
}";
			var test = new CSharpCodeFixTest<ValueObjectImmutabilityAnalyzer, ValueObjectMutabilityCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes, isExternalInit } },
				FixedState = { Sources = { after, DddAttributes, isExternalInit } },
				CodeActionEquivalenceKey = "DDD004_Init"
			};
			await test.RunAsync();
		}

		// ── DDD005 ────────────────────────────────────────────────────────────────

		[Fact]
		public async Task DDD005_EntityYValueObject_QuitaValueObject()
		{
			// Usamos [Entity, ValueObject] en una sola AttributeList para evitar artefactos de trivia
			var before = @"
using DDD.Abstractions;
namespace Domain
{
    [Entity, ValueObject]
    public class {|DDD005:Hybrid|}
    {
    }
}";
			var after = @"
using DDD.Abstractions;
namespace Domain
{
    [Entity]
    public class Hybrid
    {
    }
}";
			var test = new CSharpCodeFixTest<DddAttributeUsageAnalyzer, ValueObjectMutabilityCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes } },
				FixedState = { Sources = { after, DddAttributes } },
				CodeActionEquivalenceKey = "DDD005_KeepEntity"
			};
			await test.RunAsync();
		}

		[Fact]
		public async Task DDD005_EntityYValueObject_QuitaEntity()
		{
			var before = @"
using DDD.Abstractions;
namespace Domain
{
    [Entity, ValueObject]
    public class {|DDD005:Hybrid|}
    {
    }
}";
			var after = @"
using DDD.Abstractions;
namespace Domain
{
    [ValueObject]
    public class Hybrid
    {
    }
}";
			var test = new CSharpCodeFixTest<DddAttributeUsageAnalyzer, ValueObjectMutabilityCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes } },
				FixedState = { Sources = { after, DddAttributes } },
				CodeActionEquivalenceKey = "DDD005_KeepValueObject"
			};
			await test.RunAsync();
		}

		// ── DDD006 ────────────────────────────────────────────────────────────────

		[Fact]
		public async Task DDD006_AggregateRootYValueObject_QuitaValueObject()
		{
			var before = @"
using DDD.Abstractions;
namespace Domain
{
    [AggregateRoot, ValueObject]
    public class {|DDD006:HybridRoot|}
    {
    }
}";
			var after = @"
using DDD.Abstractions;
namespace Domain
{
    [AggregateRoot]
    public class HybridRoot
    {
    }
}";
			var test = new CSharpCodeFixTest<DddAttributeUsageAnalyzer, ValueObjectMutabilityCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes } },
				FixedState = { Sources = { after, DddAttributes } },
				CodeActionEquivalenceKey = "DDD006_KeepAggregateRoot"
			};
			await test.RunAsync();
		}

		[Fact]
		public async Task DDD006_AggregateRootYValueObject_QuitaAggregateRoot()
		{
			var before = @"
using DDD.Abstractions;
namespace Domain
{
    [AggregateRoot, ValueObject]
    public class {|DDD006:HybridRoot|}
    {
    }
}";
			var after = @"
using DDD.Abstractions;
namespace Domain
{
    [ValueObject]
    public class HybridRoot
    {
    }
}";
			var test = new CSharpCodeFixTest<DddAttributeUsageAnalyzer, ValueObjectMutabilityCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes } },
				FixedState = { Sources = { after, DddAttributes } },
				CodeActionEquivalenceKey = "DDD006_KeepValueObject"
			};
			await test.RunAsync();
		}
	}
}
