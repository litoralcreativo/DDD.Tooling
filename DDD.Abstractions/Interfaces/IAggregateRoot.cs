using System.Collections.Generic;

namespace DDD.Abstractions
{
	/// <summary>
	/// Contrato para Aggregate Roots DDD.
	/// Un Aggregate Root es la única entrada al aggregate y es responsable
	/// de mantener sus invariantes y registrar los Domain Events ocurridos.
	/// </summary>
	/// <typeparam name="TId">Tipo del identificador único del aggregate.</typeparam>
	public interface IAggregateRoot<TId> : IEntity<TId>
	{
		/// <summary>
		/// Eventos de dominio acumulados desde la última vez que se despacharon.
		/// La infraestructura (UnitOfWork/repositorio) los lee y despacha tras persistir.
		/// </summary>
		IReadOnlyList<IDomainEvent> DomainEvents { get; }

		/// <summary>
		/// Limpia la lista de eventos acumulados.
		/// Debe llamarse después de despacharlos para evitar re-procesarlos.
		/// </summary>
		void ClearDomainEvents();
	}
}
