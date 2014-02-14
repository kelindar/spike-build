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
