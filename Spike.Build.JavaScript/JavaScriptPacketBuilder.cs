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
