using DDD.Abstractions;
using System;

namespace TestDomain.Catalog.Events
{
	[DomainEvent]
	[BoundedContext("Catalog")]
	public class CoursePublishedEvent
	{
		public Guid CourseId { get; }
		public string Title { get; }
		public DateTime OccurredOn { get; }

		public CoursePublishedEvent(Guid courseId, string title, DateTime occurredOn)
		{
			CourseId = courseId;
			Title = title;
			OccurredOn = occurredOn;
		}
	}
}
