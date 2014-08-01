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

            if (!Directory.Exists(output))
                Directory.CreateDirectory(output);
                   
            //AbstractTcpChannel.java
            var abstractTcpChannelTemplate = new AbstractTcpChannelTemplate();
            File.WriteAllText(Path.Combine(output, @"AbstractTcpChannel.java"), abstractTcpChannelTemplate.TransformText());

            //CLZF.java
            var clzfTemplate = new CLZFTemplate();
            File.WriteAllText(Path.Combine(output, @"CLZF.java"), clzfTemplate.TransformText());

            //ConnectionHandler.java
            var connectionHandlerTemplate = new ConnectionHandlerTemplate();
            File.WriteAllText(Path.Combine(output, @"ConnectionHandler.java"), connectionHandlerTemplate.TransformText());

            //DisconnectionHandler.java
            var disconnectionHandlerTemplate = new DisconnectionHandlerTemplate();
            File.WriteAllText(Path.Combine(output, @"DisconnectionHandler.java"), disconnectionHandlerTemplate.TransformText());

            //PacketHandler.java
            var packetHandlerTemplate = new PacketHandlerTemplate();
            File.WriteAllText(Path.Combine(output, @"PacketHandler.java"), packetHandlerTemplate.TransformText());


            var tcpChanneltemplate = new TcpChannelTemplate();
            var tcpChannelsession = new Dictionary<string, object>();
            tcpChanneltemplate.Session = tcpChannelsession;

            tcpChannelsession["Model"] = model;
            tcpChanneltemplate.Initialize();

            var code = tcpChanneltemplate.TransformText();
            File.WriteAllText(Path.Combine(output, @"TcpChannel.java"), code);

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
                File.WriteAllText(Path.Combine(output, string.Format(@"{0}.java", receive.Name)), code);
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
                File.WriteAllText(Path.Combine(output, string.Format(@"{0}.java", customType.Name)), code);
            }

        }
    }
}
