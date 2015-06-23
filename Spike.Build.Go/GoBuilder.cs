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

namespace Spike.Build.Go
{
    partial class GoTemplate : ITemplate
    {
        public string Target { get; set; }
        public Model Model { get; set; }
        public Operation TargetOperation { get; set; }
        public CustomType TargetType { get; set; }
    }

    /// <summary>
    /// Represents a base builder for JavaScript, containing helper methods.
    /// </summary>
    internal class GoBuilder : BuilderBase
    {
        /// <summary>
        /// Gets the extension for this builder.
        /// </summary>
        public override string Extension
        {
            get { return ".go"; }
        }

        internal static string GetNativeType(Member member)
        {
            return GetNativeType(member.Type);
        }

        internal static string GetNativeType(string type)
        {
            switch (type)
            {
                case "Byte":
                    return "byte";
                case "UInt16":
                    return "uint16";
                case "UInt32":
                    return "uint32";
                case "UInt64":
                    return "uint64";

                case "SByte":
                    return "int8";
                case "Int16":
                    return "int16";
                case "Int32":
                    return "int32";
                case "Int64":
                    return "int64";

                case "DateTime":
                    return "time.Time";
                case "Boolean":
                    return "bool";
                case "Single":
                    return "float32";
                case "Double":
                    return "float64";
                case "String":
                    return "string";

                case "DynamicType":
                    return "interface{}";

                default: //CustomType 
                    return type;
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
                output = @"Go";

            if (!Directory.Exists(output))
                Directory.CreateDirectory(output);

            output = Path.Combine(output, "src/spike");
            if (!Directory.Exists(output))
                Directory.CreateDirectory(output);

            var template = new GoTemplate();
            template.Model = model;

            // Build packet_compressor.go
            this.BuildTarget("PacketCompressor", output, template, ConventionType.Underscore);

            // Build packet_reader.go
            this.BuildTarget("PacketReader", output, template, ConventionType.Underscore);

            // Build packet_writer.go
            this.BuildTarget("PacketWriter", output, template, ConventionType.Underscore);

            // Build tcp_channel.go
            this.BuildTarget("TcpChannel", output, template, ConventionType.Underscore);

            //Make packets
            template.Target = "Packet";
            foreach (var receive in model.Receives)
            {
                // Build the operation
                this.BuildOperation(receive, output, template, ConventionType.Underscore);
            }

            //Make CustomType
            template.Target = "ComplexType";
            foreach (var customType in model.CustomTypes)
            {
                // Build the type
                this.BuildType(customType, output, template, ConventionType.Underscore);
            }

            
        }
    }
}
