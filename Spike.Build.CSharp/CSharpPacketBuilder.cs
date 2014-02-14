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
