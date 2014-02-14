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

namespace Spike.Build
{
    public interface IBuilder
    {
        /// <summary>
        /// Validates a protocol definition
        /// </summary>
        void Validate(string inputFileContent, string inputFilePath);

        /// <summary>
        /// Generates the code for an input file
        /// </summary>
        string GenerateCode(string inputFileContent);

        /// <summary>
        /// Gets whether the input file is valid or not
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Fired whenever a compilation error has occured
        /// </summary>
        event EventHandler<BuildErrorEventArgs> Error;
    }

    public class BuildErrorEventArgs : EventArgs
    {
        public uint Level { get; private set; }
        public string Message { get; private set; }
        public uint Line { get; private set; }
        public uint Column { get; private set; }

        public BuildErrorEventArgs(uint level, string message, uint line, uint column)
        {
            Level = level;
            Message = message;
            Line = line;
            Column = column;
        }
    }
}