using System;
using DDD.Abstractions;

namespace TestDomain.Catalog
{
	[BoundedContext("Catalog")]
	[Entity]
	public class CourseModule : Entity<Guid>
	{
		[EntityId]
		public override Guid Id { get; protected set; }
		public string Title { get; set; }
		public string Description { get; set; }

		public static CourseModule Create(string title, string description)
		{
			return new CourseModule(Guid.NewGuid(), title, description);
		}

		private CourseModule(Guid id, string title, string description) : base(id)
		{
			Title = title;
			Description = description;
		}
	}
}