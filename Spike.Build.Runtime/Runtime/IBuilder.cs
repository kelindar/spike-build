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