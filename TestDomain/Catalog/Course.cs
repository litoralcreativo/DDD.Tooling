using DDD.Abstractions;
using System;

namespace TestDomain.Catalog
{
    [BoundedContext("Catalog")]  // DDD010: 'Course' declara su Bounded Context
    [AggregateRoot]
    public class Course
    {
        // Agregamos la propiedad con [EntityId] como identificador de la entidad. Regla DDD001
        [EntityId]
        public Guid Id { get; private set; }

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
    }
}
