using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
	DDD.Analyzers.EntityFactoryMethodAnalyzer,
	Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace DDD.Analyzers.Tests.Analyzers
{
	public class DDD009_EntityFactoryMethodTests
	{
		private const string DddAttributes = DDD.Analyzers.Tests.Helpers.AnalyzerTestHelper.DddAttributesSource;

		// ─── Happy paths ───────────────────────────────────────────────────────

		[Fact]
		public async Task Entity_ConFactoryMethod_SinConstructorPublico_NoDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
using System;
[Entity]
public class Product
{
    [EntityId]
    public Guid Id { get; private set; }
    public string Name { get; private set; }

    private Product() { }

    public static Product Create(string name)
    {
        return new Product { Id = Guid.NewGuid(), Name = name };
    }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task AggregateRoot_ConFactoryMethod_SinConstructorPublico_NoDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
using System;
[AggregateRoot]
public class Order
{
    [EntityId]
    public Guid Id { get; private set; }

    private Order() { }

    public static Order Create()
    {
        return new Order { Id = Guid.NewGuid() };
    }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task ClaseSinAtributo_NoDiagnostico()
		{
			var source = @"
public class PlainClass
{
    public PlainClass() { }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		// ─── Error paths (DDD009 es Info) ─────────────────────────────────────

		[Fact]
		public async Task Entity_ConConstructorPublico_SinFactoryMethod_ReportaDDD009()
		{
			var source = @"
using DDD.Abstractions;
using System;
[Entity]
public class {|DDD009:Product|}
{
    [EntityId]
    public Guid Id { get; private set; }

    public Product(string name) { Id = Guid.NewGuid(); }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task Entity_SinConstructorExplicito_SinFactoryMethod_ReportaDDD009()
		{
			// Sin constructor explícito el compilador genera uno público implícito
			var source = @"
using DDD.Abstractions;
using System;
[Entity]
public class {|DDD009:Product|}
{
    [EntityId]
    public Guid Id { get; private set; }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task Entity_ConFactoryMethod_PeroTambienConstructorPublico_ReportaDDD009()
		{
			var source = @"
using DDD.Abstractions;
using System;
[Entity]
public class {|DDD009:Product|}
{
    [EntityId]
    public Guid Id { get; private set; }

    public Product() { }

    public static Product Create()
    {
        return new Product { Id = Guid.NewGuid() };
    }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}
	}
}
