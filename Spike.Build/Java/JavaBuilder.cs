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

namespace Spike.Build.Java
{
    internal class JavaBuilder : IBuilder
    {
        internal static string GetNativeType(Member member)
        {
            switch (member.Type)
            {
                case "Byte":
                case "SByte":
                    return "byte";                
                case "Int16":
                case "UInt16":
                    return "short";
                case "Int32":
                case "UInt32":
                    return "int";
                case "Int64":
                case "UInt64":
                    return "long";

                case "Boolean":
                    return "boolean";
                case "Single":
                    return "float";
                case "Double":
                    return "double";
                case "String":
                    return "String";

                case "DateTime":
                    return "Date";

                case "Dynamic":
                    return "Object";

                default: //CustomType & DateTime
                    return member.Type;
            }

        }

        public void Build(Model model, string output)
        {
            if (string.IsNullOrEmpty(output))
                output = @"Java";
            //com.misakai.spike.network
            var networkDirectory = Path.Combine(output, "com", "misakai", "spike", "network");
            if (!Directory.Exists(networkDirectory))
                Directory.CreateDirectory(networkDirectory);

            var packetsDirectory = Path.Combine(output, "com", "misakai", "spike", "network", "packets");
            if (!Directory.Exists(packetsDirectory))
                Directory.CreateDirectory(packetsDirectory);

            var customTypesDirectory = Path.Combine(output, "com", "misakai", "spike", "network", "entities");
            if (!Directory.Exists(customTypesDirectory))
                Directory.CreateDirectory(customTypesDirectory);

            Extentions.CopyFromRessources("Spike.Build.Java.StaticFiles.AbstractTcpChannel.java", Path.Combine(networkDirectory, @"AbstractTcpChannel.java"));
            Extentions.CopyFromRessources("Spike.Build.Java.StaticFiles.CLZF.java", Path.Combine(networkDirectory, @"CLZF.java"));
            Extentions.CopyFromRessources("Spike.Build.Java.StaticFiles.ConnectionHandler.java", Path.Combine(networkDirectory, @"ConnectionHandler.java"));
            Extentions.CopyFromRessources("Spike.Build.Java.StaticFiles.DisconnectionHandler.java", Path.Combine(networkDirectory, @"DisconnectionHandler.java"));
            Extentions.CopyFromRessources("Spike.Build.Java.StaticFiles.PacketHandler.java", Path.Combine(networkDirectory, @"PacketHandler.java"));


            var tcpChanneltemplate = new TcpChannelTemplate();
            var tcpChannelsession = new Dictionary<string, object>();
            tcpChanneltemplate.Session = tcpChannelsession;

            tcpChannelsession["Model"] = model;
            tcpChanneltemplate.Initialize();

            var code = tcpChanneltemplate.TransformText();
            File.WriteAllText(Path.Combine(networkDirectory, @"TcpChannel.java"), code);

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
                File.WriteAllText(Path.Combine(packetsDirectory, string.Format(@"{0}.java", receive.Name)), code);
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
                File.WriteAllText(Path.Combine(customTypesDirectory, string.Format(@"{0}.java", customType.Name)), code);
            }

        }
    }
}
