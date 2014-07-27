#region Copyright(c) 2009-2014 Misakai Ltd.
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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Spike.Build.WinRT;
using Spike.Build.MonoDroid;
using Spike.Build.MonoTouch;
using System.Diagnostics;

namespace Spike.Build
{
    internal static class Program
    {
        static private Dictionary<string, IBuilder> Builders = new Dictionary<string, IBuilder>(StringComparer.CurrentCultureIgnoreCase) {
            { "WinRT", new WinRTBuilder() },
            { "MonoTouch", new MonoTouchBuilder()},
            { "MonoDroid", new MonoDroidBuilder()}
        };


        static void Main(string[] args)
        {
            // Parse arguments
            if (args.Length < 2)
            {
                PromptUsage();
                return;
            }

            // Get Model
            var model = Model.GetFrom(args[0]);
            if (model == null)
            {
                PromptUsage();
                return;
            }

            var separators = new char[] { '-', ':' };
            for (var index = 1; index < args.Length; index++) {
                var buildArguments = args[index].Split(separators,StringSplitOptions.RemoveEmptyEntries);

                if (buildArguments.Length <= 0 || buildArguments.Length > 2) {
                    PromptUsage();
                    return;
                }
                
                if(Builders.TryGetValue(buildArguments[0], out var builder))
                {
                    if (buildArguments.Length == 2)
                        builder.Build(model, buildArguments[1]);
                    else
                        builder.Build(model);
                }
                else
                {
                    PromptUsage();
                    return;
                }                
            }
            Console.Read();
        }

      

        private static void PromptUsage()
        {
            var currentAssembly = Assembly.GetExecutingAssembly();

            Console.WriteLine("{0}, Version {1}", currentAssembly.GetCustomAttribute<AssemblyTitleAttribute>().Title, currentAssembly.GetName().Version.ToString());
            Console.WriteLine(currentAssembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright);
            Console.WriteLine();
            Console.WriteLine(" Usage: ");
            Console.WriteLine(" Spike.Build <source> <build>");
            Console.WriteLine();
            Console.WriteLine(" build: ");
            foreach (var key in Builders.Keys)
                Console.WriteLine(" {0}[:output path]", key);
            Console.WriteLine();
            Console.WriteLine(" source: ");
            Console.WriteLine(" Input could be either:");
            Console.WriteLine(" An URL (ex: http://127.0.0.1:8002/spml/all )");
            Console.WriteLine(" Or A file");
        }        
    }
}
