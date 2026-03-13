using DDD.Abstractions;
using System;
using TestDomain.Catalog;
using TestDomain.SharedKernel;

namespace TestDomain.StudentManagment
{
	[BoundedContext("StudentManagment")]
	[AggregateRoot]
	public class Student
	{
		[EntityId]
		public Guid StudentId { get; private set; }

		public string FirstName { get; set; }
		public string LastName { get; set; }
		public DateTime EnrollmentDate { get; private set; }

		public Address Address { get; set; }

		private Course Course2 { get; set; } // ❌ DDD011: Student (StudentManagment) → Course (Catalog)
		public Course Course { get; set; } // ❌ DDD011: Student (StudentManagment) → Course (Catalog)

		public static Student Create(Guid studentId, string firstName, string lastName, Address address)
		{
			return new Student(studentId, firstName, lastName, address);
		}

		internal Student(Guid studentId, string firstName, string lastName, Address address)
		{
			StudentId = studentId;
			FirstName = firstName;
			LastName = lastName;
			EnrollmentDate = DateTime.UtcNow;
		}
	};
}
