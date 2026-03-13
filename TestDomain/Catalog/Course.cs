using DDD.Abstractions;
using System;
using System.Collections.Generic;

namespace TestDomain.Catalog
{
    [BoundedContext("Catalog")]  // DDD010: 'Course' declara su Bounded Context
    [AggregateRoot]
    public class Course
    {
        // Agregamos la propiedad con [EntityId] como identificador de la entidad. Regla DDD001
        [EntityId]
        public Guid Id { get; private set; }

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
    }
}
