using System.Threading.Tasks;
using DDD.Analyzers.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace DDD.Analyzers.Tests.CodeFixes
{
	/// <summary>
	/// Tests para los Code Fixes de DomainEvent:
	///   DDD014 — setter público → 'private set' o 'init'
	///   DDD015 — falta OccurredOn → agrega la propiedad
	///   DDD016 — falta BoundedContext → agrega [BoundedContext("BC")] o [SharedKernel]
	/// </summary>
	public class DDD014_015_016_DomainEventCodeFixTests
	{
		private const string DddAttributes = Helpers.AnalyzerTestHelper.DddAttributesSource;

		// ── DDD014 ────────────────────────────────────────────────────────────

		[Fact]
		public async Task DDD014_SetterPublico_CambiaAPrivateSet()
		{
			var before = @"
using DDD.Abstractions;
using System;
namespace Catalog
{
    [DomainEvent]
    [BoundedContext(""Catalog"")]
    public class CoursePublishedEvent
    {
        {|DDD014:public string Title { get; set; }|}
        public DateTime OccurredOn { get; }
    }
}";
			var after = @"
using DDD.Abstractions;
using System;
namespace Catalog
{
    [DomainEvent]
    [BoundedContext(""Catalog"")]
    public class CoursePublishedEvent
    {
        public string Title { get; private set; }
        public DateTime OccurredOn { get; }
    }
}";
			var test = new CSharpCodeFixTest<DomainEventAnalyzer, DomainEventImmutabilityCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes } },
				FixedState = { Sources = { after, DddAttributes } },
				CodeActionEquivalenceKey = "DDD014_PrivateSet"
			};
			await test.RunAsync();
		}

		[Fact]
		public async Task DDD014_SetterPublico_CambiaAInit()
		{
			const string isExternalInit = @"
namespace System.Runtime.CompilerServices { public class IsExternalInit {} }";

			var before = @"
using DDD.Abstractions;
using System;
namespace Catalog
{
    [DomainEvent]
    [BoundedContext(""Catalog"")]
    public class CoursePublishedEvent
    {
        {|DDD014:public string Title { get; set; }|}
        public DateTime OccurredOn { get; }
    }
}";
			var after = @"
using DDD.Abstractions;
using System;
namespace Catalog
{
    [DomainEvent]
    [BoundedContext(""Catalog"")]
    public class CoursePublishedEvent
    {
        public string Title { get; init; }
        public DateTime OccurredOn { get; }
    }
}";
			var test = new CSharpCodeFixTest<DomainEventAnalyzer, DomainEventImmutabilityCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes, isExternalInit } },
				FixedState = { Sources = { after, DddAttributes, isExternalInit } },
				CodeActionEquivalenceKey = "DDD014_Init"
			};
			await test.RunAsync();
		}

		// ── DDD015 ────────────────────────────────────────────────────────────

		[Fact]
		public async Task DDD015_SinOccurredOn_AgregaPropiedad()
		{
			var before = @"
using DDD.Abstractions;
using System;
namespace Catalog
{
    [DomainEvent]
    [BoundedContext(""Catalog"")]
    public class {|DDD015:CoursePublishedEvent|}
    {
        public string Title { get; }
    }
}";
			var after = @"
using DDD.Abstractions;
using System;
namespace Catalog
{
    [DomainEvent]
    [BoundedContext(""Catalog"")]
    public class CoursePublishedEvent
    {
        public DateTime OccurredOn { get; }
        public string Title { get; }
    }
}";
			var test = new CSharpCodeFixTest<DomainEventAnalyzer, DomainEventOccurredOnCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes } },
				FixedState = { Sources = { after, DddAttributes } },
				CodeActionEquivalenceKey = "DDD015_OccurredOn"
			};
			await test.RunAsync();
		}

		[Fact]
		public async Task DDD015_SinOccurredOnYSinUsingSystem_AgregaAmboS()
		{
			var before = @"
using DDD.Abstractions;
namespace Catalog
{
    [DomainEvent]
    [BoundedContext(""Catalog"")]
    public class {|DDD015:CoursePublishedEvent|}
    {
        public string Title { get; }
    }
}";
			var after = @"
using System;
using DDD.Abstractions;
namespace Catalog
{
    [DomainEvent]
    [BoundedContext(""Catalog"")]
    public class CoursePublishedEvent
    {
        public DateTime OccurredOn { get; }
        public string Title { get; }
    }
}";
			var test = new CSharpCodeFixTest<DomainEventAnalyzer, DomainEventOccurredOnCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes } },
				FixedState = { Sources = { after, DddAttributes } },
				CodeActionEquivalenceKey = "DDD015_OccurredOn"
			};
			await test.RunAsync();
		}

		// ── DDD016 ────────────────────────────────────────────────────────────

		[Fact]
		public async Task DDD016_AgregaBoundedContextInferidoDesdeNamespace()
		{
			var before = @"
using DDD.Abstractions;
using System;
namespace MyApp.Catalog
{
    [DomainEvent]
    public class {|DDD016:CoursePublishedEvent|}
    {
        public DateTime OccurredOn { get; }
    }
}";
			var after = @"
using DDD.Abstractions;
using System;
namespace MyApp.Catalog
{
    [BoundedContext(""Catalog"")]
    [DomainEvent]
    public class CoursePublishedEvent
    {
        public DateTime OccurredOn { get; }
    }
}";
			var test = new CSharpCodeFixTest<DomainEventAnalyzer, BoundedContextDeclarationCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes } },
				FixedState = { Sources = { after, DddAttributes } },
				CodeActionEquivalenceKey = nameof(BoundedContextDeclarationCodeFixProvider) + "_BC"
			};
			await test.RunAsync();
		}

		[Fact]
		public async Task DDD016_AgregaSharedKernel()
		{
			var before = @"
using DDD.Abstractions;
using System;
namespace MyApp.SharedKernel
{
    [DomainEvent]
    public class {|DDD016:AuditEvent|}
    {
        public DateTime OccurredOn { get; }
    }
}";
			var after = @"
using DDD.Abstractions;
using System;
namespace MyApp.SharedKernel
{
    [SharedKernel]
    [DomainEvent]
    public class AuditEvent
    {
        public DateTime OccurredOn { get; }
    }
}";
			var test = new CSharpCodeFixTest<DomainEventAnalyzer, BoundedContextDeclarationCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes } },
				FixedState = { Sources = { after, DddAttributes } },
				CodeActionEquivalenceKey = nameof(BoundedContextDeclarationCodeFixProvider) + "_SK"
			};
			await test.RunAsync();
		}
	}
}
