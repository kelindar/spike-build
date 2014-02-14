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
    public class ServerElementBuilder : ISubServerBuilder<Element, ServerBuilder>
    {
        public void GenerateCode(Element element, ServerBuilder builder, TextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("#region ComplexType: {0}", element.InternalElementType);
            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// Complex type {0}. {1}", element.Class, element.Description.FirstLetterUpper());
            writer.WriteLine("/// </summary>");
            writer.WriteLine("public struct {0} : IComplexType", element.InternalElementType); // Begin class
            writer.WriteLine("{");
            {
                // Generate constructors
                GenerateConstructors(writer, element);

                // Generate fields
                element.GetMembers()
                    .ForEach(prop => prop.GenerateProperty(writer));

                // Read/Write methods
                GenerateWriteMethod(writer, element);
                GenerateReadMethod(writer, element);

            }
            writer.WriteLine("}"); // End class
            writer.WriteLine("#endregion");
        }

        #region GenerateConstructors

        internal static void GenerateConstructors(TextWriter writer, Element element)
        {
            var properties = element.GetMembers();

            // Read Constructor
            writer.WriteLine();

            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// Creates a new instance of the complex type by deserializing from a binary stream.");
            writer.WriteLine("/// </summary>");
            writer.WriteLine("/// <param name=\"Reader\">The PacketReader that should be used to deserialize.</param>");
            writer.WriteLine("public {0}(PacketReader Reader)", element.InternalElementType);
            writer.WriteLine("{");
            properties.GenerateReadCode(writer);
            writer.WriteLine("}");
            writer.WriteLine();
            
            // Parameters Constructor
            if (properties.Count > 0)
            {
                writer.WriteLine("/// <summary>");
                writer.WriteLine("/// Creates a new instance of complex type by initializing all its fields.");
                writer.WriteLine("/// </summary>");
                properties.ForEach(prop =>
                        {
                            writer.WriteLine("/// <param name=\"{0}\">The value for {0} field.</param>", prop.InternalName);
                        });
                var constructorString = String.Format("public {0}(", element.InternalElementType);
                int num = 0;
                properties.ForEach(prop =>
                        {
                            constructorString += String.Format("{0} {1}", prop.InternalType, prop.InternalName);
                            num++;

                            if (properties.Count() > num)
                                constructorString += ", ";
                        });
                writer.WriteLine("{0})", constructorString);
                writer.WriteLine("{");
                {
                    properties.ForEach(prop =>
                        {
                            writer.WriteLine("this.{0} = {1};", prop.InternalName, prop.InternalName);
                        });
                }
                writer.WriteLine("}");
            }
        }
        #endregion

        #region GenerateWriteMethod, GenerateReadMethod
        internal static void GenerateWriteMethod(TextWriter writer, Element element)
        {
            writer.WriteLine();
            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// Serializes this complex type to a binary stream.");
            writer.WriteLine("/// </summary>");
            writer.WriteLine("/// <param name=\"Writer\">The PacketWriter that should be used for serialization.</param>");
            writer.WriteLine("public void Write(PacketWriter Writer)");
            writer.WriteLine("{");
            element.GetMembers().GenerateWriteCode(writer);
            writer.WriteLine("}");
        }

        internal static void GenerateReadMethod(TextWriter writer, Element element)
        {
            writer.WriteLine();
            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// Deserializes this complex type from a binary stream.");
            writer.WriteLine("/// </summary>");
            writer.WriteLine("/// <param name=\"Reader\">The PacketReader that should be used for deserialization.</param>");
            writer.WriteLine("public void Read(PacketReader Reader)");
            writer.WriteLine("{");
            element.GetMembers().GenerateReadCode(writer);
            writer.WriteLine("}");
        }
        #endregion

    }
}
