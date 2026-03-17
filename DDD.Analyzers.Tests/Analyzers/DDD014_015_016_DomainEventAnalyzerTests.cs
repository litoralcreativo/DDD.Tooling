using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
	DDD.Analyzers.DomainEventAnalyzer,
	Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace DDD.Analyzers.Tests.Analyzers
{
	public class DDD014_015_016_DomainEventAnalyzerTests
	{
		private const string DddAttributes = DDD.Analyzers.Tests.Helpers.AnalyzerTestHelper.DddAttributesSource;

		// ─── Happy paths ───────────────────────────────────────────────────────

		[Fact]
		public async Task DomainEvent_Correcto_NoDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
using System;
[DomainEvent]
[BoundedContext(""Catalog"")]
public class CoursePublishedEvent
{
    public Guid CourseId { get; }
    public DateTime OccurredOn { get; }
    public CoursePublishedEvent(Guid courseId, DateTime occurredOn)
    {
        CourseId = courseId;
        OccurredOn = occurredOn;
    }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task DomainEvent_ConSharedKernel_NoDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
using System;
[DomainEvent]
[SharedKernel]
public class AuditEvent
{
    public string Action { get; }
    public DateTime OccurredOn { get; }
    public AuditEvent(string action, DateTime occurredOn)
    {
        Action = action;
        OccurredOn = occurredOn;
    }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task DomainEvent_OccurredOnDateTimeOffset_NoDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
using System;
[DomainEvent]
[BoundedContext(""Orders"")]
public class OrderPlacedEvent
{
    public DateTimeOffset OccurredOn { get; }
    public OrderPlacedEvent(DateTimeOffset occurredOn) { OccurredOn = occurredOn; }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task DomainEvent_ConInit_NoDiagnostico()
		{
			const string isExternalInit = @"
namespace System.Runtime.CompilerServices { public class IsExternalInit {} }";

			var source = @"
using DDD.Abstractions;
using System;
[DomainEvent]
[BoundedContext(""Catalog"")]
public class CourseUpdatedEvent
{
    public string Title { get; init; }
    public DateTime OccurredOn { get; }
    public CourseUpdatedEvent(string title, DateTime occurredOn)
    {
        Title = title;
        OccurredOn = occurredOn;
    }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes + isExternalInit);
		}

		[Fact]
		public async Task ClaseSinDomainEvent_NoDiagnostico()
		{
			var source = @"
public class PlainEvent { public string Name { get; set; } }";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		// ─── DDD014 ────────────────────────────────────────────────────────────

		[Fact]
		public async Task DDD014_PropiedadConSetterPublico_ReportaDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
using System;
[DomainEvent]
[BoundedContext(""Catalog"")]
public class CoursePublishedEvent
{
    {|DDD014:public string Title { get; set; }|}
    public DateTime OccurredOn { get; }
    public CoursePublishedEvent(string title, DateTime occurredOn)
    {
        Title = title;
        OccurredOn = occurredOn;
    }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task DDD014_VariasPropsConSetterPublico_ReportaVariosDiagnosticos()
		{
			var source = @"
using DDD.Abstractions;
using System;
[DomainEvent]
[BoundedContext(""Sales"")]
public class OrderPlacedEvent
{
    {|DDD014:public string OrderId { get; set; }|}
    {|DDD014:public decimal Total { get; set; }|}
    public DateTime OccurredOn { get; }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task DDD014_PropiedadConPrivateSet_NoDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
using System;
[DomainEvent]
[BoundedContext(""Catalog"")]
public class CoursePublishedEvent
{
    public string Title { get; private set; }
    public DateTime OccurredOn { get; }
    public CoursePublishedEvent(string title, DateTime occurredOn)
    {
        Title = title;
        OccurredOn = occurredOn;
    }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		// ─── DDD015 ────────────────────────────────────────────────────────────

		[Fact]
		public async Task DDD015_SinOccurredOn_ReportaDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
[DomainEvent]
[BoundedContext(""Catalog"")]
public class {|DDD015:CoursePublishedEvent|}
{
    public string Title { get; }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task DDD015_OccurredOnTipoIncorrecto_ReportaDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
[DomainEvent]
[BoundedContext(""Catalog"")]
public class {|DDD015:CoursePublishedEvent|}
{
    public string OccurredOn { get; }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		// ─── DDD016 ────────────────────────────────────────────────────────────

		[Fact]
		public async Task DDD016_SinBoundedContext_ReportaDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
using System;
[DomainEvent]
public class {|DDD015:{|DDD016:CoursePublishedEvent|}|}
{
    public CoursePublishedEvent() { }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		[Fact]
		public async Task DDD016_ConBoundedContext_NoDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
using System;
[DomainEvent]
[BoundedContext(""Catalog"")]
public class {|DDD015:CoursePublishedEvent|}
{
    public CoursePublishedEvent() { }
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}

		// ─── Combinados ────────────────────────────────────────────────────────

		[Fact]
		public async Task DDD014_Y_DDD015_Y_DDD016_TodosAusentesReportaTres()
		{
			var source = @"
using DDD.Abstractions;
[DomainEvent]
public class {|DDD015:{|DDD016:BadEvent|}|}
{
    {|DDD014:public string Data { get; set; }|}
}";
			await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
		}
	}
}
