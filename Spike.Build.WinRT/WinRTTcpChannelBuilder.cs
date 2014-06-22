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
using Spike.Build.WinRT;


namespace Spike.Build.Client
{
    public class WinRTTcpChannelBuilder
    {
        /// <summary>
        /// Generates TcpChannel
        /// </summary>
        public static void GenerateCode(WinRTBuilder builder)
        {
            using (var writer = new StringWriter())
            {
                GenerateServerChannel(builder, writer);
                builder.AddSourceFile(builder.SrcOutputPath, @"Network\TcpChannel.cs", writer);
            }
        }


        #region GenerateServerChannel
        internal static void GenerateServerChannel(WinRTBuilder builder, TextWriter writer)
        {
            writer.WriteLine("using Spike.Network.Packets;");
            writer.WriteLine("using System;");
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine("using System.Diagnostics;");
            writer.WriteLine("using System.Linq;");
            writer.WriteLine("using System.Text;");
            writer.WriteLine("using System.Threading.Tasks;");
            writer.WriteLine();
            writer.WriteLine("namespace Spike.Network");
            writer.WriteLine("{");
            writer.WriteLine("\tclass TcpChannel : TcpChannelBase<TcpChannel>");
            writer.WriteLine("\t{");
            GenerateEvents(builder, writer);
            writer.WriteLine();
            writer.WriteLine();
            GenerateSends(builder, writer);
            writer.WriteLine();
            writer.WriteLine();
            GenerateOnReceive(builder, writer);
            writer.WriteLine("\t}"); // End class
            writer.WriteLine("}"); // End namespace
            writer.WriteLine();
        }

        internal static void GenerateOnReceive(WinRTBuilder builder, TextWriter writer)
        {
            writer.WriteLine("\t\tprotected override void OnReceive(uint key)");
            writer.WriteLine("\t\t{");
            writer.WriteLine("\t\t\tswitch (key)");
            writer.WriteLine("\t\t\t{");


            foreach (var operation in builder.Model.OperationsWithOutgoingPacket)
            {
                writer.WriteLine("\t\t\t\tcase 0x{0}: ", operation.Key.Trim('"'));
                writer.WriteLine("\t\t\t\t\t{");

                writer.WriteLine("\t\t\t\t\t\tvar packet = new {0}();", operation.Outgoing.Name);
                writer.WriteLine("\t\t\t\t\t\tBeginReadPacket({0});", operation.Compression == Compression.Outgoing ? "true" : "false");
                foreach (var member in operation.Outgoing.Member)
                {
                    writer.WriteLine("\t\t\t\t\t\tpacket.{0} = PacketRead{1}();",
                        member.Name.PascalCase(),
                        member.Class);
                }

                /*if (NgChatMessagesInform != null)
                            NgChatMessagesInform(this, packet);*/

                writer.WriteLine("\t\t\t\t\t\tif ({0} != null)", operation.Outgoing.Name.PascalCase());
                writer.WriteLine("\t\t\t\t\t\t\t{0}(this, packet);", operation.Outgoing.Name.PascalCase());
                writer.WriteLine("\t\t\t\t\t\treturn;");
                writer.WriteLine("\t\t\t\t\t}");
            }
            writer.WriteLine("\t\t\t\tdefault:");
            writer.WriteLine("\t\t\t\t\tDebug.WriteLine(\"Unknow packet : {0:X}\", key);");
            writer.WriteLine("\t\t\t\t\treturn;");
            writer.WriteLine("\t\t\t}");
            writer.WriteLine("\t\t}");
        }

        internal static void GenerateSends(WinRTBuilder builder, TextWriter writer)
        {
            writer.WriteLine("\t\t//Sends");
            foreach (var operation in builder.Model.Operations)
            {
                writer.WriteLine("\t\t/*");
                writer.WriteLine("\t\t * {0}", operation.Description);
                writer.WriteLine("\t\t */");

                if (operation.Incoming == null)
                {



                    writer.WriteLine("\t\tpublic async void {0}()", operation.Name.PascalCase());
                    writer.WriteLine("\t\t{");

                    writer.WriteLine("\t\t\tBeginNewPacket(0x{0}); //Key", operation.Key.Trim('"'));
                    writer.WriteLine("\t\t\tawait SendPacket({0});", operation.Compression == Compression.Incoming ? "true" : "false");
                    writer.WriteLine("\t\t}");
                    

                }
                else
                {
                    
                    List<Element> members = operation.Incoming.GetMembers();

                    writer.Write("\t\tpublic async void {0}(", operation.Name.PascalCase());

                    writer.Write(operation.Incoming.GetMembers()
                        .Select(member => String.Format("{0} {1}",
                            WinRTBuilderExtensions.SpikeToCSharpType(member.Type), member.Name.CamelCase()))
                        .Aggregate((a, b) => String.Format("{0}, {1}", a, b)));

                    writer.WriteLine(")");
                    writer.WriteLine("\t\t{");
                    writer.WriteLine("\t\t\tBeginNewPacket(0x{0}); //Key", operation.Key.Trim('"'));
                    
                    foreach (var member in members)
                    {
                        writer.WriteLine("\t\t\tPacketWrite({0});",
                            member.Name.CamelCase());
                    }

                    writer.WriteLine("\t\t\tawait SendPacket({0});", operation.Compression == Compression.Incoming ? "true" : "false");
                    writer.WriteLine("\t\t}");
                }
                

            }
        }

        internal static void GenerateEvents(WinRTBuilder builder, TextWriter writer)
        {
            //

            writer.WriteLine("\t\t//EventHandlers");
            foreach (var operation in builder.Model.OperationsWithOutgoingPacket)
            {
                writer.WriteLine("\t\tpublic event Action<TcpChannel, {0}> {0};",
                    operation.Outgoing.Name.PascalCase());                
            }

        }
        #endregion

    }
}
