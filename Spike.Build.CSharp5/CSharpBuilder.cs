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

namespace Spike.Build.CSharp5
{
    internal class CSharpBuilder : IBuilder
    {
        internal static string GetNativeType(Member member)
        {
            switch (member.Type)
            {
                case "Byte":
                    return "byte";
                case "UInt16":
                    return "ushort";
                case "UInt32":
                    return "uint";
                case "UInt64":
                    return "ulong";

                case "SByte":
                    return "sbyte";
                case "Int16":
                    return "short";
                case "Int32":
                    return "int";
                case "Int64":
                    return "long";

                case "Boolean":
                    return "bool";
                case "Single":
                    return "float";
                case "Double":
                    return "double";
                case "String":
                    return "string";

                case "Dynamic":
                    return "object";

                default: //CustomType & DateTime
                    return member.Type;
            }

        }

        /// <summary>
        /// Build the model of the specified type.
        /// </summary>
        /// <param name="model">The model to build.</param>
        /// <param name="output">The output type.</param>
        public void Build(Model model, string output)
        {
            if (string.IsNullOrEmpty(output))
                output = @"CSharp";

            var networkDirectory = Path.Combine(output, "Spike", "Network");
            if (!Directory.Exists(networkDirectory))
                Directory.CreateDirectory(networkDirectory);

            var packetsDirectory = Path.Combine(output, "Spike", "Network", "Packets");
            if (!Directory.Exists(packetsDirectory))
                Directory.CreateDirectory(packetsDirectory);

            var customTypesDirectory = Path.Combine(output, "Spike", "Network", "Entities");
            if (!Directory.Exists(customTypesDirectory))
                Directory.CreateDirectory(customTypesDirectory);

            Extentions.CopyFromRessources("Spike.Build.CSharp.StaticFiles.CLZF.cs", Path.Combine(networkDirectory, @"CLZF.cs"));
            Extentions.CopyFromRessources("Spike.Build.CSharp.StaticFiles.TcpChannelBase.cs", Path.Combine(networkDirectory, @"TcpChannelBase.cs"));

            var tcpChanneltemplate = new TcpChannelTemplate();
            var tcpChannelsession = new Dictionary<string, object>();
            tcpChanneltemplate.Session = tcpChannelsession;

            tcpChannelsession["Model"] = model;
            tcpChanneltemplate.Initialize();

            var code = tcpChanneltemplate.TransformText();
            File.WriteAllText(Path.Combine(networkDirectory, @"TcpChannel.cs"), code);

            //Make packets
            var packetTemplate = new PacketTemplate();
            var packetSession = new Dictionary<string, object>();
            packetTemplate.Session = packetSession;
            foreach (var receive in model.Receives)
            {
                packetTemplate.Clear();
                packetSession["Operation"] = receive;
                packetTemplate.Initialize();

                code = packetTemplate.TransformText();
                File.WriteAllText(Path.Combine(packetsDirectory, string.Format(@"{0}.cs", receive.Name)), code);
            }

            //Make CustomType
            var customTypeTemplate = new CustomTypeTemplate();
            var customTypeSession = new Dictionary<string, object>();
            customTypeTemplate.Session = customTypeSession;
            foreach (var customType in model.CustomTypes)
            {
                customTypeTemplate.Clear();
                customTypeSession["CustomType"] = customType;
                customTypeTemplate.Initialize();
                code = customTypeTemplate.TransformText();
                File.WriteAllText(Path.Combine(customTypesDirectory, string.Format(@"{0}.cs", customType.Name)), code);
            }

        }
    }
}
