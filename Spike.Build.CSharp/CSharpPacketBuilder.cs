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
using Spike.Build.Client.CSharp;

namespace Spike.Build.Client
{
    public class CSharpPacketBuilder : ISubClientBuilder<Packet, CSharpBuilder>
    {
        public void GenerateCode(Packet packet, CSharpBuilder builder)
        {
            using (var writer = new CodeWriter())
            {

                GeneratePacket(packet, writer);
                builder.AddSourceFile(builder.SrcOutputPath, String.Format(@"{0}.cs", packet.Name), writer);
            }
        }



        #region GeneratePacket

        internal static void GeneratePacket(Packet packet, TextWriter writer)
        {
            CSharpBuilder.GenerateHeader(writer);
            writer.WriteLine("namespace Spike.Network"); // Begin package
            writer.WriteLine("{");
            writer.WriteLine("public class {0} : Packet, IPacket", packet.Name); // Begin class
            writer.WriteLine("{");
            {
                // Generate constructors
                GenerateConstructors(packet, writer);

                // Generate fields
                packet.GetMembers().ForEach(prop =>
                    {
                        prop.GenerateProperty(writer);
                    });

                // Read/Write methods
                GeneratePacketWriteMethod(packet, writer);
                GeneratePacketReadMethod(packet, writer);
            }

            writer.WriteLine("}"); // End class
            writer.WriteLine("}"); // End package 


        }

        #endregion

        #region GeneratePacketReadMethod, GeneratePacketWriteMethod
        internal static void GeneratePacketWriteMethod(Packet packet, TextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("public void Write(PacketWriter Writer)");
            writer.WriteLine("{");
            var properties = packet.GetMembers();
            properties.GenerateWriteCode(writer);
            if (properties.Count() > 0)
            {
                var Compress = packet.Parent.Compression;
                if (Compress == Compression.Both || Compress == Compression.Incoming)
                {
                    writer.WriteLine("Writer.Compress();");
                }
            }
            writer.WriteLine("}");
            writer.WriteLine();
        }

        internal static void GeneratePacketReadMethod(Packet packet, TextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("public void Read(PacketReader Reader)");
            writer.WriteLine("{");
            var properties = packet.GetMembers();
            if (properties.Count() > 0)
            {
                var Compress = packet.Parent.Compression;
                if (Compress == Compression.Both || Compress == Compression.Outgoing)
                {
                    writer.WriteLine("Reader.Decompress();");
                }
            }
            properties.GenerateReadCode(writer);
            writer.WriteLine("}");
            writer.WriteLine();
        }
        #endregion

        #region GenerateConstructors

        internal static void GenerateConstructors(Packet packet, TextWriter writer)
        {
            // Default constructor, does nothing
            writer.WriteLine("public {0}() : base({1})", packet.Name, packet.Parent.Key);
            writer.WriteLine("{");
            writer.WriteLine("}");
            writer.WriteLine();
        }

        #endregion
    }
}
