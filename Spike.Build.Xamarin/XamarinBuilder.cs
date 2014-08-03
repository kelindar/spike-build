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

    internal class XamarinBuilder : IBuilder
    {
        

        public void Build(Model model, string output, string format)
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

                //CLZF.cs
                template.Target = "LZF";
                File.WriteAllText(Path.Combine(output, @"CLZF.cs"), template.TransformText());
                template.Clear();

                //TcpChannelBase.cs
                template.Target = "TcpChannelBase";
                File.WriteAllText(Path.Combine(output, @"TcpChannelBase.cs"), template.TransformText());
                template.Clear();

                //TcpChannel.cs
                template.Target = "TcpChannel";
                File.WriteAllText(Path.Combine(output, @"TcpChannel.cs"), template.TransformText());
                template.Clear();

                //Make packets
                template.Target = "Packet";
                foreach (var receive in model.Receives)
                {
                    template.TargetOperation = receive;
                    File.WriteAllText(Path.Combine(output, string.Format(@"{0}.cs", receive.Name)), template.TransformText());
                    template.Clear();
                }

                //Make CustomType
                template.Target = "ComplexType";
                foreach (var customType in model.CustomTypes)
                {
                    template.TargetType = customType;
                    File.WriteAllText(Path.Combine(output, string.Format(@"{0}.cs", customType.Name)), template.TransformText());
                    template.Clear();
                }
            }
        }
    }
}
