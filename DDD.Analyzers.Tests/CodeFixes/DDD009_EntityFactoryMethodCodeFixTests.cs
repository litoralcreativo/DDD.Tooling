using System.Threading.Tasks;
using DDD.Analyzers.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace DDD.Analyzers.Tests.CodeFixes
{
	/// <summary>
	/// Tests para EntityFactoryMethodCodeFixProvider (DDD009):
	/// El diagnóstico se reporta en classDeclaration.Identifier.
	///   - Constructor público → hace privado + agrega Create estático
	///   - Método Create no estático → lo hace estático
	/// </summary>
	public class DDD009_EntityFactoryMethodCodeFixTests
	{
		private const string DddAttributes = Helpers.AnalyzerTestHelper.DddAttributesSource;

		[Fact]
		public async Task DDD009_ConstructorPublico_AgregaCreateYHacePrivado()
		{
			var before = @"
using DDD.Abstractions;
using System;
namespace Domain
{
    [Entity]
    public class {|DDD009:Order|}
    {
        [EntityId]
        public Guid Id { get; private set; }

        public Order(Guid id)
        {
            Id = id;
        }
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

        public static Order Create(Guid id)
        {
            return new Order(id);
        }

        private Order(Guid id)
        {
            Id = id;
        }
    }
}";
			var test = new CSharpCodeFixTest<EntityFactoryMethodAnalyzer, EntityFactoryMethodCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes } },
				FixedState = { Sources = { after, DddAttributes } },
				CodeActionEquivalenceKey = nameof(EntityFactoryMethodCodeFixProvider) + "_AddFactory"
			};
			await test.RunAsync();
		}

		[Fact]
		public async Task DDD009_MetodoCreateNoEstatico_LoHaceEstatico()
		{
			var before = @"
using DDD.Abstractions;
using System;
namespace Domain
{
    [Entity]
    public class {|DDD009:Order|}
    {
        [EntityId]
        public Guid Id { get; private set; }

        private Order() { }

        public Order Create()
        {
            return new Order();
        }
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

        private Order() { }

        public static Order Create()
        {
            return new Order();
        }
    }
}";
			var test = new CSharpCodeFixTest<EntityFactoryMethodAnalyzer, EntityFactoryMethodCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes } },
				FixedState = { Sources = { after, DddAttributes } },
				CodeActionEquivalenceKey = nameof(EntityFactoryMethodCodeFixProvider) + "_MakeStatic"
			};
			await test.RunAsync();
		}
	}
}
