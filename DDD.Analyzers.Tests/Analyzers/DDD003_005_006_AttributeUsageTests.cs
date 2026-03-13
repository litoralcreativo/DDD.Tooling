using System.Threading.Tasks;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.CSharpAnalyzerVerifier<
    DDD.Analyzers.DddAttributeUsageAnalyzer,
    Microsoft.CodeAnalysis.Testing.DefaultVerifier>;

namespace DDD.Analyzers.Tests.Analyzers
{
    public class DDD003_EntityIdOnPropertyTests
    {
        private const string DddAttributes = DDD.Analyzers.Tests.Helpers.AnalyzerTestHelper.DddAttributesSource;

        [Fact]
        public async Task EntityId_EnPropiedad_NoDiagnostico()
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
        public async Task EntityId_EnCampo_ReportaDDD003()
        {
            var source = @"
using DDD.Abstractions;
using System;
[Entity]
public class Product
{
    {|DDD003:[EntityId]
    public Guid _id;|}
}";
            await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
        }
    }

    public class DDD005_EntityAndValueObjectTests
    {
        private const string DddAttributes = DDD.Analyzers.Tests.Helpers.AnalyzerTestHelper.DddAttributesSource;

        [Fact]
        public async Task SoloEntity_NoDiagnostico()
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
        public async Task SoloValueObject_NoDiagnostico()
        {
            var source = @"
using DDD.Abstractions;
[ValueObject]
public class Money { public decimal Amount { get; } }";
            await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
        }

        [Fact]
        public async Task EntityYValueObject_ReportaDDD005()
        {
            var source = @"
using DDD.Abstractions;
using System;
[Entity]
[ValueObject]
public class {|DDD005:Product|}
{
    [EntityId]
    public Guid Id { get; private set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
        }
    }

    public class DDD006_AggregateRootAndValueObjectTests
    {
        private const string DddAttributes = DDD.Analyzers.Tests.Helpers.AnalyzerTestHelper.DddAttributesSource;

        [Fact]
        public async Task SoloAggregateRoot_NoDiagnostico()
        {
            var source = @"
using DDD.Abstractions;
using System;
[AggregateRoot]
public class Order
{
    [EntityId]
    public Guid Id { get; private set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
        }

        [Fact]
        public async Task AggregateRootYValueObject_ReportaDDD006()
        {
            var source = @"
using DDD.Abstractions;
using System;
[AggregateRoot]
[ValueObject]
public class {|DDD006:Order|}
{
    [EntityId]
    public Guid Id { get; private set; }
}";
            await VerifyCS.VerifyAnalyzerAsync(source + DddAttributes);
        }
    }
}
