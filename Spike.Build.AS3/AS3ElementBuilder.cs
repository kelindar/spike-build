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
