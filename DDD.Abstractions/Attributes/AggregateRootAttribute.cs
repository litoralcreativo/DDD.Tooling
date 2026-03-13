using System;
using System.Collections.Generic;
using System.Text;

namespace DDD.Abstractions
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class AggregateRootAttribute : Attribute
    {
    }
}
