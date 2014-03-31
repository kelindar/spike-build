using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spike.Build.Java {
    static class JavaBuilderExtensions {
        /// <summary>
        /// Return the string with the first letter lowered. 
        /// </summary>        
        /// <example>
        /// var text = "MyVariable";
        /// Console.WriteLine(text.CamelCase()); //Show myVariable
        /// </example>
        internal static string CamelCase(this string text)
        {
            var array = text.ToCharArray();
            array[0] = char.ToLower(array[0]);
            return new string(array);            
        }

        /// <summary>
        /// Return the string with the first letter uppered. 
        /// </summary>        
        /// <example>
        /// var text = "byte";
        /// Console.WriteLine(text.PascalCase()); //Show Byte
        /// </example>
        internal static string PascalCase(this string text) {
            var array = text.ToCharArray();
            array[0] = char.ToUpper(array[0]);
            return new string(array);
        }

        /// <summary>
        /// Convert an ElementType to a Java type name.  
        /// </summary>
        /// <param name="spikeType">Spike protocol type to convert</param>
        /// <returns>Java type name</returns>
        internal static string SpikeToJavaType(ElementType spikeType) {
            switch (spikeType) {
                case ElementType.Byte:
                    return "byte";
                case ElementType.Int16:
                case ElementType.UInt16:
                    return "short";
                case ElementType.Int32:
                case ElementType.UInt32:
                    return "int";
                case ElementType.Int64:
                case ElementType.UInt64:
                    return "long";
                case ElementType.Boolean:
                    return "boolean";
                case ElementType.Single:
                    return "float";
                case ElementType.Double:
                    return "double";
                case ElementType.String:
                    return "String";
                case ElementType.DateTime:
                    return "Date";
                case ElementType.ListOfInt32:
                case ElementType.ListOfUInt32:
                    return "int[]";
                case ElementType.ListOfByte:
                    return "byte[]";
            }
            Console.WriteLine(spikeType);
            return "UNKNOW";
        }
    }

    
}
