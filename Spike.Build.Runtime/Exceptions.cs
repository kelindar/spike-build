using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spike.Build
{
    /// <summary>
    /// Represents a malformed protocol.
    /// </summary>
    public class ProtocolMalformedException : Exception
    {
        /// <summary>
        /// Constructs a new exception.
        /// </summary>
        /// <param name="message">The message of the exception</param>
        public ProtocolMalformedException(string message) : base(message) { }

    }
}
