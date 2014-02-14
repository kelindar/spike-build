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
using Spike.Build.Client;

namespace Spike.Build
{
    /// <summary>
    /// Represents the output information of a builder.
    /// </summary>
    public class BuildResult
    {
        /// <summary>
        /// Constructs a new instance of build output.
        /// </summary>
        /// <param name="builder">The client builder which created the output</param>
        /// <param name="description">The human-readable description of the output</param>
        /// <param name="path">The path to the file/directory that contains the output</param>
        public BuildResult(ClientBuilder builder, string description, string path)
        {
            this.Description = description;
            this.FilePath = path;
            this.CreatedWith = builder;
            this.CreatedOn = DateTime.Now;
        }


        /// <summary>
        /// Gets the human-readable description of the output.
        /// </summary>
        public string Description
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the path to the file/directory that contains the output.
        /// </summary>
        public string FilePath
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the language of the output.
        /// </summary>
        public string FileName
        {
            get { return new FileInfo(this.FilePath).Name; }
        }

        /// <summary>
        /// Gets the language of the output.
        /// </summary>
        public string Language
        {
            get { return this.CreatedWith.Language; }
        }

        /// <summary>
        /// Gets the client builder which created the output.
        /// </summary>
        public ClientBuilder CreatedWith
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the time of the creation of the output.
        /// </summary>
        public DateTime CreatedOn
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets the contents of the build result.
        /// </summary>
        public byte[] GetContents()
        {
            return File.ReadAllBytes(this.FilePath);
        }

    }

}
