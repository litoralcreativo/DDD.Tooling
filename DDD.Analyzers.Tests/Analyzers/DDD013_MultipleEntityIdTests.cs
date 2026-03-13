using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
	DDD.Analyzers.MultipleEntityIdAnalyzer,
	Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace DDD.Analyzers.Tests.Analyzers
{
	public class DDD013_MultipleEntityIdTests
	{
		private const string DddAttributes = DDD.Analyzers.Tests.Helpers.AnalyzerTestHelper.DddAttributesSource;

		// ─── Happy paths ───────────────────────────────────────────────────────

		[Fact]
		public async Task Entity_ConUnSoloEntityId_NoDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
using System;
[Entity]
public class Product
{
    [EntityId]
    public Guid Id { get; private set; }
    public string Name { get; set; }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task AggregateRoot_ConUnSoloEntityId_NoDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
using System;
[AggregateRoot]
public class Order
{
    [EntityId]
    public Guid OrderId { get; private set; }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task ValueObject_ConVariasPropiedades_NoDiagnostico()
		{
			// ValueObject no aplica esta regla
			var source = @"
using DDD.Abstractions;
[ValueObject]
public class Money
{
    public decimal Amount { get; }
    public string Currency { get; }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task ClaseSinAtributo_NoDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
using System;
public class PlainClass
{
    [EntityId]
    public Guid Id1 { get; set; }
    [EntityId]
    public Guid Id2 { get; set; }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		// ─── Error paths ───────────────────────────────────────────────────────

		[Fact]
		public async Task Entity_ConDosEntityId_ReportaDDD013EnAmbas()
		{
			var source = @"
using DDD.Abstractions;
using System;
[Entity]
public class {|DDD013:Product|}
{
    [EntityId]
    public Guid Id { get; private set; }
    [EntityId]
    public Guid LegacyId { get; private set; }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task AggregateRoot_ConTresEntityId_ReportaDDD013EnTodas()
		{
			var source = @"
using DDD.Abstractions;
using System;
[AggregateRoot]
public class {|DDD013:Order|}
{
    [EntityId]
    public Guid Id { get; private set; }
    [EntityId]
    public Guid ExternalId { get; private set; }
    [EntityId]
    public Guid LegacyId { get; private set; }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}
	}
}
