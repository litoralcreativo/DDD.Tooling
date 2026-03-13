using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    DDD.Analyzers.EntityMustHaveEntityIdAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace DDD.Analyzers.Tests.Analyzers
{
    public class DDD001_EntityMustHaveEntityIdTests
    {
        private const string DddAttributes = DDD.Analyzers.Tests.Helpers.AnalyzerTestHelper.DddAttributesSource;

        // Happy paths

        [Fact]
        public async Task Entity_ConEntityId_NoDiagnostico()
        {
            var source = @"
using DDD.Abstractions;
using System;
[Entity]
public class Product
{
    [EntityId]
    public Guid Id { get; private set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
        }

        [Fact]
        public async Task Entity_ConMultiplesProps_YEntityId_NoDiagnostico()
        {
            var source = @"
using DDD.Abstractions;
using System;
[Entity]
public class Order
{
    [EntityId]
    public Guid OrderId { get; private set; }
    public string Description { get; set; }
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
        public async Task Entity_SinEntityId_ReportaDDD001()
        {
            var source = @"
using DDD.Abstractions;
[Entity]
public class {|DDD001:Product|}
{
    public string Name { get; set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
        }

        [Fact]
        public async Task Entity_Vacia_ReportaDDD001()
        {
            var source = @"
using DDD.Abstractions;
[Entity]
public class {|DDD001:EmptyEntity|} { }";
            await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
        }
    }
}
