using System.Threading.Tasks;
using DDD.Analyzers.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace DDD.Analyzers.Tests.CodeFixes
{
	/// <summary>
	/// Tests para EntityIdOnPropertyCodeFixProvider: convierte un campo con [EntityId]
	/// en una propiedad automática (DDD003).
	/// El diagnóstico se reporta en fieldDeclaration.GetLocation() (toda la declaración del campo).
	/// </summary>
	public class DDD003_EntityIdOnPropertyCodeFixTests
	{
		private const string DddAttributes = Helpers.AnalyzerTestHelper.DddAttributesSource;

		[Fact]
		public async Task DDD003_CampoEntityId_ConverteAPropiedad()
		{
			var before = @"
using DDD.Abstractions;
using System;
namespace Domain
{
    [Entity]
    public class Order
    {
        {|DDD003:[EntityId]
        private Guid _id;|}
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
			var test = new CSharpCodeFixTest<DddAttributeUsageAnalyzer, EntityIdOnPropertyCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes } },
				FixedState = { Sources = { after, DddAttributes } },
				CodeActionEquivalenceKey = "DDD003_ConvertToProperty"
			};
			await test.RunAsync();
		}

		[Fact]
		public async Task DDD003_CampoEntityId_NombrePersonalizado_ConverteAPropiedad()
		{
			var before = @"
using DDD.Abstractions;
using System;
namespace Domain
{
    [Entity]
    public class Product
    {
        {|DDD003:[EntityId]
        private Guid _productId;|}
    }
}";
			var after = @"
using DDD.Abstractions;
using System;
namespace Domain
{
    [Entity]
    public class Product
    {
        [EntityId]
        public Guid ProductId { get; private set; }
    }
}";
			var test = new CSharpCodeFixTest<DddAttributeUsageAnalyzer, EntityIdOnPropertyCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes } },
				FixedState = { Sources = { after, DddAttributes } },
				CodeActionEquivalenceKey = "DDD003_ConvertToProperty"
			};
			await test.RunAsync();
		}
	}
}
