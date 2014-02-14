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
using System.Security.Cryptography;

namespace Spike.Build
{
    public partial class ProtocolOperation
    {
        /// <summary>
        /// Gets or sets the operation key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets the parent <see cref="Protocol"/> object.
        /// </summary>
        public Protocol Parent { get; set; }

        /// <summary>
        /// Gets or sets a human readable operation signature.
        /// </summary>
        public string Signature { get; set; }


        #region Internal Stuff
        /// <summary>
        /// Gets the value of the Key property cleaned-up from the quotes.
        /// </summary>
        /// <returns>The key.</returns>
        public string GetCleanKey()
        {
            return Key.Replace("\"", String.Empty);
        }

        internal string GetOperationSignature()
        {
            using (var writer = new StringWriter())
            {
                // 1. Protocol name
                writer.Write(Parent.Name);
                writer.Write(".");

                // 2. Operation direction
                writer.Write(Enum.GetName(typeof(Direction), this.Direction));
                writer.Write(".");

                // 3. Operation name
                writer.Write(this.Name);

                // 4. Outgoing 
                if (this.Outgoing != null && this.Outgoing.Member.Count > 0)
                {
                    writer.Write(".");
                    writer.Write(String.Format("[{0}]", this.Outgoing.Member
                                                         .Select(element => element.Type.ToString())
                                                         .Aggregate((a, b) => String.Format("{0}.{1}", a, b))));
                }
                else
                {
                    writer.Write(".");
                    writer.Write("[]");
                }

                // 5. Incoming
                if (this.Incoming != null && this.Incoming.Member.Count > 0)
                {
                    writer.Write(".");
                    writer.Write(String.Format("[{0}]", this.Incoming.Member
                                                        .Select(element => element.Type.ToString())
                                                        .Aggregate((a, b) => String.Format("{0}.{1}", a, b))));
                }
                else
                {
                    writer.Write(".");
                    writer.Write("[]");
                }

                return writer.ToString();
            }
        }

        internal string GetOperationKey()
        {
            // Compute the hash value
            byte[] bytes = Encoding.UTF8
                    .GetBytes(this.Signature)
                    .GetMurmurHash3();

            // Convert to string
            char[] chars = new char[bytes.Length * 2];
            byte current;
            for (int y = 0, x = 0; y < bytes.Length; ++y, ++x)
            {
                current = ((byte)(bytes[y] >> 4));
                chars[x] = (char)(current > 9 ? current + 0x37 : current + 0x30);
                current = ((byte)(bytes[y] & 0xF));
                chars[++x] = (char)(current > 9 ? current + 0x37 : current + 0x30);
            }

            // Get the hash of the string representation
            return String.Format("\"{0}\"",
                new string(chars)
                );
            
        }
        #endregion

        #region Overrides

        public override string ToString()
        {
            return String.Format("Operation: #{0} {1}", Key, Name);
        }

        public override bool Equals(object obj)
        {
            var right = obj as ProtocolOperation;
            if (right == null)
                return false;
            if (right.Key == this.Key)
                return true;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
        #endregion
    }
}
