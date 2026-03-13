using DDD.Abstractions;
using System;
using System.Collections.Generic;
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
