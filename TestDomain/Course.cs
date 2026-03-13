using DDD.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestDomain
{
    [Entity]
    internal class Course
    {
        // Agregamos la propiedad con [EntityId] para cumplir con la regla DDD001
        [EntityId]
        public Guid Id { get; private set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Course(Guid id, string name, string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }
    }
}
