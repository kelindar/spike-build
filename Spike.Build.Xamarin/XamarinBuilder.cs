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

using Spike.Build.CSharp5;
using System.Collections.Generic;
using System.IO;

namespace Spike.Build.Xamarin
{
    partial class XamarinTemplate
    {
        internal string Target { get; set; }
        internal Model Model { get; set; }
        internal Operation TargetOperation { get; set; }
        internal CustomType TargetType { get; set; }
    }

    internal class XamarinBuilder : CSharp5BuilderBase
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
                var template = new CSharp5Template();
                template.Target = null;
                template.Model = model;

                if (string.IsNullOrEmpty(output))
                    output = @"Xamarin";

                if (!Directory.Exists(output))
                    Directory.CreateDirectory(output);

                File.WriteAllText(Path.Combine(output, @"Network.cs"), template.TransformText());
            }
            else
            {
                if (string.IsNullOrEmpty(output))
                    output = @"Xamarin";

                if (!Directory.Exists(output))
                    Directory.CreateDirectory(output);

                var template = new CSharp5Template();
                template.Model = model;

                // Build LZF.cs
                this.BuildTarget("LZF", output, template);

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
    }
}
