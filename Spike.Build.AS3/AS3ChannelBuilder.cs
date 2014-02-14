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
using Spike.Build.Client.AS3;

namespace Spike.Build.Client
{
    public class AS3ChannelBuilder
    {
        /// <summary>
        /// Generates operation read operations
        /// </summary>
        public static void GenerateCode(AS3Builder builder)
        {
            using (var writer = new CodeWriter())
            {
                GenerateServerChannel(builder, writer);
                builder.AddSourceFile(builder.HandlerPath, "ServerChannel.as", writer);
            }
        }


        #region GenerateServerChannel
        internal static void GenerateServerChannel(AS3Builder builder, TextWriter writer)
        {
            writer.WriteLine("package network"); // Begin package
            writer.WriteLine("{");

            writer.WriteLine("import flash.events.*;");
            writer.WriteLine("import mx.events.*;");
            writer.WriteLine("import mx.rpc.events.ResultEvent;");
            writer.WriteLine("import flash.utils.ByteArray;");
            writer.WriteLine("import network.events.*;");
            writer.WriteLine("import network.packets.*;");
            writer.WriteLine("import network.*;");

            writer.WriteLine();

            GenerateEventAttributes(builder, writer);
            writer.WriteLine("public class ServerChannel extends AbstractServerChannel"); // Begin class
            writer.WriteLine("{");
            {
                GenerateSends(builder, writer);
                GeneratePropertiesAndFields(builder, writer);
                GenerateOnReceive(builder, writer);
            }

            writer.WriteLine("}"); // End class
            writer.WriteLine("}"); // End package 


        }

        internal static void GenerateOnReceive(AS3Builder builder, TextWriter writer)
        {
            writer.WriteLine("/**");
            writer.WriteLine(" * This method is automatically generated and allows event dispatching when a packet received");
            writer.WriteLine(" */");
            writer.WriteLine("protected override function onReceive(event:SocketReceiveEvent):void");
            writer.WriteLine("{");
            writer.WriteLine("switch (event.Operation)");
            writer.WriteLine("{");
            //writer.WriteLine("			case {0}:");
            builder.Model.OperationsWithOutgoingPacket
                .ForEach(operation => 
                {
                    writer.WriteLine("case {0}:", operation.Key);

                    // Pull operation
                    if (operation.Direction == Direction.Pull)
                    {
                        writer.WriteLine("if((dispatchInformOnlyOnRequest && _requested{0} > 0) || !dispatchInformOnlyOnRequest)", operation.Outgoing.Name);
                        writer.WriteLine("{");
                        writer.WriteLine("// Mask the request as terminated and dispatch the event");
                        writer.WriteLine("_requested{0} --;", operation.Outgoing.Name);
                        writer.WriteLine("dispatchEvent(new PacketReceiveEvent(\"{0}\", event.Packet));", operation.GetInformMethodName());
                        writer.WriteLine("}");
                        writer.WriteLine("return;");
                    }
                    else if(operation.Direction == Direction.Push) // Push operation
                    {
                        writer.WriteLine("dispatchEvent(new PacketReceiveEvent(\"{0}\", event.Packet));", operation.GetInformMethodName());
                        writer.WriteLine("return;");

                    }
                });

            writer.WriteLine("}");
            writer.WriteLine("}");
        }

        internal static void GenerateSends(AS3Builder builder, TextWriter writer)
        {
            builder.Model.Operations
                .ForEach(operation =>
                {
                    if (operation.Direction == Direction.Pull)
                    {
                        // Only for pull operation
                        writer.WriteLine();
                        writer.WriteLine("/**");
                        writer.WriteLine(" * {0}", operation.Description);
                        writer.WriteLine(" */");

                        if (operation.Incoming != null)
                        {
                            // Without request-crafting
                            writer.Write("public function {0}(", operation.GetRequestMethodName());
                            writer.Write(operation.Incoming.GetMembers()
                                .Select(param => String.Format("{0}:{1}", param.InternalName.FirstLetterLower(), param.InternalType))
                                .Aggregate((a, b) => String.Format("{0}, {1}", a, b)));
                            writer.WriteLine("):void");
                            writer.WriteLine("{");
                            writer.WriteLine("var requestPacket:{0} = new {0}();", operation.Incoming.Name);
                            writer.Write(operation.Incoming.GetMembers()
                                .Select(param => String.Format("requestPacket.{0} = {0};{1}", param.InternalName.FirstLetterLower(), Environment.NewLine))
                                .Aggregate((a, b) => a + b));
                            if (operation.Outgoing != null) // Only if there's a reply
                                writer.WriteLine("_requested{0} ++;", operation.Outgoing.Name);
                            writer.WriteLine("super.send({0}, requestPacket);", operation.Key);
                            writer.WriteLine("}");
                        }
                        else // No parameters
                        {
                            writer.WriteLine("public function {0}():void", operation.GetRequestMethodName());
                            writer.WriteLine("{");
                            if (operation.Outgoing != null) // Only if there's a reply
                                writer.WriteLine("_requested{0} ++;", operation.Outgoing.Name);
                            writer.WriteLine("super.send({0}, null);", operation.Key);
                            writer.WriteLine("}");
                            writer.WriteLine();
                        }
                    }
                    else if (operation.Direction == Direction.Push)
                    {
                        // Push operation
                        //var paramsNumber = operation.Incoming == null ? 0 : operation.Incoming.GetMembers().Count;

                        //writer.WriteLine();
                        //writer.WriteLine("/**");
                        //writer.WriteLine(" * Sends a subscribe/unsubscribe request for the operation ({0}).", operation.Description);
                        //writer.WriteLine(" */");
                        //writer.WriteLine("public function subscribeTo{0}({1}):void",
                        //    operation.Name,
                        //    paramsNumber > 0 ? String.Format("request:{0}", operation.Incoming.Name) : "");
                        //writer.WriteLine("{");
                        //writer.WriteLine("super.send({0}, {1});", operation.Id, paramsNumber > 0 ? "request" : "null");
                        //writer.WriteLine("}");
                        //writer.WriteLine();

                        //writer.WriteLine();
                        //writer.WriteLine("/**");
                        //writer.WriteLine(" * Sends a subscribe/unsubscribe request for the operation ({0})", operation.Description);
                        //writer.WriteLine(" */");
                        //writer.WriteLine("public function unsubscribeFrom{0}({1}):void",
                        //    operation.Name,
                        //    paramsNumber > 0 ? String.Format("request:{0}", operation.Incoming.Name) : "");
                        //writer.WriteLine("{");
                        //writer.WriteLine("super.send({0}, {1});", operation.Id, paramsNumber > 0 ? "request" : "null");
                        //writer.WriteLine("}");
                        //writer.WriteLine();
                    }
                });


        }

        internal static void GeneratePropertiesAndFields(AS3Builder builder, TextWriter writer)
        {
            writer.WriteLine("// by default will dispatch the Inform only for a request");
            writer.WriteLine("// was done (flag applies for pull operations only)");
            writer.WriteLine("private var _dispatchInformOnlyOnRequest:Boolean = true;");
            writer.WriteLine();
            writer.WriteLine("[Bindable]");
            writer.WriteLine("public function set dispatchInformOnlyOnRequest(value:Boolean) :void { _dispatchInformOnlyOnRequest = value; }");
            writer.WriteLine("public function get dispatchInformOnlyOnRequest() : Boolean { return _dispatchInformOnlyOnRequest; }");
            writer.WriteLine();
            builder.Model.Operations
                .ForEach(operation =>
                {
                    // Only for pull operation
                    if (operation.Direction == Direction.Pull)
                    {
                        if (operation.Outgoing != null)  // Only if there's a reply
                            writer.WriteLine("private var _requested{0}:int = 0;", operation.Outgoing.Name);
                    }
                });
            writer.WriteLine();
        }

        internal static void GenerateEventAttributes(AS3Builder builder, TextWriter writer)
        {
            builder.Model.Operations
                .ForEach(operation =>
                {
                    if (operation.Outgoing != null)
                        writer.WriteLine("[Event(name=\"{0}\", type=\"network.events.PacketReceiveEvent\")]", operation.GetInformMethodName());
                });
        }
        #endregion


    }
}
