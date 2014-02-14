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
using System.IO;
using Spike.Build;

namespace Spike.Build.Client.JavaScript
{
    public class JavaScriptPacketBuilder : ISubClientBuilder<Packet, JavaScriptBuilder>
    {
        public void GenerateCode(Packet packet, JavaScriptBuilder builder)
        {
            using (var writer = new CodeWriter())
            {
                GeneratePacket(packet, writer);
                builder.AddSourceFile(builder.SrcOutputPath, String.Format(@"{0}.js", packet.Name), writer);
            }
        }

        #region GeneratePacket

        internal static void GeneratePacket(Packet packet, TextWriter writer)
        {
            writer.WriteLine("function {0}()", packet.Name); // Begin class
            writer.WriteLine("{");
            {
                // Generate fields
                packet.GetMembers().ForEach(prop =>
                    {
                        prop.GenerateProperty(writer);
                    });

                // Read/Write methods
                GeneratePacketWriteMethod(packet, writer);
                GeneratePacketReadMethod(packet, writer);
            }

            writer.WriteLine("};"); // End class
        }

        #endregion

        #region GeneratePacketReadMethod, GeneratePacketWriteMethod
        internal static void GeneratePacketWriteMethod(Packet packet, TextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("this.write = function(writer)");
            writer.WriteLine("{");
            var properties = packet.GetMembers();
            properties.GenerateWriteCode(writer);
            if (properties.Count() > 0)
            {
                var Compress = packet.Parent.Compression;
                if (Compress == Compression.Both || Compress == Compression.Incoming)
                {
                    writer.WriteLine("writer.compress();");
                }
            }
            writer.WriteLine("};");
            writer.WriteLine();
        }

        internal static void GeneratePacketReadMethod(Packet packet, TextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("this.read = function(reader)");
            writer.WriteLine("{");
            var properties = packet.GetMembers();
            if (properties.Count() > 0)
            {
                var Compress = packet.Parent.Compression;
                if (Compress == Compression.Both || Compress == Compression.Outgoing)
                {
                    writer.WriteLine("reader.decompress();");
                }
            }
            properties.GenerateReadCode(writer);
            writer.WriteLine("};");
            writer.WriteLine();
        }
        #endregion


    }
}
