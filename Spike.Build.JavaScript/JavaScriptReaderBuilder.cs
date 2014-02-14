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

namespace Spike.Build.Client.JavaScript
{
    public class JavaScriptReaderBuilder
    {
        /// <summary>
        /// Generates operation read operations
        /// </summary>
        public static void GenerateCode(JavaScriptBuilder builder)
        {
            using (var writer = new CodeWriter())
            {
                GenerateOperationReader(builder, writer);
                builder.AddSourceFile(builder.SrcOutputPath, @"OperationReader.js", writer);
            }
        }

        #region GenerateOperationReader
        internal static void GenerateOperationReader(JavaScriptBuilder builder, TextWriter writer)
        {
            writer.WriteLine("function OperationReader()"); // Begin class
            writer.WriteLine("{");
            {
                GenerateReadForOperation(builder, writer);
            }

            writer.WriteLine("};"); // End class
        }

        internal static void GenerateReadForOperation(JavaScriptBuilder builder, TextWriter writer)
        {
            writer.WriteLine("/* This method is generated and called automatically, allows read operation calls */");
            writer.WriteLine("this.read = function(operation, reader)");
            writer.WriteLine("{");
            writer.WriteLine("switch (operation)");
            writer.WriteLine("{");
            builder.Model.OperationsWithOutgoingPacket
                .ForEach(operation =>
                {
                    writer.WriteLine("case {0}:", operation.Key);
                    writer.WriteLine("var packet{0} = new {1}();", operation.GetCleanKey(), operation.Outgoing.Name);
                    writer.WriteLine("packet{0}.read(reader);", operation.GetCleanKey());
                    writer.WriteLine("return packet{0};", operation.GetCleanKey());
                });

            writer.WriteLine("}");
            writer.WriteLine("return null;");
            writer.WriteLine("}");
        }

        #endregion


    }
}
