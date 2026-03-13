using DDD.Abstractions;
using System;

namespace TestDomain.Examples
{
	/// <summary>
	/// EJEMPLO DE ERROR: Entity sin EntityId
	/// Este código generará error DDD001 si se descomenta
	/// </summary>
	// [Entity]
	// public class InvalidEntity
	// {
	//     // ❌ ERROR: No tiene propiedad con [EntityId]
	//     public string Name { get; set; }
	// }

	/// <summary>
	/// EJEMPLO DE ERROR: Clase que intenta ser Entity y ValueObject
	/// Este código generará error DDD005 si se descomenta
	/// </summary>
	// [Entity]
	// [ValueObject]
	// public class ConflictingAttributes
	// {
	//     [EntityId]
	//     public Guid Id { get; set; }
	// }
}
