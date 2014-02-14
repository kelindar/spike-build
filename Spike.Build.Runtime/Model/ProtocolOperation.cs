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
