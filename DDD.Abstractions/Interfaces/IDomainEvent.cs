using System;

namespace DDD.Abstractions
{
	/// <summary>
	/// Contrato de runtime para Domain Events.
	/// Garantiza que todo evento de dominio exponga cuándo ocurrió.
	/// </summary>
	/// <remarks>
	/// Úsalo en combinación con el atributo <see cref="DomainEventAttribute"/>:
	/// el atributo valida la estructura en tiempo de compilación (analyzers),
	/// esta interfaz permite trabajar con eventos de forma polimórfica en runtime.
	/// </remarks>
	public interface IDomainEvent
	{
		/// <summary>
		/// Fecha y hora (UTC) en la que ocurrió el evento.
		/// </summary>
		DateTime OccurredOn { get; }
	}
}
