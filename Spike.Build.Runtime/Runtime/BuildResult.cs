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
