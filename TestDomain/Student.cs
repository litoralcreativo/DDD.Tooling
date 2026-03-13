using DDD.Abstractions;
using System;

namespace TestDomain
{
	/// <summary>
	/// Ejemplo de un AggregateRoot correcto
	/// </summary>
	[AggregateRoot]
	public class Student
	{
		[EntityId]
		public Guid StudentId { get; private set; }

		public string FirstName { get; set; }
		public string LastName { get; set; }
		public DateTime EnrollmentDate { get; private set; }

		public Student(Guid studentId, string firstName, string lastName)
		{
			StudentId = studentId;
			FirstName = firstName;
			LastName = lastName;
			EnrollmentDate = DateTime.UtcNow;
		}
	};
}
