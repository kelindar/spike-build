#region Copyright (c) 2009-2013 Misakai Ltd.
/*************************************************************************
 * 
 * ROMAN ATACHIANTS - CONFIDENTIAL
 * ===============================
 * 
 * THIS PROGRAM IS CONFIDENTIAL  AND PROPRIETARY TO  ROMAN  ATACHIANTS AND 
 * MAY  NOT  BE  REPRODUCED,  PUBLISHED  OR  DISCLOSED TO  OTHERS  WITHOUT 
 * ROMAN ATACHIANTS' WRITTEN AUTHORIZATION.
 *
 * COPYRIGHT (c) 2009 - 2012. THIS WORK IS UNPUBLISHED.
 * All Rights Reserved.
 * 
 * NOTICE:  All information contained herein is,  and remains the property 
 * of Roman Atachiants  and its  suppliers,  if any. The  intellectual and 
 * technical concepts contained herein are proprietary to Roman Atachiants
 * and  its suppliers and may be  covered  by U.S.  and  Foreign  Patents, 
 * patents in process, and are protected by trade secret or copyright law.
 * 
 * Dissemination of this information  or reproduction  of this material is 
 * strictly  forbidden  unless prior  written permission  is obtained from 
 * Roman Atachiants.
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
