using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    DDD.Analyzers.AggregateRootMustHaveEntityIdAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace DDD.Analyzers.Tests.Analyzers
{
    public class DDD002_AggregateRootMustHaveEntityIdTests
    {
        private const string DddAttributes = DDD.Analyzers.Tests.Helpers.AnalyzerTestHelper.DddAttributesSource;

        // Happy paths

        [Fact]
        public async Task AggregateRoot_ConEntityId_NoDiagnostico()
        {
            var source = @"
using DDD.Abstractions;
using System;
[AggregateRoot]
public class ShoppingCart
{
    [EntityId]
    public Guid CartId { get; private set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
        }

        [Fact]
        public async Task ClaseSinAtributo_NoDiagnostico()
        {
            var source = @"
using System;
public class PlainClass { public Guid Id { get; set; } }";
            await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
        }

        // Error paths

        [Fact]
        public async Task AggregateRoot_SinEntityId_ReportaDDD002()
        {
            var source = @"
using DDD.Abstractions;
[AggregateRoot]
public class {|DDD002:Order|}
{
    public string Description { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
        }

        [Fact]
        public async Task AggregateRoot_Vacio_ReportaDDD002()
        {
            var source = @"
using DDD.Abstractions;
[AggregateRoot]
public class {|DDD002:EmptyAggregate|} { }";
            await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
        }
    }
}
