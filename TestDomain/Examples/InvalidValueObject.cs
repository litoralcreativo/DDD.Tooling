using DDD.Abstractions;

namespace TestDomain.Examples
{
	/// <summary>
	/// EJEMPLO DE ERROR: ValueObject con setter público
	/// Este código generará warning DDD004
	/// </summary>
	[ValueObject]
	public class InvalidValueObject
	{
		// ❌ ERROR: ValueObject no debe tener setters públicos
		public string Value { get; set; }

		public InvalidValueObject(string value)
		{
			Value = value;
		}
	}
}
