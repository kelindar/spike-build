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
    /// Represents a complex type.
    /// </summary>
    internal sealed class CustomType
    {
        /// <summary>
        /// Constructs a new instance of an object.
        /// </summary>
        public CustomType (string name)
	    {
            this.Name = name;
            this.Members = new List<Member>();
	    }

        /// <summary>
        /// Gets the name of the type.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the members of the type.
        /// </summary>
        public List<Member> Members
        {
            get;
            private set;
        }

    }
}
