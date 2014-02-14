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
using System.Reflection;
using System.IO;

namespace Spike.Build
{
    public class BuildFileInfo
    {
        public BuildFileInfo()
        {

        }

        public BuildFileInfo(string filename, string source)
        {
            FileName = filename;
            Source = source;
        }

        /// <summary>
        /// The filename of the file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The generated source code
        /// </summary>
        public string Source { get; set; }

        #region Static Factory Methods

        /// <summary>
        /// Factory method, creates an instance of the GeneratedFileInfo
        /// </summary>
        public static BuildFileInfo CreateInstance(string filename, string source)
        {
            return new BuildFileInfo(filename, source);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Writes a source file to a directory
        /// </summary>
        /// <param name="folder">The source directory to write into</param>
        /// <param name="extension">The extension of the file (Use .cs for C#, .as for ActionScript3)</param>
        public void Write(string folder, string extension)
        {
            // Check if exists
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            // Write the text
            File.WriteAllText(Path.Combine(folder, FileName + extension), Source);
        }

        #endregion
    }
}
