using System;

namespace DDD.Abstractions
{
	/// <summary>
	/// Declara que una clase pertenece al Shared Kernel — conceptos compartidos entre
	/// múltiples Bounded Contexts. Los tipos SharedKernel pueden ser referenciados
	/// desde cualquier Bounded Context, pero no pueden depender de ninguno.
	/// </summary>
	/// <example>
	/// [SharedKernel]
	/// [ValueObject]
	/// public class Address { ... }
	/// </example>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class SharedKernelAttribute : Attribute
	{
	}
}
