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

namespace Spike.Build.Client.JavaScript
{
    public class JavaScriptChannelBuilder
    {
        /// <summary>
        /// Generates operation read operations
        /// </summary>
        public static void GenerateCode(JavaScriptBuilder builder)
        {
            using (var writer = new CodeWriter())
            {
                GenerateServerChannel(builder, writer);
                builder.AddSourceFile(builder.SrcOutputPath, "ServerChannel.js", writer);
            }
        }


        #region GenerateServerChannel
        internal static void GenerateServerChannel(JavaScriptBuilder builder, TextWriter writer)
        {
            writer.WriteLine("function ServerChannel(endPoint)"); // Begin class
            writer.WriteLine("{");
            {
                writer.WriteLine("/* Server EndPoint url */");
                writer.WriteLine("this.endPoint = endPoint;");
                writer.WriteLine();

                writer.WriteLine("/* 'Socket' object to use for all communication */");
                writer.WriteLine("this.socket = new ServerSocket(this);");
                writer.WriteLine();

                writer.WriteLine("/* Operation reader */");
                writer.WriteLine("this.operationReader = new OperationReader();");
                writer.WriteLine();

                writer.WriteLine("/* Connects to the server */");
                writer.WriteLine("this.connect = function()"); 
                writer.WriteLine("{");
        		writer.WriteLine("this.socket.connect();");
                writer.WriteLine("};");
                writer.WriteLine();

                writer.WriteLine("/* Sends a packet to the server */");
                writer.WriteLine("this.send = function(operationNumber, packet)");
                writer.WriteLine("{");
                writer.WriteLine("var writer = new PacketWriter();");
                writer.WriteLine("if(packet != null)");
                writer.WriteLine("{");
                writer.WriteLine("writer.writePacket(packet);");
                writer.WriteLine("}");
                writer.WriteLine("this.socket.send(operationNumber, writer);");
                writer.WriteLine("};");

                GenerateEvents(builder, writer);
                GenerateSends(builder, writer);
                GeneratePropertiesAndFields(builder, writer);
                GenerateOnReceive(builder, writer);
            }

            writer.WriteLine("};"); // End class
        }

        internal static void GenerateOnReceive(JavaScriptBuilder builder, TextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("/* This method is automatically generated and allows event dispatching when a packet received */");
            writer.WriteLine("this.onReceive = function(operation, reader)");
            writer.WriteLine("{");
            writer.WriteLine("var packet = this.operationReader.read(operation, reader);");
            writer.WriteLine("switch (operation)");
            writer.WriteLine("{");
            builder.Model.OperationsWithOutgoingPacket
                .ForEach(operation => 
                {
                    writer.WriteLine("case {0}:", operation.Key);

                    // Pull operation
                    if (operation.Direction == Direction.Pull)
                    {
                        writer.WriteLine("if((this.dispatchInformOnlyOnRequest && this._requested{0} > 0) || !this.dispatchInformOnlyOnRequest)", operation.Outgoing.Name);
                        writer.WriteLine("{");
                        writer.WriteLine("this._requested{0} --;", operation.Outgoing.Name);
                        writer.WriteLine("if(this.{0} != null)", operation.GetInformMethodName());
                        writer.WriteLine("{");
                        writer.WriteLine("this.{0}(packet);", operation.GetInformMethodName());
                        writer.WriteLine("}");
                        writer.WriteLine("}");
                        writer.WriteLine("return;");
                    }
                    else if(operation.Direction == Direction.Push) // Push operation
                    {
                        writer.WriteLine("if(this.{0} != null)", operation.GetInformMethodName());
                        writer.WriteLine("{");
                        writer.WriteLine("this.{0}(packet);", operation.GetInformMethodName());
                        writer.WriteLine("}");
                        writer.WriteLine("return;");

                    }
                });

            writer.WriteLine("}");
            writer.WriteLine("}");
            writer.WriteLine();
        }

        internal static void GenerateSends(JavaScriptBuilder builder, TextWriter writer)
        {
            builder.Model.Operations
                .ForEach(operation =>
                {
                    if (operation.Direction == Direction.Pull)
                    {
                        // Only for pull operation
                        writer.WriteLine();
                        writer.WriteLine("/* {0} */", operation.Description);

                        if (operation.Incoming != null)
                        {
                            // Without request-crafting
                            writer.Write("this.{0} = function(", operation.GetRequestMethodName());
                            writer.Write(operation.Incoming.GetMembers()
                                .Select(param => String.Format("{0}", param.InternalName.FirstLetterLower()))
                                .Aggregate((a, b) => String.Format("{0}, {1}", a, b)));
                            writer.WriteLine(")");
                            writer.WriteLine("{");
                            writer.WriteLine("var requestPacket = new {0}();", operation.Incoming.Name);
                            writer.Write(operation.Incoming.GetMembers()
                                .Select(param => String.Format("requestPacket.{0} = {0};{1}", param.InternalName.FirstLetterLower(), Environment.NewLine))
                                .Aggregate((a, b) => a + b));
                            if (operation.Outgoing != null) // Only if there's a reply
                                writer.WriteLine("this._requested{0} ++;", operation.Outgoing.Name);
                            writer.WriteLine("this.send({0}, requestPacket);", operation.Key);
                            writer.WriteLine("};");
                        }
                        else // No parameters
                        {
                            writer.WriteLine("this.{0} = function()", operation.GetRequestMethodName());
                            writer.WriteLine("{");
                            if (operation.Outgoing != null) // Only if there's a reply
                                writer.WriteLine("this._requested{0} ++;", operation.Outgoing.Name);
                            writer.WriteLine("this.send({0}, null);", operation.Key);
                            writer.WriteLine("};");
                            writer.WriteLine();
                        }
                    }
                });


        }

        internal static void GeneratePropertiesAndFields(JavaScriptBuilder builder, TextWriter writer)
        {
            writer.WriteLine("/* Defines if the Inform event will be dispatched only if a request was previously issued (only for pull operations) */");
            writer.WriteLine("this.dispatchInformOnlyOnRequest = true;");
            writer.WriteLine();
            builder.Model.Operations
                .ForEach(operation =>
                {
                    // Only for pull operation
                    if (operation.Direction == Direction.Pull)
                    {
                        if (operation.Outgoing != null)  // Only if there's a reply
                            writer.WriteLine("this._requested{0} = 0;", operation.Outgoing.Name);
                    }
                });
            writer.WriteLine();
        }

        internal static void GenerateEvents(JavaScriptBuilder builder, TextWriter writer)
        {
            builder.Model.Operations
                .ForEach(operation =>
                {
                    if (operation.Outgoing != null)
                    {
                        writer.WriteLine("/* Event: invoked when the {0} inform is received from the server */", operation.Name);
                        writer.WriteLine("this.{0} = null;", operation.GetInformMethodName());
                        writer.WriteLine();
                    }
                });
        }
        #endregion


    }
}
