using System;
using System.Collections.Generic;
using System.Text;

namespace DDD.Abstractions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class ValueObjectAttribute : Attribute
    {
    }
}
