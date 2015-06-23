/************************************************************************
*
* Copyright (C) 2009-2014 Misakai Ltd
*
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
* 
* You should have received a copy of the GNU General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
* 
*************************************************************************/

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Spike.Build.CSharp5
{
    partial class CSharp5Template : ITemplate
    {
        public string Target { get; set; }
        public Model Model { get; set; }
        public Operation TargetOperation { get; set; }
        public CustomType TargetType { get; set; }
    }

    /// <summary>
    /// Represents a base builder for C#5, containing helper methods.
    /// </summary>
    internal abstract class CSharp5BuilderBase : BuilderBase
    {
        /// <summary>
        /// Gets the extension for this builder.
        /// </summary>
        public override string Extension
        {
            get { return ".cs"; }
        }

        /// <summary>
        /// Gets whether the build should apply identation.
        /// </summary>
        public override bool Indent
        {
            get { return true; }
        }

        internal static string GetNativeType(Member member)
        {
            switch (member.Type)
            {
                case "Byte":
                    return "byte";
                case "UInt16":
                    return "ushort";
                case "UInt32":
                    return "uint";
                case "UInt64":
                    return "ulong";

                case "SByte":
                    return "sbyte";
                case "Int16":
                    return "short";
                case "Int32":
                    return "int";
                case "Int64":
                    return "long";

                case "Boolean":
                    return "bool";
                case "Single":
                    return "float";
                case "Double":
                    return "double";
                case "String":
                    return "string";

                case "DynamicType":
                    return "object";

                default: //CustomType & DateTime
                    return member.Type;
            }

        }

    
    }
}
