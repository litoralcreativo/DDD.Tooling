using System.Threading.Tasks;
using DDD.Analyzers;
using DDD.Analyzers.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace DDD.Analyzers.Tests.CodeFixes
{
	/// <summary>
	/// Tests para EntityIdCodeFixProvider: agrega propiedad Id con [EntityId]
	/// cuando se detecta DDD001 (Entity sin EntityId) o DDD002 (AggregateRoot sin EntityId).
	/// El diagnóstico se reporta sobre el identificador de la clase (classDeclaration.Identifier).
	/// </summary>
	public class DDD001_002_EntityIdCodeFixTests
	{
		private const string DddAttributes = Helpers.AnalyzerTestHelper.DddAttributesSource;

		// ── DDD001 ────────────────────────────────────────────────────────────────

		[Fact]
		public async Task DDD001_Entity_AgregaId_ConEntityId()
		{
			var before = @"
using DDD.Abstractions;
namespace Domain
{
    [Entity]
    public class {|DDD001:Order|}
    {
    }
}";
			var after = @"
using DDD.Abstractions;
using System;
namespace Domain
{
    [Entity]
    public class Order
    {

        [EntityId]
        public Guid Id { get; private set; }
    }
}";
			await RunCodeFixAsync(before, after);
		}

		[Fact]
		public async Task DDD001_Entity_YaTieneEntityId_NoModifica()
		{
			var source = @"
using DDD.Abstractions;
using System;
namespace Domain
{
    [Entity]
    public class Order
    {
        [EntityId]
        public Guid Id { get; private set; }
    }
}";
			var test = new CSharpCodeFixTest<EntityMustHaveEntityIdAnalyzer, EntityIdCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { source, DddAttributes } },
				FixedState = { Sources = { source, DddAttributes } },
				CodeActionEquivalenceKey = nameof(EntityIdCodeFixProvider)
			};
			await test.RunAsync();
		}

		// ── DDD002 ────────────────────────────────────────────────────────────────

		[Fact]
		public async Task DDD002_AggregateRoot_AgregaId_ConEntityId()
		{
			var before = @"
using DDD.Abstractions;
namespace Domain
{
    [AggregateRoot]
    public class {|DDD002:Customer|}
    {
    }
}";
			var after = @"
using DDD.Abstractions;
using System;
namespace Domain
{
    [AggregateRoot]
    public class Customer
    {

        [EntityId]
        public Guid Id { get; private set; }
    }
}";
			// DDD002 es reportado por AggregateRootMustHaveEntityIdAnalyzer, no por EntityMustHaveEntityIdAnalyzer
			var test = new CSharpCodeFixTest<AggregateRootMustHaveEntityIdAnalyzer, EntityIdCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes } },
				FixedState = { Sources = { after, DddAttributes } },
				CodeActionEquivalenceKey = nameof(EntityIdCodeFixProvider)
			};
			await test.RunAsync();
		}

		// ── Helper ────────────────────────────────────────────────────────────────

		private static async Task RunCodeFixAsync(string before, string after)
		{
			var test = new CSharpCodeFixTest<EntityMustHaveEntityIdAnalyzer, EntityIdCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes } },
				FixedState = { Sources = { after, DddAttributes } },
				CodeActionEquivalenceKey = nameof(EntityIdCodeFixProvider)
			};
			await test.RunAsync();
		}
	}
}
