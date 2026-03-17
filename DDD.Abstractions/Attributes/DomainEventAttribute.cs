using System;

namespace DDD.Abstractions
{
	/// <summary>
	/// Marca una clase como un Domain Event.
	/// Un Domain Event representa algo que ocurrió en el dominio y que
	/// es relevante para otras partes del sistema.
	/// </summary>
	/// <remarks>
	/// Reglas aplicadas por DDD.Analyzers:
	/// - DDD014: Un DomainEvent debe ser inmutable (sin setters públicos)
	/// - DDD015: Un DomainEvent debe tener una propiedad OccurredOn (DateTime/DateTimeOffset)
	/// - DDD016: Un DomainEvent debe declarar su Bounded Context
	/// </remarks>
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class DomainEventAttribute : Attribute
	{
	}
}
