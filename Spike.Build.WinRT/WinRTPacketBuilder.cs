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
using Spike.Build.WinRT;


namespace Spike.Build.Client
{
    public class WinRTPacketBuilder : ISubClientBuilder<Packet, WinRTBuilder>
    {
        public void GenerateCode(Packet packet, WinRTBuilder parent)
        {
            using (var writer = new StringWriter())
            {
                GeneratePacket(packet, writer);
                parent.AddSourceFile(parent.PacketsPath, String.Format(@"{0}.cs", packet.Name), writer);
            }
        }

        internal static void GeneratePacket(Packet packet, TextWriter writer)
        {

            writer.WriteLine("using System;");
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine("using System.Text;");
            writer.WriteLine("using System.IO;");
            writer.WriteLine("using System.Net;");
            writer.WriteLine();
            writer.WriteLine("namespace Spike.Network.Packets");
            writer.WriteLine("{");

            writer.WriteLine("\tpublic class {0}", packet.Name.PascalCase()); // Begin package
            writer.WriteLine("\t{");
            foreach (var member in packet.GetMembers())
            {
                
                writer.WriteLine("\t\tpublic {0} {1} {{ get; set; }}",
                    WinRTBuilderExtensions.SpikeToCSharpType(member.Type),
                    member.Name.PascalCase());
            }
            writer.WriteLine("\t}"); // End class
            writer.WriteLine("}"); // End namespace
            writer.WriteLine();
        }
    }
}
