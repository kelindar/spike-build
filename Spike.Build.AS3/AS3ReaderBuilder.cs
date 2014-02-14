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

namespace Spike.Build.Client
{
    public class AS3ReaderBuilder
    {
        /// <summary>
        /// Generates operation read operations
        /// </summary>
        public static void GenerateCode(AS3Builder builder)
        {
            using (var writer = new CodeWriter())
            {
                GenerateOperationReader(builder, writer);
                builder.AddSourceFile(builder.HandlerPath, @"OperationReader.as", writer);
            }
        }

        #region GenerateOperationReader
        internal static void GenerateOperationReader(AS3Builder builder, TextWriter writer)
        {
            writer.WriteLine("package network"); // Begin package
            writer.WriteLine("{");

            writer.WriteLine("import flash.events.*;");
            writer.WriteLine("import mx.events.*;");
            writer.WriteLine("import mx.rpc.events.ResultEvent;");
            writer.WriteLine("import network.events.*;");
            writer.WriteLine("import network.packets.*;");
            writer.WriteLine();
            writer.WriteLine("public final class OperationReader"); // Begin class
            writer.WriteLine("{");
            {
                GenerateReadForOperation(builder, writer);
            }

            writer.WriteLine("}"); // End class
            writer.WriteLine("}"); // End package 


        }

        internal static void GenerateReadForOperation(AS3Builder builder, TextWriter writer)
        {
            writer.WriteLine("/**");
            writer.WriteLine(" * This method is generated and called automatically, allows read operation calls");
            writer.WriteLine(" */");
            writer.WriteLine("public static function Read(operation:String, reader:PacketReader ):Object");
            writer.WriteLine("{");
            writer.WriteLine("switch (operation)");
            writer.WriteLine("{");
            builder.Model.Protocols
                .SelectMany(protocol => protocol.GetAllOperationsWithOutgoingPacket())
                .OrderBy(operation => operation.Key).ToList()
                .ForEach(operation =>
                {
                    writer.WriteLine("case {0}:", operation.Key);
                    writer.WriteLine("var packet{0}:{1} = new {1}();", operation.GetCleanKey(), operation.Outgoing.Name);
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
