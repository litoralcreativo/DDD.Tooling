using System;

namespace DDD.Abstractions
{
    /// <summary>
    /// Marca una clase como un Value Object.
    /// Un Value Object es un objeto sin identidad única, donde la igualdad se basa en sus valores.
    /// </summary>
    /// <remarks>
    /// Reglas aplicadas por DDD.Analyzers:
    /// - DDD004: Un ValueObject debe ser inmutable (sin setters públicos)
    /// - DDD007: Un ValueObject debe sobrescribir Equals
    /// - DDD008: Un ValueObject debe sobrescribir GetHashCode
    /// - DDD020: Un ValueObject decorado debe heredar de ValueObject (o implementar igualdad equivalente)
    /// 
    /// Si usas esta clase base, la igualdad se implementa automáticamente por valor:
    /// hereda de <see cref="ValueObject"/> e implementa <see cref="ValueObject.GetEqualityComponents"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class ValueObjectAttribute : Attribute
    {
    }
}
