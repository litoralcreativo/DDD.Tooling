using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace DDD.Analyzers.Tests.Helpers
{
    /// <summary>
    /// Atributos DDD inyectados como segundo archivo en cada test.
    /// Se mantienen en archivo separado para que los usings del test no colisionen.
    /// </summary>
    public static class AnalyzerTestHelper
    {
        public const string DddAttributesSource = @"
namespace DDD.Abstractions
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class EntityAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class AggregateRootAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class ValueObjectAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
    public class EntityIdAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class BoundedContextAttribute : System.Attribute
    {
        public BoundedContextAttribute(string name) { }
    }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class SharedKernelAttribute : System.Attribute { }

    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class DomainEventAttribute : System.Attribute { }
}
";
    }
}
