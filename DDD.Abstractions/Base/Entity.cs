using System;

namespace DDD.Abstractions
{
	/// <summary>
	/// Clase base para entidades DDD con identidad fuertemente tipada.
	/// La igualdad se basa en el <see cref="Id"/>, no en la referencia de objeto.
	/// </summary>
	/// <typeparam name="TId">Tipo del identificador único.</typeparam>
	public abstract class Entity<TId> : IEntity<TId>
		where TId : IEquatable<TId>
	{
		/// <inheritdoc/>
		public virtual TId Id { get; protected set; }
		protected Entity(TId id)
		{
			if (id == null) throw new ArgumentNullException(nameof(id));
			Id = id;
		}

		/// <summary>Constructor sin parámetros para frameworks de persistencia (EF Core, etc.).</summary>
		protected Entity() { }

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj is null) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;

			var other = (Entity<TId>)obj;
			return Id != null && Id.Equals(other.Id);
		}

		/// <inheritdoc/>
		public override int GetHashCode() => Id == null ? 0 : HashCode(Id);

		public static bool operator ==(Entity<TId> left, Entity<TId> right)
			=> left?.Equals(right) ?? right is null;

		public static bool operator !=(Entity<TId> left, Entity<TId> right)
			=> !(left == right);

		// netstandard2.0 no tiene HashCode.Combine; usamos el hash del Id directamente.
		private static int HashCode(TId id) => id.GetHashCode();
	}
}
