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
