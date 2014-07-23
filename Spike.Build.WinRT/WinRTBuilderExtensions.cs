using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spike.Build.WinRT {
    static class WinRTBuilderExtensions {
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
        /// Convert an ElementType to a ---- type name.  
        /// </summary>
        /// <param name="spikeType">Spike protocol type to convert</param>
        /// <returns>---- type name</returns>
        internal static string SpikeToCSharpType(ElementType spikeType) {
            switch (spikeType) {
                case ElementType.Byte:
                    return "byte";
                case ElementType.Int16:
                    return "short";
                case ElementType.UInt16:
                    return "ushort";
                case ElementType.Int32:
                    return "int";
                case ElementType.UInt32:
                    return "uint";
                case ElementType.Int64:
                    return "long";
                case ElementType.UInt64:
                    return "ulong";
                case ElementType.Boolean:
                    return "bool";
                case ElementType.Single:
                    return "float";
                case ElementType.Double:
                    return "double";
                case ElementType.String:
                    return "string";
                case ElementType.DateTime:
                    return "DateTime";
                case ElementType.ListOfInt32:
                    return "int[]";
                case ElementType.ListOfUInt32:
                    return "uint[]";
                case ElementType.ListOfInt16:
                    return "short[]";
                case ElementType.ListOfUInt16:
                    return "ushort[]";
                case ElementType.ListOfByte:
                    return "byte[]";
            }
            //Console.WriteLine(spikeType);
            return "UNKNOW";
            
        }
    }

    
}
