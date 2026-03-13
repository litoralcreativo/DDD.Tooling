using System.Threading.Tasks;
using DDD.Analyzers.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace DDD.Analyzers.Tests.CodeFixes
{
	/// <summary>
	/// Tests para ValueObjectEqualsCodeFixProvider:
	///   DDD007 — ValueObject sin Equals → classDeclaration.Identifier
	///   DDD008 — ValueObject sin GetHashCode → classDeclaration.Identifier
	/// El fix agrega ambos métodos de una sola vez.
	/// </summary>
	public class DDD007_008_ValueObjectEqualsCodeFixTests
	{
		private const string DddAttributes = Helpers.AnalyzerTestHelper.DddAttributesSource;

		[Fact]
		public async Task DDD007_SinEquals_AgregaEqualsYGetHashCode()
		{
			// Money dispara DDD007 y DDD008 simultáneamente — el fix resuelve ambos de una vez
			var before = @"
using DDD.Abstractions;
namespace Domain
{
    [ValueObject]
    public class {|DDD007:{|DDD008:Money|}|}
    {
        public decimal Amount { get; }
    }
}";
			// El fix genera Equals con la propiedad Amount y GetHashCode con hash * 23
			var after = @"
using DDD.Abstractions;
namespace Domain
{
    [ValueObject]
    public class Money
    {
        public decimal Amount { get; }

        public override bool Equals(object obj)
        {
            if (obj is Money other)
            {
                return Amount == other.Amount;
            }
            return false;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Amount.GetHashCode();
                return hash;
            }
        }
    }
}";
			var test = new CSharpCodeFixTest<ValueObjectImmutabilityAnalyzer, ValueObjectEqualsCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes } },
				FixedState = { Sources = { after, DddAttributes } },
				CodeActionEquivalenceKey = nameof(ValueObjectEqualsCodeFixProvider),
				// DDD007 y DDD008 se disparan al mismo tiempo; el fix los resuelve de una vez
				NumberOfFixAllIterations = 1
			};
			await test.RunAsync();
		}

		[Fact]
		public async Task DDD008_SinGetHashCode_AgregaSoloGetHashCode()
		{
			// Clase con Equals pero sin GetHashCode → dispara solo DDD008
			var before = @"
using DDD.Abstractions;
namespace Domain
{
    [ValueObject]
    public class {|DDD008:Address|}
    {
        public string Street { get; }

        public override bool Equals(object obj)
        {
            if (obj is Address other)
                return Street == other.Street;
            return false;
        }
    }
}";
			// El fix solo agrega GetHashCode (Equals ya existe)
			var after = @"
using DDD.Abstractions;
namespace Domain
{
    [ValueObject]
    public class Address
    {
        public string Street { get; }

        public override bool Equals(object obj)
        {
            if (obj is Address other)
                return Street == other.Street;
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Street?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}";
			var test = new CSharpCodeFixTest<ValueObjectImmutabilityAnalyzer, ValueObjectEqualsCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes } },
				FixedState = { Sources = { after, DddAttributes } },
				CodeActionEquivalenceKey = nameof(ValueObjectEqualsCodeFixProvider)
			};
			await test.RunAsync();
		}
	}
}
