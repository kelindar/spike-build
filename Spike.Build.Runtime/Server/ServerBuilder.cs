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

namespace Spike.Build.Server
{
    public class ServerBuilder : BuilderBase
    {
        public static ProtocolModel CSharpServerModel { get; private set; }
        public List<BuildFileInfo> Sources = new List<BuildFileInfo>();

        #region IBuilder Members

        public override string GenerateCode(string inputFileContent)
        {
            // Declare builders
            var BuilderForProtocol = new ServerProtocolBuilder();
            var BuilderForElement  = new ServerElementBuilder();
            var BuilderForPacket   = new ServerPacketBuilder();

            // Check if we already have the model
            if (CSharpServerModel == null)
                CSharpServerModel = new ProtocolModel();

            // Begin code generation
            using (var writer = new CodeWriter())
            {
                var xml = Protocol.Deserialize(inputFileContent);
                if (xml != null)
                {
                    // Preprocessing: mutate by cloning the protocol and transforming it
                    var protocol = CSharpServerModel.Mutate(xml);
                    protocol.RawSpml = inputFileContent;
                    
                    // 1st step: transform the model for Server
                    var elements = protocol.GetAllPackets()
                        .SelectMany(packet => packet.GetAllMembers()).ToList();
                    foreach (var element in elements)
                        InitializeElement(element);

                    // 2nd step, generate the code
                    GenerateHeader(writer, protocol);
                    writer.WriteLine();
                    writer.WriteLine("namespace {0}", String.IsNullOrWhiteSpace(protocol.Namespace)
                        ? "Spike.Network"
                        : protocol.Namespace);
                    writer.WriteLine("{");
                    {
                        // Protocol Info
                        BuilderForProtocol.GenerateCode(protocol, this, writer);

                        // All Entities
                        protocol.GetAllComplexElementsDistinct()
                            .ForEach(element => BuilderForElement.GenerateCode(element, this, writer));

                        // All Packets
                        protocol.GetAllPackets()
                            .ForEach(packet => BuilderForPacket.GenerateCode(packet, this, writer));

                    }
                    writer.WriteLine("}");
                    
                }

                return writer.ToString();
            }
        }

        /// <summary>
        /// Prepares the elements, altering the model
        /// </summary>
        private static void InitializeElement(Element element)
        {
            element.InternalType = element.Class;
            element.InternalName = element.Name;

            switch(element.Type)
            {
                case ElementType.Enum:
                {
                    element.InternalElementType = "Int32";
                    break;
                }

                case ElementType.DynamicType:
                {
                    element.InternalType = element.InternalElementType = element.Class = "Object";
                    break;
                }

                case ElementType.ComplexType:
                {
                    element.InternalType = element.InternalElementType = element.Class;
                    break;
                }

                case ElementType.ListOfByte:
                {
                    element.InternalType = element.Class = "byte[]";
                    element.InternalElementType = "byte";
                    break;
                }

                case ElementType.ListOfDynamicType:
                {
                    element.InternalType = element.Class = "IList<Object>";
                    element.InternalElementType = "Object";
                    break;
                }

                case ElementType.ListOfComplexType:
                {
                    element.InternalType = String.Format("IList<{0}>", element.Class);
                    element.InternalElementType = element.Class;
                    break;
                }

                default:
                {
                    element.InternalElementType = element.Class;

                    // List of primitives
                    if (element.IsList && element.IsSimpleType)
                    {
                        var simpleType = element.Class.Replace("ListOf", "");
                        element.InternalType = String.Format("IList<{0}>", simpleType);
                        element.InternalElementType = simpleType;
                        element.Class = String.Format("IList<{0}>", simpleType);
                    }
                    break;
                }
            }

        }

        #endregion

        #region GenerateHeader
        private static void GenerateHeader(TextWriter writer, Protocol protocol)
        {
            writer.WriteLine(@"// ------------------------------------------------------------------------------");
            writer.WriteLine(@"//  <auto-generated>");
            writer.WriteLine(@"//     This code was generated by a tool (Spike Build).");
            writer.WriteLine(@"//     Generated on: " + DateTime.Now.ToLongTimeString());
            writer.WriteLine(@"//     Runtime Version: " + Environment.Version.ToString() );
            writer.WriteLine(@"//");
            writer.WriteLine(@"//     Changes to this file may cause incorrect behavior and will be lost if");
            writer.WriteLine(@"//     the code is regenerated.");
            writer.WriteLine(@"//  </auto-generated>");
            writer.WriteLine(@"//------------------------------------------------------------------------------");

            var nss = new List<string>
            {
                "System",
                "System.IO",
                "System.Net",
                "System.Linq",
                "System.Runtime.Serialization",
                "System.Collections.Generic",
                "Spike",
                "Spike.Hubs",
                "Spike.Network",
                "Spike.Network.Http"
            };

            var namespaces = protocol.GetAllNamespaces();
            namespaces.AddRange(nss);
            namespaces.ForEach(ns => writer.WriteLine("using {0};", ns));
        }
        #endregion

    }
}
