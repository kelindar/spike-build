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

namespace Spike.Build
{
    /// <summary>
    /// Represents a member type.
    /// </summary>
    internal sealed class Member
    {       
        /// <summary>
        /// Constructs a new instance of an object.
        /// </summary>
        public Member(string name, string type, bool isList, bool isCustom)
	    {
            this.Name = name;
            this.Type = type;
            this.IsList = isList;
            this.IsCustom = isCustom;
	    }

        /// <summary>
        /// Gets the name of the member.
        /// </summary>
        public string Name 
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the type of the member.
        /// </summary>
        public string Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether this is a list.
        /// </summary>
        public bool IsList
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether this is a complex type.
        /// </summary>
        public bool IsCustom
        {
            get;
            private set;
        }
    }
}
