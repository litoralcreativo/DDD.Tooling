using System.Collections.Generic;
using System.Linq;

namespace DDD.Abstractions
{
	/// <summary>
	/// Clase base para Value Objects DDD.
	/// La igualdad se basa en los valores de sus componentes, no en la referencia de objeto.
	/// Implementa <see cref="Equals"/>, <see cref="GetHashCode"/> y los operadores == / !=
	/// delegando en <see cref="GetEqualityComponents"/>.
	/// </summary>
	public abstract class ValueObject
	{
		/// <summary>
		/// Devuelve los componentes que definen la igualdad de este Value Object.
		/// Implementar retornando cada propiedad relevante:
		/// <code>
		/// protected override IEnumerable&lt;object&gt; GetEqualityComponents()
		/// {
		///     yield return Amount;
		///     yield return Currency;
		/// }
		/// </code>
		/// </summary>
		protected abstract IEnumerable<object> GetEqualityComponents();

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			if (obj is null) return false;
			if (obj.GetType() != GetType()) return false;

			var other = (ValueObject)obj;
			return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			return GetEqualityComponents()
				.Aggregate(17, (hash, component) =>
					hash * 31 + (component?.GetHashCode() ?? 0));
		}

		public static bool operator ==(ValueObject left, ValueObject right)
			=> left?.Equals(right) ?? right is null;

		public static bool operator !=(ValueObject left, ValueObject right)
			=> !(left == right);
	}
}
