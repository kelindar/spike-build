using System.Collections.Generic;

namespace Spike.Build
{
    internal sealed class ComplexType(string name)
    {
        internal string Name { get; } = name;
        internal List<Member> Members { get; } = new List<Member>();
    }
}
