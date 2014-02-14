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
using Spike.Build.Client.AS3;

namespace Spike.Build.Client
{
    public class AS3PacketBuilder : ISubClientBuilder<Packet, AS3Builder>
    {
        public void GenerateCode(Packet packet, AS3Builder parent)
        {
            using (var writer = new CodeWriter())
            {

                GeneratePacket(packet, writer);
                parent.AddSourceFile(parent.PacketsPath, String.Format(@"{0}.as", packet.Name), writer);
            }
        }

        #region GeneratePacket

        internal static void GeneratePacket(Packet packet, TextWriter writer)
        {
            writer.WriteLine("package network.packets"); // Begin package
            writer.WriteLine("{");
            AS3Builder.GenerateHeader(writer);
            writer.WriteLine("[RemoteClass(alias=\"{0}\")]", packet.Name);
            writer.WriteLine("public class {0} extends Packet implements IPacket", packet.Name); // Begin class
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
            writer.WriteLine("public function write(writer:PacketWriter):void");
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
            writer.WriteLine("}");
            writer.WriteLine();
        }

        internal static void GeneratePacketReadMethod(Packet packet, TextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("public function read(reader:PacketReader):void");
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
            writer.WriteLine("}");
            writer.WriteLine();
        }
        #endregion

        #region GenerateConstructors

        internal static void GenerateConstructors(Packet packet, TextWriter writer)
        {
            // Default constructor, does nothing
            writer.WriteLine("public function {0}()", packet.Name);
            writer.WriteLine("{");
            writer.WriteLine("super({0});", packet.Parent.Key);
            writer.WriteLine("}");
            writer.WriteLine();
        }

        #endregion
    }
}
