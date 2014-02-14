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

namespace Spike.Build
{
    public partial class Protocol
    {
        public string RawSpml { get; set; }
        public List<ProtocolOperation> Operation { get { return Operations.Operation; } }

        public List<Element> GetAllElements()
        {
            var members = GetAllPackets()
                .SelectMany(packet => packet.GetAllMembers())
                .Distinct()
                .ToList();
            return members;
        }


        public string GetEncodedSpml()
        {
            try
            {
                byte[] encoded = new byte[RawSpml.Length];
                encoded = System.Text.Encoding.UTF8.GetBytes(RawSpml);
                string encodedData = Convert.ToBase64String(encoded);
                return encodedData;
            }
            catch (Exception e)
            {
                throw new Exception("Error in base64Encode" + e.Message);
            }
        }

        public List<Element> GetAllComplexElementsDistinct()
        {
            var result = new List<Element>();
            var elements = GetAllElements()
                .Where(element => element.IsComplexType).ToList();
            foreach (var element in elements)
            {
                var containsAlready = false;
                foreach (var el in result)
                {
                    if (el.Class == element.Class)
                    {
                        containsAlready = true;
                        break;
                    }
                }
                if(!containsAlready)
                    result.Add(element);
            }
            return result;
        }

        public List<ProtocolOperation> GetOperations()
        {
            return Operation;
        }

        public List<Packet> GetAllIncomingPackets()
        {
            return Operation
                .Where(operation => operation.Incoming != null)
                .Where(operation => operation.Incoming.Member.Count > 0)
                .Select(operation => operation.Incoming)
                .ToList();
        }

        public List<Packet> GetAllOutgoingPackets()
        {
            return Operation
                .Where(operation => operation.Outgoing != null)
                .Where(operation => operation.Outgoing.Member.Count > 0)
                .Select(operation => operation.Outgoing)
                .ToList();
        }

        public List<ProtocolOperation> GetAllOperationsWithOutgoingPacket()
        {
            return Operation
                .Where(operation => operation.Outgoing != null)
                .Where(operation => operation.Outgoing.Member.Count > 0)
                .ToList();
        }

        public List<ProtocolOperation> GetAllOperationsWithIncomingPacket()
        {
            return Operation
                .Where(operation => operation.Incoming != null)
                .Where(operation => operation.Incoming.Member.Count > 0)
                .ToList();
        }


        public List<Packet> GetAllPackets()
        {
            var packets = new List<Packet>();
            packets.AddRange(GetAllIncomingPackets());
            packets.AddRange(GetAllOutgoingPackets());
            return packets;
        }

        public List<string> GetAllNamespaces()
        {
            var result = new List<string>();
            result.AddRange(GetAllElements().Select(element => element.Namespace));
            result.AddRange(Operation.Select(operation => operation.Namespace));
            result.Add(Namespace);
            result = result.Distinct().Where(s => !String.IsNullOrEmpty(s)).ToList();

            return result;
        }

        #region Overrides
        public override string ToString()
        {
            return String.Format("Protocol: {0}", Name);
        }

        public override bool Equals(object obj)
        {
            var right = obj as Protocol;
            if (right == null)
                return false;
            if (right.Name == this.Name)
                return true;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
        #endregion
    }
}
