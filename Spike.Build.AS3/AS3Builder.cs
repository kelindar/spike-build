/************************************************************************
*
* Copyright (C) 2009-2014 Misakai Ltd
*
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
* 
* You should have received a copy of the GNU General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
* 
*************************************************************************/

using Spike.Build.Minifiers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Spike.Build.AS3
{
    partial class AS3Template : ITemplate
    {
        public string Target { get; set; }
        public Model Model { get; set; }
        public Operation TargetOperation { get; set; }
        public CustomType TargetType { get; set; }
    }

    /// <summary>
    /// Represents a base builder for JavaScript, containing helper methods.
    /// </summary>
    internal class AS3Builder : BuilderBase
    {
        /// <summary>
        /// Gets the extension for this builder.
        /// </summary>
        public override string Extension
        {
            get { return ".as"; }
        }

        internal static string GetNativeType(Member member)
        {
            if (member.Type == "Byte" && member.IsList)
                return "ByteArray";
            if (member.IsList)
                return "Array";

            switch (member.Type)
            {
                case "Byte":   return "uint";
                case "UInt16": return "uint";
                case "UInt32": return "uint";
                case "UInt64": return "UInt64";
                case "SByte":  return "int";
                case "Int16": return "int";
                case "Int32": return "int";
                case "Int64": return "Int64";
                case "DateTime": return "Date";
                case "Boolean": return "Boolean";
                case "Single": return "Number";
                case "Double": return "Number";
                case "String": return "String"; 
                case "DynamicType": return "Object";
                default: //CustomType 
                    return member.Type;
            }
        }


        /// <summary>
        /// Build the model of the specified type.
        /// </summary>
        /// <param name="model">The model to build.</param>
        /// <param name="output">The output type.</param>
        /// <param name="format">The format to apply.</param>
        public override void Build(Model model, string output, string format)
        {
            if (string.IsNullOrEmpty(output))
                output = @"AS3";

            if (!Directory.Exists(output))
                Directory.CreateDirectory(output);

            // Output folder
            output = Path.Combine(output, "spike");
            if (!Directory.Exists(output))
                Directory.CreateDirectory(output);

            // Events folder
            var events = Path.Combine(output, "events");
            if (!Directory.Exists(events))
                Directory.CreateDirectory(events);

            // Packets folder
            var packets = Path.Combine(output, "packets");
            if (!Directory.Exists(packets))
                Directory.CreateDirectory(packets);

            // MXML folder
            var mxml = Path.Combine(output, "mxml");
            if (!Directory.Exists(mxml))
                Directory.CreateDirectory(mxml);

            var template = new AS3Template();
            template.Model = model;

            // Build single files
            this.BuildTarget("PacketCompressor", output, template);
            this.BuildTarget("PacketReader", output, template);
            this.BuildTarget("PacketWriter", output, template);
            this.BuildTarget("TcpChannel", output, template);
            this.BuildTarget("TcpSocket", output, template);
            this.BuildTarget("Int64", output, template);
            this.BuildTarget("UInt64", output, template);
            this.BuildTarget("IEntity", output, template);
            this.BuildTarget("IPacket", output, template);
            this.BuildTarget("Packet", output, template);

            // Events
            this.BuildTarget("ConnectionEvent", events, template);
            this.BuildTarget("PacketReceiveEvent", events, template);
            this.BuildTarget("SocketReceiveEvent", events, template);

            // MXML
            this.BuildTarget("MxmlChannel", mxml, template);

            //Make packets
            template.Target = "CustomPacket";
            foreach (var receive in model.Receives)
            {
                // Build the operation
                this.BuildOperation(receive, packets, template);
            }

            //Make CustomType
            template.Target = "ComplexType";
            foreach (var customType in model.CustomTypes)
            {
                // Build the type
                this.BuildType(customType, packets, template);
            }

            
        }
    }
}
