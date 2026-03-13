using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
	DDD.Analyzers.BoundedContextDeclarationAnalyzer,
	Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace DDD.Analyzers.Tests.Analyzers
{
	public class DDD010_BoundedContextDeclarationTests
	{
		private const string DddAttributes = DDD.Analyzers.Tests.Helpers.AnalyzerTestHelper.DddAttributesSource;

		// ─── Happy paths ───────────────────────────────────────────────────────

		[Fact]
		public async Task Entity_ConBoundedContext_NoDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
using System;
[Entity]
[BoundedContext(""Catalog"")]
public class Product
{
    [EntityId]
    public Guid Id { get; private set; }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task AggregateRoot_ConBoundedContext_NoDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
using System;
[AggregateRoot]
[BoundedContext(""Sales"")]
public class Order
{
    [EntityId]
    public Guid Id { get; private set; }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task ValueObject_ConBoundedContext_NoDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
[ValueObject]
[BoundedContext(""Catalog"")]
public class Money { public decimal Amount { get; } }";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task Entity_ConSharedKernel_NoDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
using System;
[Entity]
[SharedKernel]
public class SharedEntity
{
    [EntityId]
    public Guid Id { get; private set; }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task ClaseSinAtributoDDD_NoDiagnostico()
		{
			var source = @"
public class PlainClass { public string Name { get; set; } }";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		// ─── Error paths ───────────────────────────────────────────────────────

		[Fact]
		public async Task Entity_SinBoundedContext_ReportaDDD010()
		{
			var source = @"
using DDD.Abstractions;
using System;
[Entity]
public class {|DDD010:Product|}
{
    [EntityId]
    public Guid Id { get; private set; }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task AggregateRoot_SinBoundedContext_ReportaDDD010()
		{
			var source = @"
using DDD.Abstractions;
using System;
[AggregateRoot]
public class {|DDD010:Order|}
{
    [EntityId]
    public Guid Id { get; private set; }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task ValueObject_SinBoundedContext_ReportaDDD010()
		{
			var source = @"
using DDD.Abstractions;
[ValueObject]
public class {|DDD010:Money|} { public decimal Amount { get; } }";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}
	}
}
