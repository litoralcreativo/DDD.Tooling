using DDD.Abstractions;
using System;

namespace TestDomain.Examples
{
	/// <summary>
	/// Ejemplo de ValueObject con diferentes tipos de datos
	/// para probar el Code Fix mejorado
	/// </summary>
	[ValueObject]
	public class Price
	{
		// Tipo de referencia (string)
		public string Currency { get; }

		// Tipo de valor (decimal) - NO nullable
		public decimal Amount { get; }

		// Tipo de valor (DateTime) - NO nullable
		public DateTime CreatedAt { get; }

		// Tipo de valor nullable (int?)
		public int? DiscountPercent { get; }

		// Tipo de referencia nullable (string)
		public string Description { get; }

		public Price(string currency, decimal amount, DateTime createdAt, int? discountPercent = null, string description = null)
		{
			if (string.IsNullOrWhiteSpace(currency))
				throw new ArgumentException("Currency cannot be empty", nameof(currency));

			if (amount < 0)
				throw new ArgumentException("Amount cannot be negative", nameof(amount));

			Currency = currency;
			Amount = amount;
			CreatedAt = createdAt;
			DiscountPercent = discountPercent;
			Description = description;
		}

		public override bool Equals(object obj)
		{
			if (obj is Price other)
			{
				return Currency == other.Currency && Amount == other.Amount && CreatedAt == other.CreatedAt && DiscountPercent == other.DiscountPercent && Description == other.Description;
			}
			return false;
		}
		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 23 + (Currency?.GetHashCode() ?? 0);
				hash = hash * 23 + Amount.GetHashCode();
				hash = hash * 23 + CreatedAt.GetHashCode();
				hash = hash * 23 + (DiscountPercent?.GetHashCode() ?? 0);
				hash = hash * 23 + (Description?.GetHashCode() ?? 0);
				return hash;
			}
		}
		// ⚠️ Intencionalmente sin Equals y GetHashCode para probar el Code Fix
		// El Code Fix debería generar:
		// - Currency?.GetHashCode() ?? 0  (string - referencia)
		// - Amount.GetHashCode()           (decimal - valor no nullable)
		// - CreatedAt.GetHashCode()        (DateTime - valor no nullable)
		// - DiscountPercent?.GetHashCode() ?? 0  (int? - nullable)
		// - Description?.GetHashCode() ?? 0      (string - referencia nullable)
	}
}
