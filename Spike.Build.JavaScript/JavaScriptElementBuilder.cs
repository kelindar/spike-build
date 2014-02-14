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


namespace Spike.Build.Client.JavaScript
{
    public class JavaScriptElementBuilder : ISubClientBuilder<Element, JavaScriptBuilder>
    {
        public void GenerateCode(Element element, JavaScriptBuilder builder)
        {
            using (var writer = new CodeWriter())
            {
                GeneratePartialEntity(element, writer);
                builder.AddSourceFile(builder.SrcOutputPath, String.Format(@"{0}.js", element.InternalElementType), writer);
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

            writer.WriteLine("function {0}()", element.InternalElementType); // Begin class
            writer.WriteLine("{");
            {
                // Generate fields
                element.GetMembers().ForEach(prop =>
                    {
                        prop.GenerateProperty(writer);
                    });

                // Read/Write methods
                element.GenerateWriteMethod(writer);
                element.GenerateReadMethod(writer);
            }

            writer.WriteLine("};"); // End class

        }

        #endregion

    }
}
