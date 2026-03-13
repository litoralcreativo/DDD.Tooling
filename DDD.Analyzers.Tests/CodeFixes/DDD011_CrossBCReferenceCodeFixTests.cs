using System.Threading.Tasks;
using DDD.Analyzers;
using DDD.Analyzers.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace DDD.Analyzers.Tests.CodeFixes
{
	/// <summary>
	/// Tests para CrossBoundedContextReferenceCodeFixProvider (DDD011):
	///   - Tipo simple: reemplaza la propiedad por el Id del tipo referenciado
	///   - Colección genérica: reemplaza List&lt;Course&gt; por List&lt;Guid&gt;
	/// </summary>
	public class DDD011_CrossBoundedContextReferenceCodeFixTests
	{
		private const string DddAttributes = Helpers.AnalyzerTestHelper.DddAttributesSource;

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

		[Fact]
		public async Task DDD011_TipoSimple_ReemplazaPorId()
		{
			var before = @"
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
			var after = @"
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
        public Guid CourseId { get; set; }
    }
}";
			var expected = new DiagnosticResult(DiagnosticDescriptors.NoCrossContextDirectReference)
				.WithSpan(13, 9, 13, 51)
				.WithArguments("Student", "Students", "Course", "Catalog");

			var test = new CSharpCodeFixTest<CrossBoundedContextReferenceAnalyzer, CrossBoundedContextReferenceCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, CatalogSource, DddAttributes } },
				FixedState = { Sources = { after, CatalogSource, DddAttributes } },
				CodeActionEquivalenceKey = nameof(CrossBoundedContextReferenceCodeFixProvider) + "_Simple"
			};
			test.ExpectedDiagnostics.Add(expected);
			await test.RunAsync();
		}

		[Fact]
		public async Task DDD011_ColeccionGenerica_ReemplazaPorListDeIds()
		{
			var before = @"
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
			var after = @"
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
        public List<Guid> CourseIds { get; set; }
    }
}";
			var expected = new DiagnosticResult(DiagnosticDescriptors.NoCrossContextDirectReference)
				.WithSpan(14, 9, 14, 50)
				.WithArguments("Student", "Students", "Course", "Catalog");

			var test = new CSharpCodeFixTest<CrossBoundedContextReferenceAnalyzer, CrossBoundedContextReferenceCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, CatalogSource, DddAttributes } },
				FixedState = { Sources = { after, CatalogSource, DddAttributes } },
				CodeActionEquivalenceKey = nameof(CrossBoundedContextReferenceCodeFixProvider) + "_Generic"
			};
			test.ExpectedDiagnostics.Add(expected);
			await test.RunAsync();
		}
	}
}
