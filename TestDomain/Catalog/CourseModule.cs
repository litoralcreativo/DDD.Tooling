using System;
using DDD.Abstractions;

namespace TestDomain.Catalog
{
	[BoundedContext("Catalog")]
	[Entity]
	public class CourseModule
	{

		[EntityId]
		public Guid Id { get; private set; }
		public string Title { get; set; }
		public string Description { get; set; }

		public static CourseModule Create(string title, string description)
		{
			return new CourseModule(title, description);
		}

		private CourseModule(string title, string description)
		{
			Title = title;
			Description = description;
		}
	}
}