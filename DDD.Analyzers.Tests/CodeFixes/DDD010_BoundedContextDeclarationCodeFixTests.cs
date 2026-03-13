using System.Threading.Tasks;
using DDD.Analyzers.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace DDD.Analyzers.Tests.CodeFixes
{
	/// <summary>
	/// Tests para BoundedContextDeclarationCodeFixProvider (DDD010):
	/// El diagnóstico se reporta en classDeclaration.Identifier.
	///   - Agrega [BoundedContext("BC")] inferido desde el namespace
	///   - Agrega [SharedKernel]
	/// </summary>
	public class DDD010_BoundedContextDeclarationCodeFixTests
	{
		private const string DddAttributes = Helpers.AnalyzerTestHelper.DddAttributesSource;

		[Fact]
		public async Task DDD010_AgregaBoundedContextInferidoDesdeNamespace()
		{
			// El namespace es "MyApp.Catalog" → BC inferido = "Catalog"
			var before = @"
using DDD.Abstractions;
namespace MyApp.Catalog
{
    [Entity]
    public class {|DDD010:Course|}
    {
    }
}";
			var after = @"
using DDD.Abstractions;
namespace MyApp.Catalog
{
    [BoundedContext(""Catalog"")]
    [Entity]
    public class Course
    {
    }
}";
			var test = new CSharpCodeFixTest<BoundedContextDeclarationAnalyzer, BoundedContextDeclarationCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes } },
				FixedState = { Sources = { after, DddAttributes } },
				CodeActionEquivalenceKey = nameof(BoundedContextDeclarationCodeFixProvider) + "_BC"
			};
			await test.RunAsync();
		}

		[Fact]
		public async Task DDD010_AgregaSharedKernel()
		{
			var before = @"
using DDD.Abstractions;
namespace MyApp.SharedKernel
{
    [ValueObject]
    public class {|DDD010:Money|}
    {
    }
}";
			var after = @"
using DDD.Abstractions;
namespace MyApp.SharedKernel
{
    [SharedKernel]
    [ValueObject]
    public class Money
    {
    }
}";
			var test = new CSharpCodeFixTest<BoundedContextDeclarationAnalyzer, BoundedContextDeclarationCodeFixProvider, DefaultVerifier>
			{
				TestState = { Sources = { before, DddAttributes } },
				FixedState = { Sources = { after, DddAttributes } },
				CodeActionEquivalenceKey = nameof(BoundedContextDeclarationCodeFixProvider) + "_SK"
			};
			await test.RunAsync();
		}
	}
}
