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

using System.Collections.Generic;
using System.IO;

namespace Spike.Build.WinRT
{
    partial class WinRTTemplate
    {
        internal string Target { get; set; }
        internal Model Model { get; set; }
        internal Operation TargetOperation { get; set; }
        internal CustomType TargetType { get; set; }
    }

    internal class WinRTBuilder : Spike.Build.CSharp5.CSharp5BuilderBase
    {
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
                var template = new WinRTTemplate();
                template.Target = null;
                template.Model = model;

                if (string.IsNullOrEmpty(output))
                    output = @"WinRT";

                if (!Directory.Exists(output))
                    Directory.CreateDirectory(output);

                File.WriteAllText(Path.Combine(output, @"SpikeSdk.cs"), template.TransformText());
            }
            else
            {
                if (string.IsNullOrEmpty(output))
                    output = @"WinRT";

                if (!Directory.Exists(output))
                    Directory.CreateDirectory(output);

                var template = new WinRTTemplate();
                template.Model = model;


                // Build LZF.cs
                this.BuildTarget("LZF", output, template);

                // Build PacketReader.cs
                this.BuildTarget("PacketReader", output, template);

                // Build PacketWriter.cs
                this.BuildTarget("PacketWriter", output, template);

                // Build TcpChannelBase.cs
                this.BuildTarget("TcpChannelBase", output, template);

                // Build TcpChannel.cs
                this.BuildTarget("TcpChannel", output, template);

                //Make packets
                template.Target = "Packet";
                foreach (var receive in model.Receives)
                {
                    // Build the operation
                    this.BuildOperation(receive, output, template);
                }

                //Make CustomType
                template.Target = "ComplexType";
                foreach (var customType in model.CustomTypes)
                {
                    // Build the type
                    this.BuildType(customType, output, template);
                }
            }
        }

        #region WinRT support

        /// <summary>
        /// Helper method that builds a template target.
        /// </summary>
        /// <param name="target">The target name.</param>
        /// <param name="outputDirectory">The output directory for the file.</param>
        /// <param name="template">The template to use.</param>
        protected void BuildTarget(string target, string outputDirectory, WinRTTemplate template)
        {
            template.Target = target;
            File.WriteAllText(
                Path.Combine(outputDirectory, target + ".cs"),
                this.Indent(template.TransformText()));
            template.Clear();
        }

        /// <summary>
        /// Helper method that builds a template target.
        /// </summary>
        /// <param name="operation">The target operation.</param>
        /// <param name="outputDirectory">The output directory for the file.</param>
        /// <param name="template">The template to use.</param>
        protected void BuildOperation(Operation operation, string outputDirectory, WinRTTemplate template)
        {
            template.TargetOperation = operation;
            File.WriteAllText(
                Path.Combine(outputDirectory, string.Format(@"{0}.cs", operation.Name)),
                this.Indent(template.TransformText())
                );
            template.Clear();
        }

        /// <summary>
        /// Helper method that builds a template target.
        /// </summary>
        /// <param name="type">The target type.</param>
        /// <param name="outputDirectory">The output directory for the file.</param>
        /// <param name="template">The template to use.</param>
        protected void BuildType(CustomType type, string outputDirectory, WinRTTemplate template)
        {
            template.TargetType = type;
            File.WriteAllText(
                Path.Combine(outputDirectory, string.Format(@"{0}.cs", type.Name)),
                this.Indent(template.TransformText())
                );
            template.Clear();
        }
        #endregion
    }

    
}
