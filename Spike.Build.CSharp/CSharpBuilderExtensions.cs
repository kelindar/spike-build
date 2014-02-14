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

namespace Spike.Build.Client.CSharp
{
    internal static class CSharpBuilderExtensions
    {

        #region Element Extensions - Properties, Read & Write

        internal static void GenerateProperty(this Element element, TextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("private {0} {1};", element.InternalType, element.GetFieldName());
            writer.WriteLine("public {0} {1}", element.InternalType, element.InternalName);
            writer.WriteLine("{");
            writer.WriteLine("set");
            writer.WriteLine("{");
            writer.WriteLine("{0} = value;", element.GetFieldName());
            writer.WriteLine("}");
            writer.WriteLine("get");
            writer.WriteLine("{");
            writer.WriteLine("return {0};", element.GetFieldName());
            writer.WriteLine("}");
            writer.WriteLine("}");
        }

        internal static string GetFieldName(this Element element)
        {
            return String.Format("f{0}", element.Name);
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
            writer.WriteLine("public void Write(PacketWriter Writer)");
            writer.WriteLine("{");
            element.GetMembers().GenerateWriteCode(writer);
            writer.WriteLine("}");
            writer.WriteLine();
        }

        internal static void GenerateReadMethod(this Element element, TextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("public void Read(PacketReader Reader)");
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
            if (element.IsList && element.IsComplexType)
                writer.WriteLine("Writer.Write<{0}>({1});", element.InternalElementType, element.InternalName);
            else if (element.IsDynamicType)
                writer.WriteLine("Writer.WriteDynamic({0});", element.InternalName);
            else
                writer.WriteLine("Writer.Write({0});", element.InternalName);

        }

        /// <summary>
        /// Generates a read operation for a particular property
        /// </summary>
        internal static void GenerateReadProperty(this Element element, TextWriter writer)
        {
            if (element.Type == ElementType.Enum)
                writer.WriteLine("this.{0} = Reader.ReadInt32();", element.InternalName);
            else if (element.Type == ElementType.ListOfByte)
            {
                // Byte array
                writer.WriteLine("this.{0} = Reader.ReadByteArray();", element.InternalName);
            }
            else if (element.IsDynamicType && element.IsList)
            {
                // List of dynamic types
                writer.WriteLine("this.{0} = Reader.ReadListOfDynamic();", element.InternalName);
            }
            else if (element.IsDynamicType)
            {
                // Simple dynamic type
                writer.WriteLine("this.{0} = Reader.ReadDynamic();", element.InternalName);
            }
            else if (element.IsSimpleType && element.IsList)
            {
                // Is a supported list of primitives
                writer.WriteLine("this.{0} = Reader.Read{1}();", element.InternalName, element.Type.ToString());
            }
            else if (element.IsSimpleType)
            {
                // Is a supported primitive
                writer.WriteLine("this.{0} = Reader.Read{1}();", element.InternalName, element.InternalType);
            }
            else if (element.IsList && element.IsComplexType)
            {
                // A list of exposed entities
                writer.WriteLine("this.{0} = Reader.ReadListOfEntity<{1}>();", element.InternalName, element.InternalElementType);
            }
            else if (element.IsComplexType)
            {
                // Exposed entity
                writer.WriteLine("this.{0} = new {1}();", element.InternalName, element.InternalType);
                writer.WriteLine("Reader.ReadEntity(this.{0});", element.InternalName);
            }
            else if (element.IsSimpleType && element.IsList)
            {
                // Is list (or array) of primitives
                writer.WriteLine("this.{0} = Reader.ReadListOf{1}();", element.InternalName, element.InternalType);
            }
            else
                writer.WriteLine(@" /!\ Protocol have defined an unsupported {0} type /!\", element.InternalType);
        }


        #endregion

        #region ProtocolOperation Extensions - Naming
        internal static string GetRequestMethodName(this ProtocolOperation operation)
        {
            return operation.Name;
        }

        internal static string GetInformMethodName(this ProtocolOperation operation)
        {
            return operation.Name + "Inform";
        }
        #endregion

    }
}
