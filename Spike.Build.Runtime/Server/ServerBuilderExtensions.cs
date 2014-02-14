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
    public static class ServerBuilderExtensions
    {
        #region Element Extensions
        public static void GenerateProperty(this Element element, TextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// Gets or sets the {0} field. {1}", element.InternalName, element.Description.FirstLetterUpper());
            writer.WriteLine("/// </summary>");
            writer.WriteLine("public {0} {1};", element.InternalType, element.InternalName);
        }


        public static string TryGenerateListConstruct(this Element element)
        {
            var Result = "";
            if (element.Type == ElementType.ListOfByte)
            {
                Result = " new byte[0]";
            }
            else if (element.IsList) // That's a list
            {
                Result = String.Format(" new {0}()", element.InternalType);
                //if ((Primitives.Contains(Element) && IsOnlyArray(prop.PropertyType)) || prop.PropertyType.Name.Contains("List<"))
                //    Result = String.Format(" new {0}[0]", GetElementType(prop.PropertyType).Name);
            }
            return Result;
        }


        public static void GenerateSetField(this Element element, TextWriter writer, string getFrom)
        {
            var Box = "";
            var Cast = "";
            if (element.Type == ElementType.Enum && !element.IsList)
                Box = "(int)";
            /*else if (element.IsList) // That's a list
            {
                //Cast = String.Format(".ToList<{0}>()", element.InternalType);
                //if ((Primitives.Contains(Element) && IsOnlyArray(prop.PropertyType)) || prop.PropertyType.Name.Contains("List<"))
                //    Cast = ""; // No cast
            }*/

            //if (prop.GetGetMethod() != null)
            writer.WriteLine("this.{0} = {1}{2}{3};", element.InternalName, Box, getFrom, Cast);
        }

        public static void GenerateGetField(this Element element, TextWriter writer, string setTo)
        {
            var Box = "";
            var Cast = "";

            if (element.Type == ElementType.Enum && !element.IsList)
                Box = String.Format("({0})", element.Class);
            /*else if (element.IsList) // That's a list
            {
                Cast = String.Format(".ToList<{0}>()", element.Class);
            }
            */
            //if (prop.GetSetMethod() != null)
            writer.WriteLine("{0} = {1}this.{2}{3};", setTo, Box, element.InternalName, Cast);
        }


        #region GenerateWriteCode, GenerateReadCode, GenerateWriteProperty, GenerateReadProperty
        public static void GenerateWriteCode(this List<Element> elements, TextWriter writer)
        {
            elements.ForEach(prop =>
                {
                    prop.GenerateWriteProperty(writer);
                });
        }

        public static void GenerateReadCode(this List<Element> elements, TextWriter writer)
        {
            elements.ForEach(prop =>
                {
                    prop.GenerateReadProperty(writer);
                });
        }

        public static void GenerateWriteProperty(this Element element, TextWriter writer)
        {
            if (element.IsList && element.IsComplexType)
                writer.WriteLine("Writer.Write<{0}>({1});", element.InternalElementType, element.InternalName);
            else if (element.IsDynamicType) 
                writer.WriteLine("Writer.WriteDynamic({0});", element.InternalName);
            else if (element.Type == ElementType.Enum)
                writer.WriteLine("Writer.Write((Int32){0});", element.InternalName);
            else
                writer.WriteLine("Writer.Write({0});", element.InternalName);
        }

        public static void GenerateReadProperty(this Element element, TextWriter writer)
        {
            if (element.Type == ElementType.Enum)
                writer.WriteLine("this.{0} = ({1}) Reader.ReadInt32();", element.InternalName, element.Class);
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
            else if (element.IsList && element.IsComplexType )
            {
                // A list of exposed entities
                writer.WriteLine("this.{0} = Reader.ReadListOfComplexType<{1}>();", element.InternalName, element.InternalElementType);
            }
            else if (element.IsComplexType)
            {
                // Exposed entity
                writer.WriteLine("this.{0} = new {1}(Reader);", element.InternalName, element.InternalType);
                //writer.WriteLine("Reader.ReadComplexType(this.{0});", element.InternalName);
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


        #endregion

        #region Operation Extensions
        public static IEnumerable<string> GetIncomingFunctionTypesAndNames(this ProtocolOperation operation)
        {
            if(operation.Incoming == null) return new List<string>();

            return operation.GetIncomingFunctionTypes()
                .Zip(operation.GetIncomingFunctionNames(), (type, name) => String.Format("{0} {1}", type, name));
        }

        public static IEnumerable<string> GetIncomingFunctionTypes(this ProtocolOperation operation)
        {
            if (operation.Incoming == null) return new List<string>();
            return operation.Incoming.Member
                   .Select(element => element.IsComplexType ?
                          (element.IsList ? String.Format("IList<{0}>", element.Class) : element.Class) : (element.InternalType));
        }

        public static IEnumerable<string> GetIncomingFunctionNames(this ProtocolOperation operation)
        {
            if (operation.Incoming == null) return new List<string>();
            return operation.Incoming.Member
                   .Select(element => element.InternalName);
        }

        public static string GetOutgoingFunctionType(this ProtocolOperation operation)
        {
            return operation.Outgoing == null ? "void" :
                (operation.Outgoing.Member.Count == 1 ? operation.Outgoing.Member[0].InternalType : operation.Outgoing.Name);

        }
        #endregion
    }
}
