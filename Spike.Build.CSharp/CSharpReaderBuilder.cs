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

namespace Spike.Build.Client
{
    public class CSharpReaderBuilder
    {
        /// <summary>
        /// Generates operation read operations
        /// </summary>
        public static void GenerateCode(CSharpBuilder builder)
        {
            using (var writer = new CodeWriter())
            {
                GenerateOperationReader(builder, writer);
                builder.AddSourceFile(builder.SrcOutputPath, @"OperationReader.cs", writer);
            }
        }

        #region GenerateOperationReader
        internal static void GenerateOperationReader(CSharpBuilder builder, TextWriter writer)
        {
            CSharpBuilder.GenerateHeader(writer);
            writer.WriteLine("namespace Spike.Network"); // Begin package
            writer.WriteLine("{");
            writer.WriteLine();
            writer.WriteLine("internal sealed class OperationReader : OperationReaderBase"); // Begin class
            writer.WriteLine("{");
            {
                GenerateInstanceSet(builder, writer);
                GenerateReadForOperation(builder, writer);
            }

            writer.WriteLine("}"); // End class
            writer.WriteLine("}"); // End package 


        }

        internal static void GenerateInstanceSet(CSharpBuilder builder, TextWriter writer)
        {
            writer.WriteLine("static OperationReader()");
            writer.WriteLine("{");
            writer.WriteLine("OperationReaderBase.SetInstance(new OperationReader());");
            writer.WriteLine("}");
        }

        internal static void GenerateReadForOperation(CSharpBuilder builder, TextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("/// <summary>");
            writer.WriteLine("/// This method is generated and called automatically, allows read operation calls");
            writer.WriteLine("/// </summary>");
            writer.WriteLine("internal override object Read(string operationKey, PacketReader reader)");
            writer.WriteLine("{");
            writer.WriteLine("switch (operationKey)");
            writer.WriteLine("{");
            builder.Model.Protocols
                .SelectMany(protocol => protocol.GetAllOperationsWithOutgoingPacket())
                .OrderBy(operation => operation.Key).ToList()
                .ForEach(operation =>
                {
                    writer.WriteLine();
                    writer.WriteLine("case {0}:", operation.Key);
                    writer.WriteLine("   {1} packet{0} = new {1}();", operation.GetCleanKey(), operation.Outgoing.Name);
                    writer.WriteLine("   packet{0}.Read(reader);", operation.GetCleanKey());
                    writer.WriteLine("return packet{0};", operation.GetCleanKey());
                });

            writer.WriteLine("}");
            writer.WriteLine("return null;");
            writer.WriteLine("}");
        }

        #endregion


    }
}
