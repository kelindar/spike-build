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

namespace Spike.Build.Client.AS3
{
    internal static class AS3BuilderExtensions
    {

        #region Element Extensions - Properties, Read & Write

        internal static void GenerateProperty(this Element element, TextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("private var {0}:{1};", element.GetFieldName(), element.InternalType);
            writer.WriteLine("[Bindable]");
            writer.WriteLine("public function set {0}(value:{1}):void", element.InternalName.FirstLetterLower(), element.InternalType);
            writer.WriteLine("{");
            writer.WriteLine("{0} = value;", element.GetFieldName());
            writer.WriteLine("}");
            writer.WriteLine("public function get {0}():{1}", element.InternalName.FirstLetterLower(), element.InternalType);
            writer.WriteLine("{");
            writer.WriteLine("return {0};", element.GetFieldName());
            writer.WriteLine("}");
            
        }

        internal static string GetFieldName(this Element element)
        {
            return String.Format("_{0}", element.Name);
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
            writer.WriteLine("public function write(writer:PacketWriter):void");
            writer.WriteLine("{");
            element.GetMembers().GenerateWriteCode(writer);
            writer.WriteLine("}");
            writer.WriteLine();
        }

        internal static void GenerateReadMethod(this Element element, TextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("public function read(reader:PacketReader):void");
            writer.WriteLine("{");
            element.GetMembers().GenerateReadCode(writer);
            writer.WriteLine("}");
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
                    writer.WriteLine("writer.WriteInt32({0});", element.GetFieldName());
                    break;
                }

                case ElementType.ListOfByte:
                {
                    writer.WriteLine("writer.WriteByteArray({0});", element.GetFieldName());
                    break;
                }

                case ElementType.ListOfDynamicType:
                {
                    writer.WriteLine("writer.WriteArrayOfDynamic({0});", element.GetFieldName());
                    break;
                }

                case ElementType.DynamicType:
                {
                    writer.WriteLine("writer.WriteDynamic({0});", element.GetFieldName());
                    break;
                }

                case ElementType.ListOfComplexType:
                {
                    writer.WriteLine("writer.WriteArray({0});", element.GetFieldName());
                    break;
                }

                case ElementType.ComplexType:
                {
                    writer.WriteLine("writer.WriteEntity({0});", element.GetFieldName());
                    break;
                }

                default:
                {
                    if (element.IsSimpleType && element.IsList)
                    {
                        // Is list (or array) of primitives
                        writer.WriteLine("writer.WriteArrayOf{0}({1});", element.ServerElementType, element.GetFieldName());
                    } 
                    else if(element.IsSimpleType)
                    {
                        // Is a supported primitive
                        writer.WriteLine("writer.Write{0}({1});", element.Type.ToString(), element.GetFieldName());
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
                    writer.WriteLine("{0} = reader.ReadInt32();", element.GetFieldName());
                    break;
                }

                case ElementType.ListOfByte:
                {
                    writer.WriteLine("{0} = reader.ReadByteArray();", element.GetFieldName());
                    break;
                }

                case ElementType.DynamicType:
                {
                    writer.WriteLine("{0} = reader.ReadDynamic();", element.GetFieldName());
                    break;
                }

                case ElementType.ListOfDynamicType:
                {
                    writer.WriteLine("{0} = reader.ReadArrayOfDynamic();", element.GetFieldName());
                    break;
                }

                case ElementType.ListOfComplexType:
                {
                    writer.WriteLine(@"network.packets.{0};",  element.InternalElementType); // Needed for stupid flex compiler
                    writer.WriteLine(@"{0} = reader.ReadArrayOfEntity(""network.packets.{1}"");", element.GetFieldName(), element.InternalElementType);
                    break;
                }

                case ElementType.ComplexType:
                {
                    writer.WriteLine("{0} = new {1}();", element.GetFieldName(), element.InternalType);
                    writer.WriteLine("reader.ReadEntity({0});", element.GetFieldName());
                    break;
                }

                default:
                {
                    if (element.IsSimpleType && element.IsList)
                    {
                        // Is list (or array) of primitives
                        writer.WriteLine("{0} = reader.ReadArrayOf{1}();", element.GetFieldName(), element.ServerElementType);
                    } 
                    else if(element.IsSimpleType)
                    {
                        // Is a supported primitive
                        writer.WriteLine("{0} = reader.Read{1}();", element.GetFieldName(), element.Type.ToString());
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
