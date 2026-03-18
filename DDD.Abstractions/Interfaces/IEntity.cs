namespace DDD.Abstractions
{
	/// <summary>
	/// Contrato para entidades DDD con identidad fuertemente tipada.
	/// Dos entidades con el mismo <typeparamref name="TId"/> son la misma entidad,
	/// independientemente de sus demás propiedades.
	/// </summary>
	/// <typeparam name="TId">Tipo del identificador único (Guid, int, string, Value Object, etc.).</typeparam>
	public interface IEntity<TId>
	{
		/// <summary>
		/// Identificador único de la entidad.
		/// </summary>
		TId Id { get; }
	}
}
