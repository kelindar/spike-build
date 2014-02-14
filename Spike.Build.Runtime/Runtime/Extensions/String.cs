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
