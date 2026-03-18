using DDD.Abstractions;
using System;
using System.Collections.Generic;
using TestDomain.Catalog.Events;

namespace TestDomain.Catalog
{
    [BoundedContext("Catalog")]  // DDD010: 'Course' declara su Bounded Context
    [AggregateRoot]
    public class Course : AggregateRoot<Guid>
    {
        private List<CourseModule> _modules = new List<CourseModule>();
        public IReadOnlyCollection<CourseModule> Modules => _modules.AsReadOnly();

        public string Name { get; set; }

        public string Description { get; set; }

        // Método de fábrica para crear instancias de Course. Regla DDD009
        public static Course Create(Guid id, string name, string description)
        {
            return new Course(id, name, description);
        }

        // Constructor privado para forzar el uso del método de fábrica Create. Regla DDD009
        private Course(Guid id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        public void AddModule(CourseModule module)
        {
            if (module == null)
                throw new ArgumentNullException(nameof(module));

            _modules.Add(module);
        }

        public void PublishCourse()
        {
            // Lógica de negocio para publicar el curso (ej. validar que tenga módulos, etc.)
            if (_modules.Count == 0)
                throw new InvalidOperationException("Cannot publish a course without modules.");

            // Aquí se podría agregar lógica adicional, como cambiar el estado del curso a "Publicado", etc.

            // Regla DDD012: Al publicar el curso, se genera un evento de dominio
            RaiseDomainEvent(new CoursePublishedEvent(Id, Name, DateTime.UtcNow));
        }
    }
}
