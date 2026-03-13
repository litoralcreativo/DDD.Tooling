using System.Threading.Tasks;
using DDD.Analyzers;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace DDD.Analyzers.Tests.Analyzers
{
	/// <summary>
	/// Tests para DDD011 (referencia directa entre BCs en miembros publicos)
	/// y DDD012 (uso en miembros privados).
	/// Se usa TestState.Sources con archivos separados para evitar conflictos de usings.
	/// </summary>
	public class DDD011_CrossBoundedContextTests
	{
		private const string DddAttributes = DDD.Analyzers.Tests.Helpers.AnalyzerTestHelper.DddAttributesSource;

		private const string CatalogSource = @"
using DDD.Abstractions;
using System;
namespace Catalog
{
    [AggregateRoot]
    [BoundedContext(""Catalog"")]
    public class Course
    {
        [EntityId]
        public Guid Id { get; private set; }
    }

    [Entity]
    [BoundedContext(""Catalog"")]
    public class CourseModule
    {
        [EntityId]
        public Guid Id { get; private set; }
    }

    [ValueObject]
    [BoundedContext(""Catalog"")]
    public class CourseName
    {
        public string Value { get; }
    }
}";

		// Happy paths

		[Fact]
		public async Task Referencias_DentroDelMismoBC_NoDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
using System;
namespace Catalog
{
    [AggregateRoot]
    [BoundedContext(""Catalog"")]
    public class CourseCatalog
    {
        [EntityId]
        public Guid Id { get; private set; }
        public Course RelatedCourse { get; set; }
    }
}";
			var test = new CSharpAnalyzerTest<CrossBoundedContextReferenceAnalyzer, DefaultVerifier>
			{
				TestState = { Sources = { source, CatalogSource, DddAttributes } }
			};
			await test.RunAsync();
		}

		[Fact]
		public async Task Referencia_ASharedKernel_NoDiagnostico()
		{
			var sharedKernelSource = @"
using DDD.Abstractions;
namespace SharedKernel
{
    [ValueObject]
    [SharedKernel]
    public class Money { public decimal Amount { get; } }
}";
			var source = @"
using DDD.Abstractions;
using System;
using SharedKernel;
namespace Sales
{
    [AggregateRoot]
    [BoundedContext(""Sales"")]
    public class Order
    {
        [EntityId]
        public Guid Id { get; private set; }
        public Money Price { get; set; }
    }
}";
			var test = new CSharpAnalyzerTest<CrossBoundedContextReferenceAnalyzer, DefaultVerifier>
			{
				TestState = { Sources = { source, sharedKernelSource, DddAttributes } }
			};
			await test.RunAsync();
		}

		[Fact]
		public async Task ClaseSinBoundedContext_NoDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
using System;
using Catalog;
namespace Students
{
    [AggregateRoot]
    public class Student
    {
        [EntityId]
        public Guid Id { get; private set; }
        public Course EnrolledCourse { get; set; }
    }
}";
			var test = new CSharpAnalyzerTest<CrossBoundedContextReferenceAnalyzer, DefaultVerifier>
			{
				TestState = { Sources = { source, CatalogSource, DddAttributes } }
			};
			await test.RunAsync();
		}

		// Error paths — DDD011
		// Nota: usamos WithSpan explícito porque los tres descriptores DDD011 (AggregateRoot/Entity/ValueObject)
		// comparten el mismo ID, lo que genera ambigüedad con la sintaxis de marcado inline {|DDD011:...|}.

		[Fact]
		public async Task PropiedadPublica_ReferenciaAggregateRoot_OtroBC_ReportaDDD011()
		{
			// línea 13: "        public Course EnrolledCourse { get; set; }"
			// col 9..49  (1-indexed, longitud de la propiedad completa)
			var source = @"
using DDD.Abstractions;
using System;
using Catalog;
namespace Students
{
    [AggregateRoot]
    [BoundedContext(""Students"")]
    public class Student
    {
        [EntityId]
        public Guid Id { get; private set; }
        public Course EnrolledCourse { get; set; }
    }
}";
			var expected = new DiagnosticResult(DiagnosticDescriptors.NoCrossContextDirectReference)
				.WithSpan(13, 9, 13, 51)
				.WithArguments("Student", "Students", "Course", "Catalog");

			var test = new CSharpAnalyzerTest<CrossBoundedContextReferenceAnalyzer, DefaultVerifier>
			{
				TestState = { Sources = { source, CatalogSource, DddAttributes } }
			};
			test.ExpectedDiagnostics.Add(expected);
			await test.RunAsync();
		}

		[Fact]
		public async Task PropiedadPublica_ReferenciaEntity_OtroBC_ReportaDDD011b()
		{
			// línea 13: "        public CourseModule CurrentModule { get; set; }"
			var source = @"
using DDD.Abstractions;
using System;
using Catalog;
namespace Students
{
    [AggregateRoot]
    [BoundedContext(""Students"")]
    public class Student
    {
        [EntityId]
        public Guid Id { get; private set; }
        public CourseModule CurrentModule { get; set; }
    }
}";
			var expected = new DiagnosticResult(DiagnosticDescriptors.NoCrossContextEntityReference)
				.WithSpan(13, 9, 13, 56)
				.WithArguments("Student", "Students", "CourseModule", "Catalog");

			var test = new CSharpAnalyzerTest<CrossBoundedContextReferenceAnalyzer, DefaultVerifier>
			{
				TestState = { Sources = { source, CatalogSource, DddAttributes } }
			};
			test.ExpectedDiagnostics.Add(expected);
			await test.RunAsync();
		}

		[Fact]
		public async Task PropiedadPublica_ReferenciaValueObject_OtroBC_ReportaDDD011c()
		{
			// línea 13: "        public CourseName Name { get; set; }"
			var source = @"
using DDD.Abstractions;
using System;
using Catalog;
namespace Students
{
    [AggregateRoot]
    [BoundedContext(""Students"")]
    public class Student
    {
        [EntityId]
        public Guid Id { get; private set; }
        public CourseName Name { get; set; }
    }
}";
			var expected = new DiagnosticResult(DiagnosticDescriptors.NoCrossContextValueObjectReference)
				.WithSpan(13, 9, 13, 45)
				.WithArguments("Student", "Students", "CourseName", "Catalog");

			var test = new CSharpAnalyzerTest<CrossBoundedContextReferenceAnalyzer, DefaultVerifier>
			{
				TestState = { Sources = { source, CatalogSource, DddAttributes } }
			};
			test.ExpectedDiagnostics.Add(expected);
			await test.RunAsync();
		}

		[Fact]
		public async Task PropiedadPublica_ColeccionGenerica_OtroBC_ReportaDDD011()
		{
			// línea 14: "        public List<Course> Courses { get; set; }"
			var source = @"
using DDD.Abstractions;
using System;
using System.Collections.Generic;
using Catalog;
namespace Students
{
    [AggregateRoot]
    [BoundedContext(""Students"")]
    public class Student
    {
        [EntityId]
        public Guid Id { get; private set; }
        public List<Course> Courses { get; set; }
    }
}";
			var expected = new DiagnosticResult(DiagnosticDescriptors.NoCrossContextDirectReference)
				.WithSpan(14, 9, 14, 50)
				.WithArguments("Student", "Students", "Course", "Catalog");

			var test = new CSharpAnalyzerTest<CrossBoundedContextReferenceAnalyzer, DefaultVerifier>
			{
				TestState = { Sources = { source, CatalogSource, DddAttributes } }
			};
			test.ExpectedDiagnostics.Add(expected);
			await test.RunAsync();
		}
	}

	public class DDD012_PrivateMemberCrossBCTests
	{
		private const string DddAttributes = DDD.Analyzers.Tests.Helpers.AnalyzerTestHelper.DddAttributesSource;

		private const string CatalogSource = @"
using DDD.Abstractions;
using System;
namespace Catalog
{
    [AggregateRoot]
    [BoundedContext(""Catalog"")]
    public class Course
    {
        [EntityId]
        public Guid Id { get; private set; }
    }
}";

		// Happy paths

		[Fact]
		public async Task CampoPrivado_MismoBC_NoDiagnostico()
		{
			var source = @"
using DDD.Abstractions;
using System;
namespace Catalog
{
    [AggregateRoot]
    [BoundedContext(""Catalog"")]
    public class CourseCatalog
    {
        [EntityId]
        public Guid Id { get; private set; }
        private Course _current;
    }
}";
			var test = new CSharpAnalyzerTest<CrossBoundedContextReferenceAnalyzer, DefaultVerifier>
			{
				TestState = { Sources = { source, CatalogSource, DddAttributes } }
			};
			await test.RunAsync();
		}

		// Error paths — DDD012

		[Fact]
		public async Task CampoPrivado_OtroBC_ReportaDDD012()
		{
			var source = @"
using DDD.Abstractions;
using System;
using Catalog;
namespace Students
{
    [AggregateRoot]
    [BoundedContext(""Students"")]
    public class Student
    {
        [EntityId]
        public Guid Id { get; private set; }
        {|DDD012:private Course _currentCourse;|}
    }
}";
			var test = new CSharpAnalyzerTest<CrossBoundedContextReferenceAnalyzer, DefaultVerifier>
			{
				TestState = { Sources = { source, CatalogSource, DddAttributes } }
			};
			await test.RunAsync();
		}

		[Fact]
		public async Task PropiedadPrivada_OtroBC_ReportaDDD012()
		{
			var source = @"
using DDD.Abstractions;
using System;
using Catalog;
namespace Students
{
    [AggregateRoot]
    [BoundedContext(""Students"")]
    public class Student
    {
        [EntityId]
        public Guid Id { get; private set; }
        {|DDD012:private Course CurrentCourse { get; set; }|}
    }
}";
			var test = new CSharpAnalyzerTest<CrossBoundedContextReferenceAnalyzer, DefaultVerifier>
			{
				TestState = { Sources = { source, CatalogSource, DddAttributes } }
			};
			await test.RunAsync();
		}
	}
}
