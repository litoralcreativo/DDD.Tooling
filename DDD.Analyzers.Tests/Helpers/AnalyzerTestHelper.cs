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

    public interface IDomainEvent
    {
        System.DateTime OccurredOn { get; }
    }

    public interface IEntity<TId> where TId : System.IEquatable<TId>
    {
        TId Id { get; }
    }

    public interface IAggregateRoot<TId> : IEntity<TId> where TId : System.IEquatable<TId>
    {
        System.Collections.Generic.IReadOnlyList<IDomainEvent> DomainEvents { get; }
        void ClearDomainEvents();
    }

    public abstract class Entity<TId> : IEntity<TId> where TId : System.IEquatable<TId>
    {
        public virtual TId Id { get; protected set; }
        protected Entity(TId id) { Id = id; }
        protected Entity() { }
    }

    public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot<TId> where TId : System.IEquatable<TId>
    {
        private readonly System.Collections.Generic.List<IDomainEvent> _domainEvents = new System.Collections.Generic.List<IDomainEvent>();
        public System.Collections.Generic.IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
        protected AggregateRoot(TId id) : base(id) { }
        protected AggregateRoot() { }
        protected void RaiseDomainEvent(IDomainEvent domainEvent) { _domainEvents.Add(domainEvent); }
        public void ClearDomainEvents() => _domainEvents.Clear();
    }

    public abstract class ValueObject
    {
        protected abstract System.Collections.Generic.IEnumerable<object> GetEqualityComponents();
    }
}
";
    }
}
