using System.Collections.Generic;
using DDD.Abstractions;

namespace TestDomain.SharedKernel
{
	[SharedKernel] // DDD010: 'Address' es un concepto compartido, por lo que se marca como SharedKernel
	[ValueObject]
	public class Address : ValueObject
	{
		public string Street { get; }
		public string City { get; }
		public string PostalCode { get; }
		public string Country { get; }

		public Address(string street, string city, string postalCode, string country)
		{
			Street = street;
			City = city;
			PostalCode = postalCode;
			Country = country;
		}

		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return Street;
			yield return City;
			yield return PostalCode;
			yield return Country;
		}
	}
}
