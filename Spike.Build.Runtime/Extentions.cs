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

using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Spike.Build
{
    internal static class Extentions
    {
        public static string CamelCase(this string text)
        {
            if (text != null && text.Length > 0 && char.IsUpper(text[0]))
            {
                var array = text.ToCharArray();
                array[0] = char.ToLower(array[0]);
                return new string(array);
            }
            return text;
        }

        public static string PascalCase(this string text)
        {
            if (text != null && text.Length > 0 && char.IsLower(text[0]))
            {
                var array = text.ToCharArray();
                array[0] = char.ToUpper(array[0]);
                return new string(array);
            }
            return text;
        }

        public static string WithoutInform(this string text)
        {
            if (text == null || text.Length == 0 || !text.EndsWith("Inform"))
                return text;
            return text.Substring(0, text.Length - 6);
        }

        public static void CopyFromRessources(string source, string destination)
        {
            using (var sourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(source))
            using (var destinationStream = File.OpenWrite(destination))
                sourceStream.CopyTo(destinationStream);
        }

        /// <summary>
        /// Gets the attribute value with necessary checks.
        /// </summary>
        /// <param name="source">The element that should have the attribute.</param>
        /// <param name="attributeName">The name of the attribute to fetch.</param>
        /// <returns>The value found, otherwise null.</returns>
        public static string GetAttributeValue(this XElement source, string attributeName)
        {
            var element = source.Attribute(attributeName);
            if (element == null)
                return null;

            if (string.IsNullOrWhiteSpace(element.Value))
                return null;
            return element.Value;
        }

        /// <summary>
        /// Executes the template and returns the text.
        /// </summary>
        /// <param name="indent">Whether we should indent it or not.</param>
        /// <param name="template">The template to execute.</param>
        /// <returns>The compiled text.</returns>
        public static string AsText(this ITemplate template, bool indent = false)
        {
            var text = template.TransformText().Trim();
            return indent
                ? text.Indent()
                : text;
        }


        /// <summary>
        /// Helper method that indents the C-like code.
        /// </summary>
        /// <param name="code">The code to indent.</param>
        /// <returns>The indented code.</returns>
        public static string Indent(this string code)
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

            return result.ToString();
        }
    }

    internal static class KeyExtensions
    {
        private const uint Seed = 37;

        /// <summary>
        /// Computes MurmurHash3 on this set of bytes and returns the calculated hash value.
        /// </summary>
        /// <param name="data">The data to compute the hash of.</param>
        /// <returns>A 32bit hash value.</returns>
        public static uint GetMurmurHash3(this string signature)
        {
            byte[] data = Encoding.UTF8.GetBytes(signature);

            const uint c1 = 0xcc9e2d51;
            const uint c2 = 0x1b873593;


            int curLength = data.Length; /* Current position in byte array */
            int length = curLength; /* the const length we need to fix tail */
            uint h1 = Seed;
            uint k1 = 0;

            /* body, eat stream a 32-bit int at a time */
            int currentIndex = 0;
            while (curLength >= 4)
            {
                /* Get four bytes from the input into an UInt32 */
                k1 = (uint)(data[currentIndex++]
                  | data[currentIndex++] << 8
                  | data[currentIndex++] << 16
                  | data[currentIndex++] << 24);

                /* bitmagic hash */
                k1 *= c1;
                k1 = Rotl32(k1, 15);
                k1 *= c2;

                h1 ^= k1;
                h1 = Rotl32(h1, 13);
                h1 = h1 * 5 + 0xe6546b64;
                curLength -= 4;
            }

            /* tail, the reminder bytes that did not make it to a full int */
            /* (this switch is slightly more ugly than the C++ implementation
            * because we can't fall through) */
            switch (curLength)
            {
                case 3:
                    k1 = (uint)(data[currentIndex++]
                      | data[currentIndex++] << 8
                      | data[currentIndex++] << 16);
                    k1 *= c1;
                    k1 = Rotl32(k1, 15);
                    k1 *= c2;
                    h1 ^= k1;
                    break;
                case 2:
                    k1 = (uint)(data[currentIndex++]
                      | data[currentIndex++] << 8);
                    k1 *= c1;
                    k1 = Rotl32(k1, 15);
                    k1 *= c2;
                    h1 ^= k1;
                    break;
                case 1:
                    k1 = data[currentIndex++];
                    k1 *= c1;
                    k1 = Rotl32(k1, 15);
                    k1 *= c2;
                    h1 ^= k1;
                    break;
            };

            // finalization, magic chants to wrap it all up
            h1 ^= (uint)length;
            h1 = Mix(h1);
            
            return BitConverter.ToUInt32(BitConverter.GetBytes(h1).Reverse().ToArray(),0);            
        }

        private static uint Rotl32(uint x, byte r)
        {
            return (x << r) | (x >> (32 - r));
        }

        private static uint Mix(uint h)
        {
            h ^= h >> 16;
            h *= 0x85ebca6b;
            h ^= h >> 13;
            h *= 0xc2b2ae35;
            h ^= h >> 16;
            return h;
        }

    }

}
