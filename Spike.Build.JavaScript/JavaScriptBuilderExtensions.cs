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

namespace Spike.Build.Client.JavaScript
{
    internal static class JavaScriptBuilderExtensions
    {

        #region Element Extensions - Properties, Read & Write

        internal static void GenerateProperty(this Element element, TextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("/* Public property for the field {0} */", element.Name );
            writer.WriteLine("this.{0};", element.GetFieldName());
        }

        internal static string GetFieldName(this Element element)
        {
            return element.Name.FirstLetterLower();
        }


        internal static void GenerateWriteCode(this List<Element> elements, TextWriter writer)
        {
            elements.ForEach(prop =>
            {
                prop.GenerateWriteProperty(writer);
            });
        }

        internal static void GenerateReadCode(this List<Element> elements, TextWriter writer)
        {
            elements.ForEach(prop =>
            {
                prop.GenerateReadProperty(writer);
            });
        }
        internal static void GenerateWriteMethod(this Element element, TextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("this.write = function(writer)");
            writer.WriteLine("{");
            element.GetMembers().GenerateWriteCode(writer);
            writer.WriteLine("};");
            writer.WriteLine();
        }

        internal static void GenerateReadMethod(this Element element, TextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("this.read = function(reader)");
            writer.WriteLine("{");
            element.GetMembers().GenerateReadCode(writer);
            writer.WriteLine("};");
            writer.WriteLine();
        }


        /// <summary>
        /// Generates a write operation for a particular property
        /// </summary>
        internal static void GenerateWriteProperty(this Element element, TextWriter writer)
        {
            switch (element.Type)
            {
                case ElementType.Enum:
                {
                    writer.WriteLine("writer.writeInt32(this.{0});", element.GetFieldName());
                    break;
                }

                case ElementType.ListOfByte:
                {
                    writer.WriteLine("writer.writeByteArray(this.{0});", element.GetFieldName());
                    break;
                }

                case ElementType.ListOfDynamicType:
                {
                    writer.WriteLine("writer.writeArrayOfDynamic(this.{0});", element.GetFieldName());
                    break;
                }

                case ElementType.DynamicType:
                {
                    writer.WriteLine("writer.writeDynamic(this.{0});", element.GetFieldName());
                    break;
                }

                case ElementType.ListOfComplexType:
                {
                    writer.WriteLine("writer.writeArray(this.{0});", element.GetFieldName());
                    break;
                }

                case ElementType.ComplexType:
                {
                    writer.WriteLine("writer.writeEntity(this.{0});", element.GetFieldName());
                    break;
                }

                default:
                {
                    if (element.IsSimpleType && element.IsList)
                    {
                        // Is list (or array) of primitives
                        writer.WriteLine("writer.writeArrayOf{0}(this.{1});", element.ServerElementType, element.GetFieldName());
                    } 
                    else if(element.IsSimpleType)
                    {
                        // Is a supported primitive
                        writer.WriteLine("writer.write{0}(this.{1});", element.Type.ToString(), element.GetFieldName());
                    }
                    else
                    {
                        writer.WriteLine(@" /!\ Protocol have defined an unsupported {0} type /!\", element.Type.ToString());
                    }
                    break;
                }
            }

        }

        /// <summary>
        /// Generates a read operation for a particular property
        /// </summary>
        internal static void GenerateReadProperty(this Element element, TextWriter writer)
        {
            switch (element.Type)
            {
                case ElementType.Enum:
                {
                    writer.WriteLine("this.{0} = reader.readInt32();", element.GetFieldName());
                    break;
                }

                case ElementType.ListOfByte:
                {
                    writer.WriteLine("this.{0} = reader.readByteArray();", element.GetFieldName());
                    break;
                }

                case ElementType.DynamicType:
                {
                    writer.WriteLine("this.{0} = reader.readDynamic();", element.GetFieldName());
                    break;
                }

                case ElementType.ListOfDynamicType:
                {
                    writer.WriteLine("this.{0} = reader.readArrayOfDynamic();", element.GetFieldName());
                    break;
                }

                case ElementType.ListOfComplexType:
                {
                    writer.WriteLine(@"this.{0} = reader.readArrayOfEntity('{1}');", element.GetFieldName(), element.InternalElementType);
                    break;
                }

                case ElementType.ComplexType:
                {
                    writer.WriteLine("this.{0} = new {1}();", element.GetFieldName(), element.InternalType);
                    writer.WriteLine("reader.readEntity(this.{0});", element.GetFieldName());
                    break;
                }

                default:
                {
                    if (element.IsSimpleType && element.IsList)
                    {
                        // Is list (or array) of primitives
                        writer.WriteLine("this.{0} = reader.readArrayOf{1}();", element.GetFieldName(), element.ServerElementType);
                    } 
                    else if(element.IsSimpleType)
                    {
                        // Is a supported primitive
                        writer.WriteLine("this.{0} = reader.read{1}();", element.GetFieldName(), element.Type.ToString());
                    }
                    else
                    {
                        writer.WriteLine(@" /!\ Protocol have defined an unsupported {0} type /!\", element.Type.ToString());
                    }
                    break;
                }
            }

        }


        #endregion

        #region ProtocolOperation Extensions - Naming
        internal static string GetRequestMethodName(this ProtocolOperation operation)
        {
            return operation.Name.FirstLetterLower();
        }

        internal static string GetInformMethodName(this ProtocolOperation operation)
        {
            return operation.Name.FirstLetterLower() + "Inform";
        }
        #endregion

    }
}
