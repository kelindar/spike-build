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
