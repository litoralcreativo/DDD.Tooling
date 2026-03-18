using System;
using System.Collections.Generic;

namespace DDD.Abstractions
{
	/// <summary>
	/// Clase base para Aggregate Roots DDD.
	/// Extiende <see cref="Entity{TId}"/> con la capacidad de acumular
	/// Domain Events para ser despachados por la infraestructura tras la persistencia.
	/// </summary>
	/// <typeparam name="TId">Tipo del identificador único del aggregate.</typeparam>
	public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot<TId>
		where TId : IEquatable<TId>
	{
		private readonly List<IDomainEvent> _domainEvents = new List<IDomainEvent>();

		/// <inheritdoc/>
		public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

		protected AggregateRoot(TId id) : base(id) { }

		/// <summary>Constructor sin parámetros para frameworks de persistencia (EF Core, etc.).</summary>
		protected AggregateRoot() { }

		/// <summary>
		/// Registra un Domain Event para ser despachado tras la persistencia.
		/// Llamar desde los métodos de negocio del aggregate cuando ocurre algo relevante.
		/// </summary>
		/// <param name="domainEvent">El evento ocurrido.</param>
		protected void RaiseDomainEvent(IDomainEvent domainEvent)
		{
			if (domainEvent == null) throw new ArgumentNullException(nameof(domainEvent));
			_domainEvents.Add(domainEvent);
		}

		/// <inheritdoc/>
		public void ClearDomainEvents() => _domainEvents.Clear();
	}
}
