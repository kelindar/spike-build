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
using Spike.Build.Java;


namespace Spike.Build.Client
{
    public class JavaTcpChannelBuilder
    {
        /// <summary>
        /// Generates TcpChannel
        /// </summary>
        public static void GenerateCode(JavaBuilder builder)
        {
            using (var writer = new StringWriter())
            {
                GenerateServerChannel(builder, writer);
                builder.AddSourceFile(builder.SrcOutputPath, @"com\misakai\spike\network\TcpChannel.java", writer);
            }
        }


        #region GenerateServerChannel
        internal static void GenerateServerChannel(JavaBuilder builder, TextWriter writer)
        {
            writer.WriteLine(@"package com.misakai.spike.network;"); // Begin package
            writer.WriteLine(); 
            writer.WriteLine(@"import java.util.ArrayList;");
            writer.WriteLine(@"import com.misakai.spike.network.packets.*;");
            writer.WriteLine();
            writer.WriteLine(@"public final class TcpChannel extends AbstractTcpChannel {"); // Begin class
            GenerateEvents(builder, writer);
            writer.WriteLine();
            writer.WriteLine();
            GenerateSends(builder, writer);
            writer.WriteLine();
            writer.WriteLine();
            GenerateOnReceive(builder, writer);
            writer.WriteLine(@"}"); // End class
            writer.WriteLine();
            


        }

        internal static void GenerateOnReceive(JavaBuilder builder, TextWriter writer)
        {
            writer.WriteLine("\t@Override");

            writer.WriteLine("\tprotected void onReceive(int key){");
            writer.WriteLine("\t\tswitch(key){");
            
            foreach (var operation in builder.Model.OperationsWithOutgoingPacket) {
                writer.WriteLine("\t\t\tcase 0x{0}: {{", operation.Key.Trim('"'));
                
                writer.WriteLine("\t\t\t\t//Create data structure");
                writer.WriteLine("\t\t\t\t{0} packet = new {0}();", operation.Outgoing.Name);
                writer.WriteLine("\t\t\t\tbeginReadPacket({0});", operation.Compression == Compression.Outgoing ? "true" : "false");
                foreach (var member in operation.Outgoing.Member) {
                    writer.WriteLine("\t\t\t\tpacket.{0} = packetRead{1}();",
                        member.Name.CamelCase(), 
                        member.Class);
                }

                writer.WriteLine("\t\t\t\tfor (PacketHandler<{0}> handler : {1})", operation.Outgoing.Name, operation.Outgoing.Name.CamelCase());
                writer.WriteLine("\t\t\t\t\thandler.onReceive(packet);");
                writer.WriteLine("\t\t\t\treturn;");
                writer.WriteLine("\t\t\t\t}");
            }
            writer.WriteLine("\t\t\tdefault:");
            writer.WriteLine("\t\t\t\tSystem.out.println(\"Unknow packet : \"+ key);");
            writer.WriteLine("\t\t\t\treturn;");
            writer.WriteLine("\t\t}");
            writer.WriteLine("\t}");
        }

        internal static void GenerateSends(JavaBuilder builder, TextWriter writer)
        {
            writer.WriteLine("\t//Sends");
            foreach (var operation in builder.Model.Operations) {
                if (operation.Incoming == null) {
                    writer.WriteLine("\t/**");
                    writer.WriteLine("\t * {0}", operation.Description);
                    writer.WriteLine("\t */");
                    
                    writer.WriteLine("\tpublic void {0}(){{", operation.Name.CamelCase());
                    writer.WriteLine("\t\tbeginNewPacket(0x{0}); //Key", operation.Key.Trim('"'));
                    writer.WriteLine("\t\tsendPacket({0});", operation.Compression == Compression.Incoming ? "true" : "false"); 
                    writer.WriteLine("\t}");
                } else {
                    List<Element> members = operation.Incoming.GetMembers();

                    writer.Write("\tpublic void {0}(", operation.Name.CamelCase());
                    writer.Write(operation.Incoming.GetMembers()
                        .Select(member => String.Format("{0} {1}",
                            JavaBuilderExtensions.SpikeToJavaType(member.Type), member.Name.CamelCase()))
                        .Aggregate((a, b) => String.Format("{0}, {1}", a, b)));

                    writer.WriteLine("){");
                    writer.WriteLine("\t\tbeginNewPacket(0x{0}); //Key", operation.Key.Trim('"'));

                    foreach (var member in members) {
                        writer.WriteLine("\t\tpacketWrite({0});",
                            member.Name.CamelCase());
                    }

                    writer.WriteLine("\t\tsendPacket({0});", operation.Compression == Compression.Incoming ? "true" : "false");
                    writer.WriteLine("\t}");
                }                
            }
        }        

        internal static void GenerateEvents(JavaBuilder builder, TextWriter writer) 
        {
            writer.WriteLine("\t//EventHandlers");
            foreach (var operation in builder.Model.OperationsWithOutgoingPacket) {
                writer.WriteLine("\tpublic final ArrayList<PacketHandler<{0}>> {1} = new ArrayList<PacketHandler<{0}>>();",
                    operation.Outgoing.Name,
                    operation.Outgoing.Name.CamelCase());
            }

        }
        #endregion      

    }
}
