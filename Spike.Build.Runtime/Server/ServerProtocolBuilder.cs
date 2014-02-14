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

namespace Spike.Build.Server
{
    public class ServerProtocolBuilder : ISubServerBuilder<Protocol, ServerBuilder>
    {
        /// <summary>
        /// Generates Connection extension methods and protocol registration
        /// </summary>
        public void GenerateCode(Protocol protocol, ServerBuilder builder, TextWriter writer)
        {
            writer.WriteLine("#region Class: {0}", protocol.Name);
            writer.WriteLine("///<summary>");
            writer.WriteLine("/// Represents a static class that performs all the necessary initialization routines for {0}.", protocol.Name);
            writer.WriteLine("///</summary>");
            writer.WriteLine(String.Format("public static class {0}", protocol.Name)); // Begin class
            writer.WriteLine("{");
            {
                GenerateConfigureMethod(writer, protocol);
                GenerateInitializeMethod(writer, protocol);
            }
            writer.WriteLine("}"); // End class
            writer.WriteLine("#endregion");

            writer.WriteLine();
            writer.WriteLine("#region Class: {0}Extensions", protocol.Name);
            writer.WriteLine("///<summary>");
            writer.WriteLine("/// Represents a static class that contains IClient extension methods for {0}.", protocol.Name);
            writer.WriteLine("///</summary>");
            writer.WriteLine(String.Format("public static class {0}Extensions", protocol.Name)); // Begin class
            writer.WriteLine("{");
            GenerateClientExtensions(writer, protocol);
            writer.WriteLine("}");  // End class
            writer.WriteLine("#endregion");

            writer.WriteLine();
            writer.WriteLine("#region Class: {0}HubBase", protocol.Name);
            writer.WriteLine("///<summary>");
            writer.WriteLine("/// Represents an optional, abstract hub class for {0}.", protocol.Name);
            writer.WriteLine("///</summary>");
            writer.WriteLine(String.Format("public abstract class {0}HubBase : Hub", protocol.Name)); // Begin class
            writer.WriteLine("{");
            GenerateHubBase(writer, protocol);
            writer.WriteLine("}");  // End class
            writer.WriteLine("#endregion");

           
            GenerateDelegates(writer, protocol);
        }

        #region Generate Congigure

        public static void GenerateConfigureMethod(TextWriter writer, Protocol protocol)
        {
            writer.WriteLine("///<summary>");
            writer.WriteLine("/// Performs the necessary configuration. This method is automatically invoked by Spike-Engine runtime.");
            writer.WriteLine("///</summary>");
            writer.WriteLine("[InvokeAt(InvokeAtType.Configure)]");
            writer.WriteLine("public static void Configure()");
            writer.WriteLine("{");
            foreach (var operation in protocol.Operation)
            {
                writer.WriteLine();
                writer.WriteLine("// Signature: " + operation.Signature);
                writer.WriteLine("// Signature Digest: " + operation.Key);

                // Flags
                bool ContainsRequest = operation.Incoming != null;
                bool ContainsInform  = operation.Outgoing != null;

                // Register metadata
                writer.WriteLine("PacketIndex.RegisterMetadata(new OperationInfo({0}, {1}, {2}, {3}, {4}{5}{6}));",
                    operation.Key,
                    operation.SuppressSecurity.ToString().ToLower(),
                    String.IsNullOrEmpty(operation.Role) ? "null" : String.Format("\"{0}\"", operation.Role),
                    String.Format("CompressionTarget.{0}", operation.Compression.ToString()),
                    String.Format("Direction.{0}", operation.Direction.ToString()),

                    // Optional
                    ContainsRequest ? String.Format(", typeof({0}), {0}.CreateInstance", operation.Incoming.Name) : ", null, null",
                    ContainsInform  ? String.Format(", typeof({0}), {0}.CreateInstance", operation.Outgoing.Name) : ", null, null"
                    );

            }

            writer.WriteLine();
            writer.WriteLine("ProtocolInfo.Register(\"{0}\", \"{1}\");", protocol.Name, protocol.GetEncodedSpml());
            writer.WriteLine("}");
        }
        #endregion

        #region Generate Initialize, Delegates & Events

        public static void GenerateInitializeMethod(TextWriter writer, Protocol protocol)
        {
            bool NeedToGenerate = !protocol.ServerHandlingSpecified || protocol.ServerHandling != ServerHandlingType.Manual;
            if (NeedToGenerate)
            {
                // Generate Initialize()
                writer.WriteLine();
                writer.WriteLine("///<summary>");
                writer.WriteLine("/// Performs the necessary initaliazation. This method is automatically invoked by Spike-Engine runtime.");
                writer.WriteLine("///</summary>");
                writer.WriteLine("[InvokeAt(InvokeAtType.Initialize)]");
                writer.WriteLine("public static void Initialize()");
                writer.WriteLine("{");
                foreach (var operation in protocol.Operation)
                {
                    if (operation.Direction == Direction.Push)
                        continue;
            
                    writer.WriteLine();
                    writer.WriteLine("// " + operation.Signature);
                    writer.WriteLine("PacketHandlers.Register({0}, Internal{1});",
                        operation.Key,
                        operation.Name);

                }


                writer.WriteLine("}");

                // Generate a static handler per operation
                foreach (var operation in protocol.Operation)
                {
                    if (operation.Direction == Direction.Push)
                        continue;

                    if (protocol.ServerHandling == ServerHandlingType.Delegates)
                    {
                        writer.WriteLine("///<summary>");
                        writer.WriteLine("/// Delegate that is invoked when a request for {0} operation comes in.", operation.Name);
                        writer.WriteLine("///</summary>");
                        writer.WriteLine("public static {0}Delegate {1};", operation.Name, operation.Name);
                    }
                    else if (protocol.ServerHandling == ServerHandlingType.Events)
                    {
                        writer.WriteLine("///<summary>");
                        writer.WriteLine("/// Event that is invoked when a request for {0} operation comes in.", operation.Name);
                        writer.WriteLine("///</summary>");
                        writer.WriteLine("public static event RequestHandler{0} {1};", operation.Incoming != null 
                            ? String.Format("<{0}>", operation.Incoming.Name) 
                            : "", operation.Name);
                    }

                    writer.WriteLine();
                    writer.WriteLine("private static void Internal{0}(IClient client, Packet requestPacket)", operation.Name);
                    writer.WriteLine("{");
                    if (operation.Incoming != null)
                    {
                        writer.WriteLine("{0} request = requestPacket as {0};", operation.Incoming.Name);
                    }



                    // if no incoming, it will just be empty ()
                    if (protocol.ServerHandling == ServerHandlingType.Delegates)
                    {
                        writer.WriteLine("{0}{1}({2}){3};",
                            operation.Outgoing == null ? "" : String.Format("client.Send{0}Inform(", operation.Name),
                            operation.Name, 
                            operation.Incoming != null ? operation
                                .GetIncomingFunctionNames()
                                .Select(name => "request." + name)
                                .Aggregate((a, b) => String.Format("{0}, {1}", a, b)) : "",
                            operation.Outgoing == null ? "" : ")");
                    }
                    else if (protocol.ServerHandling == ServerHandlingType.Events)
                    {
                        writer.WriteLine("if({0} != null)", operation.Name);
                        writer.WriteLine("   {0}(client{1});", operation.Name, operation.Incoming == null ? "" : ", request"); 
                    }
                    
                    
                    writer.WriteLine("}");
                }
            }
        }


        public static void GenerateDelegates(TextWriter writer, Protocol protocol)
        {
            foreach (var operation in protocol.Operation)
            {
                if (operation.Direction == Direction.Push)
                    continue;

                var outParams = operation.GetOutgoingFunctionType();
                var inParams  = operation.Incoming != null ?
                                operation
                                    .GetIncomingFunctionTypesAndNames()
                                    .Aggregate((a, b) => String.Format("{0}, {1}", a, b)) : "";

                if (protocol.ServerHandling == ServerHandlingType.Delegates)
                {
                    writer.WriteLine();
                    writer.WriteLine("#region Delegate: {0}Delegate", operation.Name );
                    writer.WriteLine("///<summary>");
                    writer.WriteLine("/// Automatically generated delegate which represents the protocol {0} operation:", protocol.Name);
                    writer.WriteLine("/// Signature: ", operation.Signature);
                    writer.WriteLine("/// Signature Digest: ", operation.Key);
                    writer.WriteLine("///</summary>");
                    writer.WriteLine("internal delegate {0} {1}Delegate({2});", outParams, operation.Name, inParams);
                    writer.WriteLine("#endregion");
                }
            }
        }
        #endregion

        #region GenerateClientExtensions, GenerateClientSend

        internal static void GenerateClientExtensions(TextWriter writer, Protocol protocol)
        {
            protocol.GetAllOperationsWithOutgoingPacket()
                .ForEach(operation =>
                {
                    GenerateClientSend(writer, operation);
                });
        }


        /// <summary>
        /// Generates a manual send for any operation that can send
        /// </summary>
        internal static void GenerateClientSend(TextWriter writer, ProtocolOperation operation)
        {
            var packet = operation.Outgoing;
            var parameters = packet.GetMembers();

            if (parameters.Count > 0)
            {
                writer.WriteLine("/// <summary>");
                writer.WriteLine("/// Sents a reply to the operation: {0}", operation.Description);
                writer.WriteLine("/// </summary>");

                var paramList = parameters
                    .Select(p => String.Format("{0} {1}", p.IsList && p.IsComplexType ? String.Format("IList<{0}>", p.Class) : p.Class, p.Name))
                    .Aggregate((a, b) => String.Format("{0}, {1}", a, b));

                writer.WriteLine("public static void Send{0}Inform(this IClient client, {1})", operation.Name, paramList);
                writer.WriteLine("{");
                /*parameters
                    .ForEach(parameter =>
                    {

                        var paramName = String.Format("p{0}", parameter.Name);
                        if (parameter.IsList && parameter.IsComplexType) // List of exposed entities
                        {
                            writer.WriteLine("var {0} = new {1}();", paramName, parameter.InternalType);
                            writer.WriteLine("for(int i=0; i < {0}.Count; ++i)", parameter.Name);
                            writer.WriteLine("{");
                            writer.WriteLine("{0}.Add( new {1} ( {2}[i] ) ); ",paramName, parameter.InternalElementType, parameter.Name);
                            writer.WriteLine("}");
                        }
                        else if (parameter.IsComplexType) // Exposed entity
                        {
                            writer.WriteLine("var {0} = new {1}({2}); ", paramName, parameter.InternalType, parameter.Name);
                        }
                        else  // List of primitives or a primitive
                        {
                            writer.WriteLine("var {0} = {1}; ", paramName, parameter.Name);
                        }
                    });*/

                // Send now
                var sendList = parameters
                    //.Select(p => String.Format("p{0}", p.Name))
                    .Select(p => p.Name)
                    .Aggregate((a, b) => String.Format("{0}, {1}", a, b));
                //writer.WriteLine("state.Send( new {0} ({1}) );", packet.Name, sendList);

                // Few bugs, but too tired to fix!
                writer.WriteLine("{0} packet = {0}.Metadata.AcquireInform() as {0};", packet.Name);
                parameters.ForEach(element =>
                {
                    writer.WriteLine("packet.{0} = {1};", element.Name, element.Name);
                });
                writer.WriteLine("client.Send(packet);");

                writer.WriteLine("}");
                writer.WriteLine();


                // Direct packet send
                writer.WriteLine("/// <summary>");
                writer.WriteLine("/// Sents a reply to the operation: {0}", operation.Description);
                writer.WriteLine("/// </summary>");
                writer.WriteLine("public static void Send{0}Inform(this IClient client, {1} packet)", operation.Name, operation.Outgoing.Name);
                writer.WriteLine("{");
                writer.WriteLine("client.Send(packet);");
                writer.WriteLine("}");


            }
        }
        #endregion

        #region GenerateHubBase
        internal static void GenerateHubBase(TextWriter writer, Protocol protocol)
        {
            // Class name of the hub
            var hubName = String.Format("{0}HubBase", protocol.Name);

            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// Constructs a new instance of a <see cref=\"{0}\"/> class. The instance will be locked", hubName);
            writer.WriteLine("/// with a default randomly generated publish key.");
            writer.WriteLine("/// </summary>");
            writer.WriteLine("public {0}() : base()", hubName);
            writer.WriteLine("{");
            writer.WriteLine("}");
            writer.WriteLine();

            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// Constructs a new instance of a <see cref=\"{0}\"/> class. The instance will be locked", hubName);
            writer.WriteLine("/// with a default randomly generated publish key.");
            writer.WriteLine("/// </summary>");
            writer.WriteLine("/// <param name=\"name\">The name of the hub.</param>");
            writer.WriteLine("public {0}(string name) : base(name)", hubName);
            writer.WriteLine("{");
            writer.WriteLine("}");
            writer.WriteLine();

            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// Constructs a new instance of a <see cref=\"{0}\"/> class. The instance will be locked with a ", hubName);
            writer.WriteLine("/// default randomly generated publish key.");
            writer.WriteLine("/// </summary>");
            writer.WriteLine("/// <param name=\"autoRegister\">Whether the hub should be automatically registered in the provider or not.</param>");
            writer.WriteLine("/// <param name=\"name\">The name of the hub.</param>");
            writer.WriteLine("public {0}(string name, bool autoRegister)", hubName);
            writer.WriteLine("    : base(name, autoRegister)");
            writer.WriteLine("{");
            writer.WriteLine("}");
            writer.WriteLine();

            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// Constructs a new instance of a <see cref=\"{0}\"/> class. ", hubName);
            writer.WriteLine("/// </summary>");
            writer.WriteLine("/// <param name=\"name\">The name of the hub.</param>");
            writer.WriteLine("/// <param name=\"defaultPublishKey\">The default publish key to apply to this hub.</param>");
            writer.WriteLine("/// <param name=\"defaultSubscribeKey\">The default subscribe key to apply to this hub.</param>");
            writer.WriteLine("public {0}(string name, string defaultPublishKey, string defaultSubscribeKey)", hubName);
            writer.WriteLine("    : base(name, defaultPublishKey, defaultSubscribeKey)");
            writer.WriteLine("{");
            writer.WriteLine("}");
            writer.WriteLine();

            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// Constructs a new instance of a <see cref=\"{0}\"/> class.", hubName);
            writer.WriteLine("/// </summary>");
            writer.WriteLine("/// <param name=\"name\">The name of the hub.</param>");
            writer.WriteLine("/// <param name=\"defaultPublishKey\">The default publish key to apply to this hub.</param>");
            writer.WriteLine("public {0}(string name, string defaultPublishKey)", hubName);
            writer.WriteLine("    : base(name, defaultPublishKey)");
            writer.WriteLine("{");
            writer.WriteLine("}");
            writer.WriteLine();

            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// Constructs a new instance of a <see cref=\"{0}\"/> class.", hubName);
            writer.WriteLine("/// </summary>");
            writer.WriteLine("/// <param name=\"name\">The name of the hub.</param>");
            writer.WriteLine("/// <param name=\"defaultPublishKey\">The default publish key to apply to this hub.</param>");
            writer.WriteLine("/// <param name=\"autoRegister\">Whether the hub should be automatically registered in the provider or not.</param>");
            writer.WriteLine("public {0}(string name, string defaultPublishKey, bool autoRegister)", hubName);
            writer.WriteLine("    : base(name, defaultPublishKey, autoRegister)");
            writer.WriteLine("{");
            writer.WriteLine("}");
            writer.WriteLine();

            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// Constructs a new instance of a <see cref=\"{0}\"/> class.", hubName);
            writer.WriteLine("/// </summary>");
            writer.WriteLine("/// <param name=\"name\">The name of the hub.</param>");
            writer.WriteLine("/// <param name=\"defaultPublishKey\">The default publish key to apply to this hub.</param>");
            writer.WriteLine("/// <param name=\"defaultSubscribeKey\">The default subscribe key to apply to this hub.</param>");
            writer.WriteLine("/// <param name=\"autoRegister\">Whether the hub should be automatically registered in the provider or not.</param>");
            writer.WriteLine("public {0}(string name, string defaultPublishKey, string defaultSubscribeKey, bool autoRegister)", hubName);
            writer.WriteLine("    : base(name, defaultPublishKey, defaultSubscribeKey,  autoRegister)");
            writer.WriteLine("{");
            writer.WriteLine("}");
            writer.WriteLine();

            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// Invoked when a new instance of a <see cref=\"Hub\"/> is constructed.");
            writer.WriteLine("/// </summary>");
            writer.WriteLine("protected override void OnAfterConstruct()");
            writer.WriteLine("{");
            writer.WriteLine("// Call the base");
            writer.WriteLine("base.OnAfterConstruct();");
            writer.WriteLine();
            writer.WriteLine("// Hook the handlers");
            foreach (var operation in protocol.Operation)
            {
                if (operation.Direction == Direction.Push)
                    continue;

                writer.WriteLine("{0}.{1} += this.On{1};", protocol.Name, operation.Name);
            }

            writer.WriteLine("}");

            writer.WriteLine();
            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// Releases the unmanaged resources used by the ByteSTream class and optionally releases the managed resources.");
            writer.WriteLine("/// </summary>");
            writer.WriteLine("/// <param name=\"disposing\">If set to true, release both managed and unmanaged resources, othewise release only unmanaged resources. </param>");
            writer.WriteLine("protected override void Dispose(bool disposing)");
            writer.WriteLine("{");
            writer.WriteLine("// Call the base");
            writer.WriteLine("base.Dispose(disposing);");
            writer.WriteLine("");
            writer.WriteLine("// Unhook the handlers.");
            foreach (var operation in protocol.Operation)
            {
                if (operation.Direction == Direction.Push)
                    continue;

                writer.WriteLine("{0}.{1} -= this.On{1};", protocol.Name, operation.Name);
            }
            writer.WriteLine("}");

            writer.WriteLine();
            foreach (var operation in protocol.Operation)
            {
                if (operation.Direction == Direction.Push)
                    continue;

                writer.WriteLine();
                writer.WriteLine("/// <summary>");
                writer.WriteLine("/// Invoked when an incoming request for {0} operation comes in.", operation.Name);
                writer.WriteLine("/// </summary>");
                writer.WriteLine("public abstract void On{0}(IClient client{1});", 
                    operation.Name,
                    operation.Incoming != null
                        ? String.Format(", {0}Request packet", operation.Name)
                        : "");

            }
        }
        #endregion

    }
}
