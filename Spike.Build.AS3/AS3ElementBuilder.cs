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
using Spike.Build;
using Spike.Build.Client.AS3;

namespace Spike.Build.Client
{
    public class AS3ElementBuilder : ISubClientBuilder<Element, AS3Builder>
    {
        public void GenerateCode(Element element, AS3Builder parent)
        {
            using (var writer = new CodeWriter())
            {
                GeneratePartialEntity(element, writer);
                parent.AddSourceFile(parent.PacketsPath, String.Format(@"{0}.as", element.InternalElementType), writer);
            }
        }

        #region GeneratePartialEntity

        internal static List<string> PartialEntitiesGenerated = new List<string>();
        internal static void GeneratePartialEntity(Element element, TextWriter writer)
        {
            // Check whether we've already generated the entity (in case it's used in different packets)
            if (PartialEntitiesGenerated.Contains(element.InternalElementType))
                return;
            PartialEntitiesGenerated.Add(element.InternalElementType);


            writer.WriteLine("package network.packets"); // Begin package
            writer.WriteLine("{");
            AS3Builder.GenerateHeader(writer);
            writer.WriteLine("public class {0} implements IEntity", element.InternalElementType); // Begin class
            writer.WriteLine("{");
            {
                // Generate constructors
                GeneratePartialEntityConstructors(element, writer);

                // Generate fields
                element.GetMembers().ForEach(prop =>
                    {
                        prop.GenerateProperty(writer);
                    });

                // Read/Write methods
                element.GenerateWriteMethod(writer);
                element.GenerateReadMethod(writer);
            }

            writer.WriteLine("}"); // End class
            writer.WriteLine("}"); // End package 


        }

        #region GeneratePartialEntityConstructors

        internal static void GeneratePartialEntityConstructors(Element element, TextWriter writer)
        {
            // Default constructor, need to instantiate the partial entities within
            writer.WriteLine("public function {0}()", element.InternalElementType);
            writer.WriteLine("{");

            writer.WriteLine("}");
            writer.WriteLine();

        }

        #endregion

        #endregion

    }
}
