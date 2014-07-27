using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spike.Build
{
    internal sealed class Operation(uint id, string name, bool compressed)
    {
        internal uint Id { get; } = id; 
        internal string Name { get; } = name;
        internal bool Compressed { get; } = compressed;
        internal List<Member> Members { get; } = new List<Member>();
    }
}
