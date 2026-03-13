using DDD.Abstractions;
using System;

namespace TestDomain
{
	[ValueObject]
	public class Currency
	{
		public string Symbol { get; }
		public decimal Value { get; }

		public Currency(string symbol, decimal value)
		{
			if (string.IsNullOrWhiteSpace(symbol))
				throw new ArgumentException("Currency symbol cannot be empty", nameof(symbol));

			if (value < 0)
				throw new ArgumentException("Currency value cannot be negative", nameof(value));

			Symbol = symbol;
			Value = value;
		}

		/// <summary>
		/// Formatea la moneda como string
		/// </summary>
		public override string ToString() => $"{Symbol} {Value:N2}";

		/// <summary>
		/// Compara por valor (patrón ValueObject)
		/// </summary>
		public override bool Equals(object obj)
		{
			if (obj is Currency other)
			{
				return Symbol == other.Symbol && Value == other.Value;
			}
			return false;
		}

		/// <summary>
		/// Hash basado en los valores
		/// </summary>
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + (Symbol?.GetHashCode() ?? 0);
				hash = hash * 23 + Value.GetHashCode(); // decimal es valor, no nullable
				return hash;
			}
		}

		/// <summary>
		/// Operador de igualdad
		/// </summary>
		public static bool operator ==(Currency left, Currency right)
		{
			if (ReferenceEquals(left, right)) return true;
			if (left is null || right is null) return false;
			return left.Equals(right);
		}

		/// <summary>
		/// Operador de desigualdad
		/// </summary>
		public static bool operator !=(Currency left, Currency right)
		{
			return !(left == right);
		}
	}
}
