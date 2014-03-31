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
using Spike.Build.Java;


namespace Spike.Build.Client
{
    public class JavaPacketBuilder : ISubClientBuilder<Packet, JavaBuilder>
    {
        public void GenerateCode(Packet packet, JavaBuilder parent)
        {
            using (var writer = new StringWriter())
            {
                GeneratePacket(packet, writer);
                parent.AddSourceFile(parent.PacketsPath, String.Format(@"{0}.java", packet.Name), writer);
            }
        }

        internal static void GeneratePacket(Packet packet, TextWriter writer)
        {
            writer.WriteLine(@"package com.misakai.spike.network.packets;");

            //import java.util.Date only if Packet containt a date member 
            if (packet.GetMembers().Any(member => member.Type == ElementType.DateTime))
            {
                writer.WriteLine();
                writer.WriteLine(@"import java.util.Date;");
            }
            writer.WriteLine();
            writer.WriteLine(@"public final class {0} {{", packet.Name); // Begin package
            foreach (var member in packet.GetMembers())
            {
                writer.WriteLine("\tpublic {0} {1};",
                    JavaBuilderExtensions.SpikeToJavaType(member.Type),
                    member.Name.CamelCase());
            }
            writer.WriteLine(@"}"); // End class
            writer.WriteLine();
        }
    }
}
