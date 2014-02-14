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
    public class ProtocolDefinitionFile
    {

        private string fProtocolName;
        private string fRawSpml;

        public ProtocolDefinitionFile(string protocolName, string rawSpml)
        {
			fProtocolName = protocolName;
            fRawSpml = rawSpml;
        }

        /// <summary>
        /// Gets Protocol name for this protocol definition
        /// </summary>
        public string ProtocolName
        {
            get { return fProtocolName; }
        }

        /// <summary>
        /// Gets SPML string for this protocol definition
        /// </summary>
        public string Spml
        {
            get { return fRawSpml; }
        }

        public override string ToString()
        {
            return fProtocolName;
        }
    }
}
