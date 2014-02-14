#region Copyright (c) 2009-2014 Misakai Ltd.
/*************************************************************************
* 
* This file is part of Spike.Build Project.
*
* Spike.Build is free software: you can redistribute it and/or modify it 
* under the terms of the GNU General Public License as published by the 
* Free Software Foundation, either version 3 of the License, or (at your
* option) any later version.
*
* Foobar is distributed in the hope that it will be useful, but WITHOUT 
* ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
* or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public 
* License for more details.
* 
* You should have received a copy of the GNU General Public License 
* along with Foobar. If not, see http://www.gnu.org/licenses/.
*************************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Spike.Build
{

    public partial class Packet
    {
        public ProtocolOperation Parent { get; set; }
        public PacketDirection Direction { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Gets the list of members
        /// </summary>
        public List<Element> GetMembers()
        {
            return this.Member;
        }

        /// <summary>
        /// Gets the list of members recursively
        /// </summary>
        public List<Element> GetAllMembers()
        {
            var result = new List<Element>();
            if (this.Member.Count > 0)
            {
                foreach (var element in Member)
                {
                    if (element.IsComplexType)
                        result.AddRange(element.GetAllMembers(true));
                    else
                        result.Add(element);
                }
            }
            return result;
        }

        public static string GetPacketName(ProtocolOperation operation, PacketDirection direction)
        {
            var firstName =  operation.Name;
            var lastName = direction == PacketDirection.Incoming ? "Request" : "Inform";
            
            return firstName + lastName;
        }

        public override string ToString()
        {
            return String.Format("Packet: {0}", Name);
        }
    }


}
