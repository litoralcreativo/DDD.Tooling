using System;

namespace DDD.Abstractions
{
	/// <summary>
	/// Declara que una clase pertenece a un Bounded Context específico.
	/// Aplicar en clases decoradas con [Entity], [AggregateRoot] o [ValueObject].
	/// </summary>
	/// <example>
	/// [BoundedContext("Catalog")]
	/// [AggregateRoot]
	/// public class Course { ... }
	/// </example>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class BoundedContextAttribute : Attribute
	{
		/// <summary>
		/// Nombre del Bounded Context al que pertenece esta clase.
		/// </summary>
		public string Name { get; }

		public BoundedContextAttribute(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("El nombre del Bounded Context no puede estar vacío.", nameof(name));

			Name = name;
		}
	}
}
