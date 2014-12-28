/************************************************************************
*
* Copyright (C) 2009-2014 Misakai Ltd
*
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
* 
* You should have received a copy of the GNU General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
* 
*************************************************************************/

using System.Collections.Generic;

namespace Spike.Build
{
    /// <summary>
    /// Class that represents SECP operation.
    /// </summary>
    public sealed class Operation
    {
        /// <summary>
        /// Constructs a new instance of an object.
        /// </summary>
        /// <param name="id">The id of the operation.</param>
        /// <param name="name">The name of the operation.</param>
        /// <param name="compressed">Whether the operation is compressed or not.</param>
        public Operation (uint id, string name, bool compressed)
	    {
            this.Id = id;
            this.Name = name;
            this.Compressed = compressed;
            this.Members = new List<Member>();
	    }

        /// <summary>
        /// Gets the id of the operation.
        /// </summary>
        public uint Id 
        {
            get; 
            private set; 
        } 

        /// <summary>
        /// Gets the name of the operation.
        /// </summary>
        public string Name
        {
            get;
            private set;
        } 

        /// <summary>
        /// Gets whether the operation is compressed or not.
        /// </summary>
        public bool Compressed
        {
            get;
            private set;
        } 

        /// <summary>
        /// Gets the list of members.
        /// </summary>
        public List<Member> Members
        {
            get;
            private set;
        } 

    }
}
