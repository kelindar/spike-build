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
using Spike;

namespace System.Linq
{
    public static class StringExtensions
    {
        /// <summary>
        /// Converts the string, capitalizing first letter
        /// </summary>
        public static string FirstLetterUpper(this string source)
        {
            if (source == null)
                return null;

            if (source.Length == 0) 
                return source;

            return source.ToUpper()[0] + source.Substring(1, source.Length - 1);
        }


        /// <summary>
        /// Converts the string, lowering the first letter
        /// </summary>
        public static string FirstLetterLower(this string source)
        {
            if (source == null) 
                return null;

            if (source.Length == 0) 
                return source;

            return source.ToLower()[0] + source.Substring(1, source.Length - 1);
        }

		/// <summary>
        /// Converts the windows path string to the current platform
        /// </summary>
        public static string AsPath(this string source)
        {
            if (source == null) 
                return null;

            if (source.Length == 0)
                return source;

            //source = source.RemoveLastSlash();
            return source.Replace('\\', System.IO.Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Removes last slash or backslash if there is one.
        /// </summary>
        public static string RemoveLastSlash(this string source)
        {
            if (source.EndsWith(@"\"))
                source = source.Remove(source.Length - 1, 1);

            if (source.EndsWith(@"/"))
                source = source.Remove(source.Length - 1, 1);

            return source;
        }


    }
}
