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
    partial class CSharp5Template
    {
        internal string Target { get; set; }
        internal Model Model { get; set; }
        internal Operation TargetOperation { get; set; }
        internal CustomType TargetType { get; set; }
    }

    /// <summary>
    /// Represents a base builder for C#5, containing helper methods.
    /// </summary>
    internal abstract class CSharp5BuilderBase : IBuilder
    {
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

        /// <summary>
        /// Build the model of the specified type.
        /// </summary>
        /// <param name="model">The model to build.</param>
        /// <param name="output">The output type.</param>
        /// <param name="format">The format to apply.</param>
        public abstract void Build(Model model, string output, string format);

        /// <summary>
        /// Helper method that builds a template target.
        /// </summary>
        /// <param name="target">The target name.</param>
        /// <param name="outputDirectory">The output directory for the file.</param>
        /// <param name="template">The template to use.</param>
        protected void BuildTarget(string target, string outputDirectory, CSharp5Template template)
        {
            template.Target = target;
            File.WriteAllText(
                Path.Combine(outputDirectory, target + ".cs"),
                this.Indent(template.TransformText()));
            template.Clear();
        }

        /// <summary>
        /// Helper method that builds a template target.
        /// </summary>
        /// <param name="operation">The target operation.</param>
        /// <param name="outputDirectory">The output directory for the file.</param>
        /// <param name="template">The template to use.</param>
        protected void BuildOperation(Operation operation, string outputDirectory, CSharp5Template template)
        {
            template.TargetOperation = operation;
            File.WriteAllText(
                Path.Combine(outputDirectory, string.Format(@"{0}.cs", operation.Name)),
                this.Indent(template.TransformText())
                );
            template.Clear();
        }

        /// <summary>
        /// Helper method that builds a template target.
        /// </summary>
        /// <param name="type">The target type.</param>
        /// <param name="outputDirectory">The output directory for the file.</param>
        /// <param name="template">The template to use.</param>
        protected void BuildType(CustomType type, string outputDirectory, CSharp5Template template)
        {
            template.TargetType = type;
            File.WriteAllText(
                Path.Combine(outputDirectory, string.Format(@"{0}.cs", type.Name)),
                this.Indent(template.TransformText())
                );
            template.Clear();
        }



        #region Indent Members

        /// <summary>
        /// Helper method that indents the C# code.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        protected string Indent(string code)
        {
            const string INDENT_STEP = "    ";

            if (string.IsNullOrWhiteSpace(code))
            {
                return code;
            }

            var result = new StringBuilder();
            var indent = string.Empty;
            var lineContent = false;
            var stringDefinition = false;

            for (var i = 0; i < code.Length; i++)
            {
                var ch = code[i];

                if (ch == '"' && !stringDefinition)
                {
                    result.Append(ch);
                    stringDefinition = true;
                    continue;
                }

                if (ch == '"' && stringDefinition)
                {
                    result.Append(ch);
                    stringDefinition = false;
                    continue;
                }

                if (stringDefinition)
                {
                    result.Append(ch);
                    continue;
                }

                if (ch == '{' && !stringDefinition)
                {
                    if (lineContent)
                    {
                        result.AppendLine();
                    }

                    result.Append(indent).Append("{");

                    if (lineContent)
                    {
                        result.AppendLine();
                    }

                    indent += INDENT_STEP;
                    lineContent = false;

                    continue;
                }

                if (ch == '}' && !stringDefinition)
                {
                    if (indent.Length != 0)
                    {
                        indent = indent.Substring(0, indent.Length - INDENT_STEP.Length);
                    }

                    if (lineContent)
                    {
                        result.AppendLine();
                    }

                    result.Append(indent).Append("}");

                    if (lineContent)
                    {
                        result.AppendLine();
                    }


                    lineContent = false;

                    continue;
                }

                if (ch == '\r')
                {
                    continue;
                }

                if ((ch == ' ' || ch == '\t') && !lineContent)
                {
                    continue;
                }

                if (ch == '\n')
                {
                    lineContent = false;
                    result.AppendLine();

                    continue;
                }

                if (!lineContent)
                {
                    result.Append(indent);
                    lineContent = true;
                }

                result.Append(ch);
            }

            return result.ToString().Trim();
        }
        #endregion
    }
}
