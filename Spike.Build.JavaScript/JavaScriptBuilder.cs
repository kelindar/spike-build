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

namespace Spike.Build.JavaScript
{
    partial class JavaScriptTemplate : ITemplate
    {
        public string Target { get; set; }
        public Model Model { get; set; }
        public Operation TargetOperation { get; set; }
        public CustomType TargetType { get; set; }
    }

    /// <summary>
    /// Represents a base builder for JavaScript, containing helper methods.
    /// </summary>
    internal class JavaScriptBuilder : BuilderBase
    {
        /// <summary>
        /// Gets the extension for this builder.
        /// </summary>
        public override string Extension
        {
            get { return ".js"; }
        }

        internal static string GetNativeType(Member member)
        {
            switch (member.Type)
            {
                case "Byte":
                    return "Number";
                case "UInt16":
                    return "Number";
                case "UInt32":
                    return "Number";
                case "UInt64":
                    return "Number";

                case "SByte":
                    return "Number";
                case "Int16":
                    return "Number";
                case "Int32":
                    return "Number";
                case "Int64":
                    return "Number";

                case "DateTime":
                    return "Date";
                case "Boolean":
                    return "bool";
                case "Single":
                    return "Number";
                case "Double":
                    return "Number";
                case "String":
                    return "string";

                case "DynamicType":
                    return "object";


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
            if (format == "single")
            {
                var template = new JavaScriptTemplate();
                template.Target = null;
                template.Model = model;

                if (string.IsNullOrEmpty(output))
                    output = @"JavaScript";

                if (!Directory.Exists(output))
                    Directory.CreateDirectory(output);

                var compiled = template.TransformText();
                File.WriteAllText(Path.Combine(output, @"spike-sdk.js"), compiled);
                File.WriteAllText(Path.Combine(output, @"spike-sdk.min.js"), new Minifier().MinifyJavaScript(compiled));
            }
            else
            {
                if (string.IsNullOrEmpty(output))
                    output = @"JavaScript";

                if (!Directory.Exists(output))
                    Directory.CreateDirectory(output);

                var template = new JavaScriptTemplate();
                template.Model = model;

                // Build ByteArray.js
                this.BuildTarget("ByteArray", output, template);

                // Build PacketCompressor.js
                this.BuildTarget("PacketCompressor", output, template);

                // Build PacketReader.js
                this.BuildTarget("PacketReader", output, template);

                // Build PacketWriter.js
                this.BuildTarget("PacketWriter", output, template);

                // Build Engine.js
                this.BuildTarget("Engine", output, template);

                // Build ServerSocket.js
                this.BuildTarget("ServerSocket", output, template);

                // Build ServerChannel.js
                this.BuildTarget("ServerChannel", output, template);

                //Make CustomType
                /*template.Target = "ComplexType";
                foreach (var customType in model.CustomTypes)
                {
                    // Build the type
                    this.BuildType(customType, output, template);
                }*/
            }
        }
    }
}
