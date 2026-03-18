using DDD.Abstractions;
using System;

namespace TestDomain.Catalog.Events
{
	[DomainEvent]
	[BoundedContext("Catalog")]  // DDD010: 'CoursePublishedEvent' declara su Bounded Context
	public class CoursePublishedEvent : IDomainEvent
	{
		public Guid CourseId { get; }
		public string Title { get; }
		public DateTime OccurredOn { get; } // Regla DDD015: Los eventos de dominio deben incluir la fecha y hora de su ocurrencia

		public CoursePublishedEvent(Guid courseId, string title, DateTime occurredOn)
		{
			CourseId = courseId;
			Title = title;
			OccurredOn = occurredOn;
		}
	}
}
