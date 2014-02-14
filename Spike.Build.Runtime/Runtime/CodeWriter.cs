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
using System.Xml;
using System.IO;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Spike.Build
{
    public class CodeWriter : StringWriter
    {
        private int Tabs = 0;
        private bool LastWriteLine = false;

        public CodeWriter()
        {

        }
        public CodeWriter(IFormatProvider formatProvider)
            : base(formatProvider)
        {

        }
        public CodeWriter(StringBuilder sb)
            : base(sb)
        {

        }
        public CodeWriter(StringBuilder sb, IFormatProvider formatProvider)
            : base(sb, formatProvider)
        {

        }

        public override void Write(string value)
        {
            if (LastWriteLine)
            {
                base.Write(GesSpacing("") + value);
                LastWriteLine = false;
            }
            else
            {
                base.Write(value);
            }
        }

        public override void WriteLine(string value)
        {
            if (!LastWriteLine)
                LastWriteLine = true;
            base.WriteLine(GesSpacing(value) + value);
        }

        private string GesSpacing(string value)
        {
            var buffer = this.ToString();
            var a = buffer.Count(symbol => symbol == '{');
            var b = buffer.Count(symbol => symbol == '}');
            var c = value.Count(symbol => symbol == '}');

            Tabs = a - b - c ;
            if (Tabs < 0)
                Tabs = 0;

            var spacing = "";
            for (int i = 0; i < Tabs; i++)
                spacing += "   ";
            return spacing;
        }
    }
}
