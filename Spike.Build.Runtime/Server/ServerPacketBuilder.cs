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
    public class ServerPacketBuilder : ISubServerBuilder<Packet, ServerBuilder>
    {
        /// <summary>
        /// Generates a packet (outgoing or incoming)
        /// </summary>
        public void GenerateCode(Packet packet, ServerBuilder builder, TextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("#region Packet: {0}", packet.Name);
            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// {0} defines a packet container object for the {1} operation.", packet.Name, packet.Parent.Name);
            writer.WriteLine("/// </summary>");
            writer.WriteLine("public sealed class {0}: SecpPacket", packet.Name); // Begin class
            writer.WriteLine("{");
            {
                // Generate constructors
                GenerateConstructors(writer, packet);

                // Generate fields
                packet.GetMembers().ForEach(prop =>
                    {
                        prop.GenerateProperty(writer);
                    });

                GenerateDirectionProperty(writer, packet);

                // Read/Write methods
                GenerateWriteMethod(writer, packet);
                GenerateReadMethod(writer, packet);
                GenerateCreateInstanceMethod(writer, packet);
                GenerateMetadataProperty(writer, packet);

            }

            //packet.GetMembers().GenerateExposedWrappers(writer);

            writer.WriteLine("}"); // End class
            writer.WriteLine("#endregion"); 
        }

        #region GenerateCreateInstanceMethod

        internal static void GenerateCreateInstanceMethod(TextWriter writer, Packet packet)
        {
            writer.WriteLine();
            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// A factory method that constructs an instance of {0} packet.", packet.Name);
            writer.WriteLine("/// </summary>");
            writer.WriteLine("public static Packet CreateInstance()");
            writer.WriteLine("{");
            writer.WriteLine("return new {0}();", packet.Name);
            writer.WriteLine("}");
        }
        #endregion

        #region GenerateMetadataProperty

        private void GenerateMetadataProperty(TextWriter writer, Packet packet)
        {
            writer.WriteLine();
            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// Gets the metadata for the operation which contains the given packet type.");
            writer.WriteLine("/// </summary>");
            writer.WriteLine("public static OperationInfo Metadata");
            writer.WriteLine("{");
            writer.WriteLine("get");
            writer.WriteLine("{");
            writer.WriteLine("if(OpInfo == null)");
            writer.WriteLine("{");
            writer.WriteLine("OpInfo = PacketIndex.GetMetadata({0});", packet.Parent.Key);
            writer.WriteLine("}");
            writer.WriteLine("return OpInfo;");
            writer.WriteLine("}");
            writer.WriteLine("}");
            writer.WriteLine();
            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// Cached metadata reference for faster access. ");
            writer.WriteLine("/// </summary>");
            writer.WriteLine("private static OperationInfo OpInfo = null;");
        }

        #endregion

        #region GenerateConstructors

        internal static void GenerateConstructors(TextWriter writer, Packet packet)
        {
            var operation = packet.Parent;
            var operationNumber = operation.Key;
            var properties = packet.GetMembers();

            // Default constructor, does nothing
            writer.WriteLine();
            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// Creates a new empty instance of the packet");
            writer.WriteLine("/// </summary>");
            writer.WriteLine("public {0}() : base(PacketKey.Get({1}))", packet.Name, operationNumber.ToString());
            writer.WriteLine("{");
            writer.WriteLine("}");

            if (properties.Count > 0)
            {
                // Set constructor, from the parameters 
                writer.WriteLine();
                writer.WriteLine("/// <summary>");
                writer.WriteLine("/// Creates a new instance of the packet by initializing all its fields");
                writer.WriteLine("/// </summary>");
                var constructorString = String.Format("public {0}(", packet.Name);
                int num = 0;
                properties.ForEach(prop =>
                        {
                            constructorString += String.Format("{0} {1}", prop.InternalType, prop.InternalName);
                            num++;

                            if (properties.Count() > num)
                                constructorString += ", ";
                        });
                writer.WriteLine("{0}) : base(PacketKey.Get({1}))", constructorString, operationNumber.ToString());
                writer.WriteLine("{");
                {
                    properties.ForEach(prop =>
                        {
                            prop.GenerateSetField(writer, String.Format("{0}", prop.InternalName));
                        });
                }
                writer.WriteLine("}");
                writer.WriteLine();
            }
        }
        #endregion

        #region GenerateReadMethod, GenerateWriteMethod
        internal static void GenerateWriteMethod(TextWriter writer, Packet packet)
        {
            writer.WriteLine();
            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// Serializes this complex type to a binary stream.");
            writer.WriteLine("/// </summary>");
            writer.WriteLine("public override void Write(PacketWriter Writer)");
            writer.WriteLine("{");
            packet.Member.GenerateWriteCode(writer);
            writer.WriteLine("}");
        }

        internal static void GenerateReadMethod(TextWriter writer, Packet packet)
        {
            writer.WriteLine();
            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// Serializes this complex type to a binary stream.");
            writer.WriteLine("/// </summary>");
            writer.WriteLine("public override void Read(PacketReader Reader)");
            writer.WriteLine("{");
            packet.Member.GenerateReadCode(writer);
            writer.WriteLine("}");
        }
        #endregion

        #region GenerateDirectionProperty
        public static void GenerateDirectionProperty(TextWriter writer, Packet packet)
        {
            writer.WriteLine();
            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// Gets the direction of the packet: whether the first ");
            writer.WriteLine("/// call is initiated on server (Push) or client (Pull)");
            writer.WriteLine("/// </summary>");
            writer.WriteLine("public sealed override PacketDirection Direction");
            writer.WriteLine("{");
            writer.WriteLine("   get {{ return PacketDirection.{0}; }} ", packet.Direction);
            writer.WriteLine("}");
        }
        #endregion
    }
}
