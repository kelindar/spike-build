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
using System.Reflection;

namespace Spike.Build
{
    public class ProtocolModel
    {
        public List<Protocol>  Protocols { get; private set; }

        /// <summary>
        /// Builds a view model for a protocol
        /// </summary>
        public Protocol Mutate(Protocol protocol)
        {
            // Check if lists are assigned
            if (Protocols == null)
                Protocols = new List<Protocol>();

            // Protocols cloning
            var cloned = protocol.Clone();
            Parse(cloned);

            return cloned;
        }

        #region Parsing Methods

        /// <summary>
        /// Parses a protocol 
        /// </summary>
        private void Parse(Protocol protocol)
        {
            EnsureExists(protocol);
            foreach (var operation in protocol.Operation)
            {
                //EnsureExists(operation);

                operation.Parent = protocol;
                operation.Obsolete = operation.ObsoleteSpecified ? operation.Obsolete : false;
                operation.SuppressSecurity = operation.SuppressSecuritySpecified ? operation.SuppressSecurity : false;
                operation.Compression = operation.CompressionSpecified ? operation.Compression : Compression.None;
                operation.Direction = operation.DirectionSpecified ? operation.Direction : Direction.Pull;
                operation.Signature = operation.GetOperationSignature();
                operation.Key = operation.GetOperationKey();

                if (operation.Incoming != null && operation.Incoming.Member.Count == 0) operation.Incoming = null;
                if (operation.Outgoing != null && operation.Outgoing.Member.Count == 0) operation.Outgoing = null;
                AlreadyParsed = new List<string>();


                if (operation.Incoming != null)
                {
                    var packet = operation.Incoming;
                    packet.Parent = operation;
                    packet.Direction = PacketDirection.Incoming;
                    packet.Name = Packet.GetPacketName(operation, packet.Direction);
                    foreach(var element in packet.Member)
                        ParseElement(element);
                }

                if (operation.Outgoing != null)
                {
                    var packet = operation.Outgoing;
                    packet.Parent = operation;
                    packet.Direction = PacketDirection.Outgoing;
                    packet.Name = Packet.GetPacketName(operation, packet.Direction);
                    foreach (var element in packet.Member)
                        ParseElement(element);
                }
            }
        }

        /// <summary>
        /// Parses the element
        /// </summary>
        private static void ParseElement(Element element)
        {
            if (element.IsComplexType && !AlreadyParsed.Contains(element.Class))
            {
                AlreadyParsed.Add(element.Class);
                if(element.Member.Count > 0)
                    foreach (var sub in element.Member)
                        ParseElement(sub);  
            }
            
            element.Class = String.IsNullOrEmpty(element.Class) ? element.Type.ToString() : element.Class;
        }
        private static List<string> AlreadyParsed = new List<string>();
        #endregion

        #region Helper Methods

        public void EnsureExists(Protocol protocol)
        {
            if (Protocols.Contains(protocol))
                Protocols.Remove(protocol);
            Protocols.Add(protocol);
        }

        public List<ProtocolOperation> Operations
        {
            get
            {
                return this.Protocols
                    .SelectMany(protocol => protocol.GetOperations())
                    .OrderBy(operation => operation.Key)
                    .ToList();
            }
        }

        public List<ProtocolOperation> OperationsWithOutgoingPacket
        {
            get
            {
                return this.Protocols
                    .SelectMany(protocol => protocol.GetAllOperationsWithOutgoingPacket())
                    .OrderBy(operation => operation.Key)
                    .ToList();
            }
        }

        #endregion
    }
}
