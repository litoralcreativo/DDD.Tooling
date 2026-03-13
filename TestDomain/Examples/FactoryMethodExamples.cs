using DDD.Abstractions;
using System;

namespace TestDomain.Examples
{
	/// <summary>
	/// Ejemplo de Entity CON constructor público - Recibirá sugerencia DDD009
	/// </summary>
	[Entity]
	public class ProductWithPublicConstructor
	{
		[EntityId]
		public Guid Id { get; private set; }

		public string Name { get; private set; }
		public decimal Price { get; private set; }

		// ℹ️ DDD009: Tiene constructor público - Se sugiere Factory Method
		public ProductWithPublicConstructor(string name, decimal price)
		{
			Id = Guid.NewGuid();
			Name = name;
			Price = price;
		}
	}

	/// <summary>
	/// Ejemplo de Entity SIN constructor público - NO recibirá sugerencia
	/// </summary>
	[Entity]
	public class ProductWithPrivateConstructor
	{
		[EntityId]
		public Guid Id { get; private set; }

		public string Name { get; private set; }
		public decimal Price { get; private set; }

		// ✅ Constructor privado - ¡Perfecto!
		private ProductWithPrivateConstructor() { }

		// ✅ Factory Method - ¡Excelente patrón!
		public static ProductWithPrivateConstructor Create(string name, decimal price)
		{
			return new ProductWithPrivateConstructor
			{
				Id = Guid.NewGuid(),
				Name = name,
				Price = price
			};
		}
	}

	/// <summary>
	/// Ejemplo de Entity CON Factory Method - NO recibirá sugerencia
	/// </summary>
	[Entity]
	public class ProductWithFactoryMethod
	{
		[EntityId]
		public Guid Id { get; private set; }

		public string Name { get; private set; }
		public decimal Price { get; private set; }

		// ✅ Tiene constructor público PERO también tiene Factory Method
		// El analizador NO sugerirá nada porque ya sigue el patrón
		public ProductWithFactoryMethod(Guid id, string name, decimal price)
		{
			Id = id;
			Name = name;
			Price = price;
		}

		// ✅ Factory Method presente - Sin sugerencia
		public static ProductWithFactoryMethod Create(string name, decimal price)
		{
			return new ProductWithFactoryMethod(Guid.NewGuid(), name, price);
		}
	}

	/// <summary>
	/// Ejemplo de AggregateRoot CON constructor público - Recibirá sugerencia DDD009
	/// </summary>
	[AggregateRoot]
	public class OrderWithPublicConstructor
	{
		[EntityId]
		public Guid OrderId { get; private set; }

		public DateTime OrderDate { get; private set; }

		// ℹ️ DDD009: Tiene constructor público - Se sugiere Factory Method
		public OrderWithPublicConstructor()
		{
			OrderId = Guid.NewGuid();
			OrderDate = DateTime.UtcNow;
		}
	}
}
