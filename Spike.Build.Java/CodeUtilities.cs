using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spike.Build.Java {
    static class CodeUtilities {
        internal static string LowerFirstChar(string text) {
            var array = text.ToCharArray();
            array[0] = char.ToLower(array[0]);
            return new string(array);
        }

        internal static string UpperFirstChar(string text) {
            var array = text.ToCharArray();
            array[0] = char.ToUpper(array[0]);
            return new string(array);
        }

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
