#region Copyright (c) 2009-2013 Misakai Ltd.
/*************************************************************************
 * 
 * ROMAN ATACHIANTS - CONFIDENTIAL
 * ===============================
 * 
 * THIS PROGRAM IS CONFIDENTIAL  AND PROPRIETARY TO  ROMAN  ATACHIANTS AND 
 * MAY  NOT  BE  REPRODUCED,  PUBLISHED  OR  DISCLOSED TO  OTHERS  WITHOUT 
 * ROMAN ATACHIANTS' WRITTEN AUTHORIZATION.
 *
 * COPYRIGHT (c) 2009 - 2012. THIS WORK IS UNPUBLISHED.
 * All Rights Reserved.
 * 
 * NOTICE:  All information contained herein is,  and remains the property 
 * of Roman Atachiants  and its  suppliers,  if any. The  intellectual and 
 * technical concepts contained herein are proprietary to Roman Atachiants
 * and  its suppliers and may be  covered  by U.S.  and  Foreign  Patents, 
 * patents in process, and are protected by trade secret or copyright law.
 * 
 * Dissemination of this information  or reproduction  of this material is 
 * strictly  forbidden  unless prior  written permission  is obtained from 
 * Roman Atachiants.
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
