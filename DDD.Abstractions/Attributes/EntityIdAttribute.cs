using System;
using System.Collections.Generic;
using System.Text;

namespace DDD.Abstractions
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EntityIdAttribute : Attribute
    {
    }
}
