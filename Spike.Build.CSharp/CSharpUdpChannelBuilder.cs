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
using Spike.Build.Client.CSharp;

namespace Spike.Build.Client
{
    public class CSharpUdpChannelBuilder
    {
        /// <summary>
        /// Generates operation read operations
        /// </summary>
        public static void GenerateCode(CSharpBuilder builder)
        {
            using (var writer = new CodeWriter())
            {
                GenerateServerChannel(builder, writer);
                builder.AddSourceFile(builder.SrcOutputPath, @"UdpChannel.cs", writer);
            }
        }


        #region GenerateServerChannel
        internal static void GenerateServerChannel(CSharpBuilder builder, TextWriter writer)
        {
            CSharpBuilder.GenerateHeader(writer);
            writer.WriteLine("namespace Spike.Network"); // Begin package
            writer.WriteLine("{");

            writer.WriteLine();

            writer.WriteLine("public class UdpChannel : UdpChannelBase"); // Begin class
            writer.WriteLine("{");
            {
                GenerateConstructors(builder, writer);
                GeneratePropertiesAndFields(builder, writer);
                GenerateEvents(builder, writer);
                GenerateSends(builder, writer);
                GenerateOnReceive(builder, writer);
                GenerateSystemMethods(builder, writer);
            }

            writer.WriteLine("}"); // End class
            writer.WriteLine("}"); // End package 


        }

        internal static void GenerateConstructors(CSharpBuilder builder, TextWriter writer)
        {
            writer.WriteLine("public UdpChannel() : base()"); 
            writer.WriteLine("{");
            writer.WriteLine("if(OperationReaderBase.Instance == null)");
            writer.WriteLine("   OperationReaderBase.SetInstance(new OperationReader());");
            writer.WriteLine("}");
            writer.WriteLine();
        }

        internal static void GenerateOnReceive(CSharpBuilder builder, TextWriter writer)
        {
            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// This method is automatically generated and allows event dispatching when a packet received");
            writer.WriteLine("/// </summary>");
            writer.WriteLine("internal override void OnReceive(ChannelReceiveEventArgs e)");
            writer.WriteLine("{");
            writer.WriteLine("switch (e.Operation)");
            writer.WriteLine("{");
            builder.Model.OperationsWithOutgoingPacket
                .ForEach(operation => 
                {
                    writer.WriteLine("case {0}:", operation.Key);

                    // Pull operation
                    if (operation.Direction == Direction.Pull)
                    {
                        writer.WriteLine("if((fDispatchInformOnlyOnRequest && fRequested{0} > 0) || !fDispatchInformOnlyOnRequest)", operation.Outgoing.Name);
                        writer.WriteLine("{");
                        writer.WriteLine("// Mask the request as terminated and dispatch the event");
                        writer.WriteLine("fRequested{0} --;", operation.Outgoing.Name);
                        writer.WriteLine("On{0}(e.Packet as {1});", operation.GetInformMethodName(), operation.Outgoing.Name);
                        writer.WriteLine("}");
                        writer.WriteLine("return;");
                    }
                    else if(operation.Direction == Direction.Push) // Push operation
                    {
                        writer.WriteLine("On{0}(e.Packet as {1});", operation.GetInformMethodName(), operation.Outgoing.Name);
                        writer.WriteLine("return;");

                    }
                });

            writer.WriteLine("}");
            writer.WriteLine("}");
        }

        internal static void GenerateSends(CSharpBuilder builder, TextWriter writer)
        {
            builder.Model.Operations
                .ForEach(operation =>
                {
                    if (operation.Direction == Direction.Pull)
                    {
                        // Only for pull operation
                        writer.WriteLine();
                        writer.WriteLine("/// <summary>");
                        writer.WriteLine("/// {0}", operation.Description);
                        writer.WriteLine("/// </summary>");

                        if (operation.Incoming != null)
                        {
                            // Without request-crafting
                            writer.Write("public void {0}(", operation.GetRequestMethodName());
                            writer.Write(operation.Incoming.GetMembers()
                                .Select(param => String.Format("{0} {1}", param.InternalType, param.InternalName))
                                .Aggregate((a, b) => String.Format("{0}, {1}", a, b)));
                            writer.WriteLine(")");
                            writer.WriteLine("{");
                            writer.WriteLine("{0} requestPacket = new {0}();", operation.Incoming.Name);
                            writer.Write(operation.Incoming.GetMembers()
                                .Select(param => String.Format("requestPacket.{0} = {0};{1}", param.InternalName, Environment.NewLine))
                                .Aggregate((a, b) => a + b));
                            if (operation.Outgoing != null) // Only if there's a reply
                                writer.WriteLine("fRequested{0} ++;", operation.Outgoing.Name);
                            writer.WriteLine("base.Send({0}, requestPacket);", operation.Key);
                            writer.WriteLine("}");
                        }
                        else // No parameters
                        {
                            writer.WriteLine("public void {0}()", operation.GetRequestMethodName());
                            writer.WriteLine("{");
                            if (operation.Outgoing != null) // Only if there's a reply
                                writer.WriteLine("fRequested{0} ++;", operation.Outgoing.Name);
                            writer.WriteLine("base.Send({0}, null);", operation.Key);
                            writer.WriteLine("}");
                            writer.WriteLine();
                        }
                    }
                    else if (operation.Direction == Direction.Push)
                    {
                        // Push operation
                        var paramsNumber = operation.Incoming == null ? 0 : operation.Incoming.GetMembers().Count;

                        writer.WriteLine();
                        writer.WriteLine("/// <summary>");
                        writer.WriteLine("/// Sends a subscribe/unsubscribe request for the operation ({0}).", operation.Description);
                        writer.WriteLine("/// </summary>");
                        writer.WriteLine("public void SubscribeTo{0}({1})",
                            operation.Name,
                            paramsNumber > 0 ? String.Format("{0} request", operation.Incoming.Name) : "");
                        writer.WriteLine("{");
                        writer.WriteLine("base.Send({0}, {1});", operation.Key, paramsNumber > 0 ? "request" : "null");
                        writer.WriteLine("}");
                        writer.WriteLine();

                        writer.WriteLine();
                        writer.WriteLine("/// <summary>");
                        writer.WriteLine("/// Sends a subscribe/unsubscribe request for the operation ({0})", operation.Description);
                        writer.WriteLine("/// </summary>");
                        writer.WriteLine("public void UnsubscribeFrom{0}({1})",
                            operation.Name,
                            paramsNumber > 0 ? String.Format("{0} request", operation.Incoming.Name) : "");
                        writer.WriteLine("{");
                        writer.WriteLine("base.Send({0}, {1});", operation.Key, paramsNumber > 0 ? "request" : "null");
                        writer.WriteLine("}");
                        writer.WriteLine();
                    }
                });


        }

        internal static void GeneratePropertiesAndFields(CSharpBuilder builder, TextWriter writer)
        {
            writer.WriteLine("private bool fDispatchInformOnlyOnRequest = true;");
            writer.WriteLine();
            writer.WriteLine("/// <summary>");
            writer.WriteLine("// By default will dispatch the Inform only for a request");
            writer.WriteLine("// was done (flag applies for pull operations only)");
            writer.WriteLine("/// </summary>");
            writer.WriteLine("public bool DispatchInformOnlyOnRequest");
            writer.WriteLine("{");
            writer.WriteLine("get{ return fDispatchInformOnlyOnRequest;}");
            writer.WriteLine("set{ fDispatchInformOnlyOnRequest = value;}");
            writer.WriteLine("}");
            writer.WriteLine();
            builder.Model.Operations
                .ForEach(operation =>
                {
                    // Only for pull operation
                    if (operation.Direction == Direction.Pull)
                    {
                        if (operation.Outgoing != null)  // Only if there's a reply
                            writer.WriteLine("private int fRequested{0} = 0;", operation.Outgoing.Name);
                    }
                });
            writer.WriteLine();
        }

        internal static void GenerateEvents(CSharpBuilder builder, TextWriter writer)
        {
            builder.Model.Operations
                .ForEach(operation =>
                {
                    if (operation.Outgoing != null)
                    {
                        var packetType = operation.Outgoing.Name;
                        var eventName = operation.GetInformMethodName();
                        
                        writer.WriteLine("public event EventHandler<PacketReceiveEventArgs<{0}>> {1};", packetType, eventName);
                        writer.WriteLine("private void On{0}({1} packet)", eventName, packetType);
                        writer.WriteLine("{");
                        writer.WriteLine("if ({0} != null)", eventName);
                        writer.WriteLine("{0}(this, new PacketReceiveEventArgs<{1}>(packet));", eventName, packetType);
                        writer.WriteLine("}");
                    }
                });
        }

        internal static void GenerateSystemMethods(CSharpBuilder builder, TextWriter writer)
        {

            writer.WriteLine();
            
            writer.WriteLine("private const int PingRequests = 11;");
            writer.WriteLine("private int fPingTest = 0;");
            writer.WriteLine("private DateTime[] fPingStarts = new DateTime[PingRequests];");
            writer.WriteLine("private uint[] fPingResults = new uint[PingRequests];");
            writer.WriteLine("public event EventHandler<LatencyComputedEventArgs> LatencyComputed;");
            writer.WriteLine();
            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// Computes the latency (Ping) by sending 10 request to the server and averaging the results");
            writer.WriteLine("/// </summary>");
            writer.WriteLine("public void ComputeLatency()");
            writer.WriteLine("{");
            writer.WriteLine("fPingTest = 0;");
            writer.WriteLine("fPingStarts[0] = DateTime.Now;");
            writer.WriteLine("PingInform += PongReceived;");
            writer.WriteLine("Ping();");
            writer.WriteLine("}");
            writer.WriteLine();
            writer.WriteLine("private void PongReceived(object sender, PacketReceiveEventArgs<PingInform> e)");
            writer.WriteLine("{");
            writer.WriteLine("fPingResults[fPingTest] = (uint)((DateTime.Now - fPingStarts[fPingTest]).TotalMilliseconds);");
            writer.WriteLine("fPingTest++;");
            writer.WriteLine("if (fPingTest < PingRequests)");
            writer.WriteLine("{");
            writer.WriteLine("// Send more");
            writer.WriteLine("fPingStarts[fPingTest] = DateTime.Now;");
            writer.WriteLine("Ping();");
            writer.WriteLine("}");
            writer.WriteLine("else");
            writer.WriteLine("{");
            writer.WriteLine("// Compute the average latency");
            writer.WriteLine("double median = 0;");
            writer.WriteLine("for (int i = 1; i < PingRequests; i++)");
            writer.WriteLine("median += (double)fPingResults[i] / PingRequests;");
            writer.WriteLine();
            writer.WriteLine("if(LatencyComputed != null)");
            writer.WriteLine("LatencyComputed(null, new LatencyComputedEventArgs(Convert.ToUInt32(median)));");
            writer.WriteLine("}");
            writer.WriteLine("}");


            


        }

        #endregion


    }
}
