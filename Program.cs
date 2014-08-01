/************************************************************************
*
* Copyright (C) 2009-2014 Misakai Ltd
*
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
* 
* You should have received a copy of the GNU General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
* 
*************************************************************************/

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Net.Http;

using Spike.Build.WinRT;
using Spike.Build.Xamarin;
using Spike.Build.CSharp;
using Spike.Build.Java;


namespace Spike.Build
{
    internal static class Program
    {
        static internal void Exit(string message = null)
        {
            if (message != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Error : ");
                Console.WriteLine(message);
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            PromptUsage();
            Console.Read();
            Environment.Exit(-1);
        }

        static private Dictionary<string, IBuilder> Builders = new Dictionary<string, IBuilder>(StringComparer.CurrentCultureIgnoreCase) {
            { "Java", new JavaBuilder() },
            { "CSharp", new CSharpBuilder() },
            { "WinRT", new WinRTBuilder() },
            { "Xamarin", new XamarinBuilder() }
        };

        static void Main(string[] args)
        {
            try
            {
                var currentAssembly = Assembly.GetExecutingAssembly();
                Console.WriteLine("{0}, Version {1}", currentAssembly.GetCustomAttribute<AssemblyTitleAttribute>().Title, currentAssembly.GetName().Version.ToString());
                Console.WriteLine(currentAssembly.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright);
                Console.WriteLine();



                // Parse arguments
                if (args.Length == 0)
                    Program.Exit();

                if (args.Length < 2)
                    Program.Exit("You must define <source> AND <build>");

                // Get Model
                var model = new Model();
                var modelFile = args[0].TrimEnd('/');

                if (modelFile.EndsWith("/spml/all", StringComparison.CurrentCultureIgnoreCase))
                {
                    using (var client = new HttpClient()) {
                        var result = client.GetAsync(modelFile).Result;
                        if (result.IsSuccessStatusCode) {
                            var protocols = result.Content.ReadAsStringAsync().Result.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                            if (protocols.Length > 0) {
                                var baseUrl = modelFile.Substring(0, modelFile.Length - 4);
                                foreach (var protocol in protocols) {
                                    Console.WriteLine(protocol);
                                    model.Load(string.Format("{0}?file={1}", baseUrl, protocol));
                                }
                            }
                            else
                                Program.Exit("No Protocols");
                        }
                        else
                            Program.Exit("Host unreachable");
                    }
                }
                else
                    model.Load(modelFile);
                

                var separators = new char[] { '-', ':' };
                for (var index = 1; index < args.Length; index++)
                {
                    var buildArguments = args[index].Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    if (buildArguments.Length <= 0 || buildArguments.Length > 2)
                        Program.Exit("Syntax error");

                    if (Builders.TryGetValue(buildArguments[0], out var builder))
                    {
                        if (buildArguments.Length == 2)
                            builder.Build(model, buildArguments[1]);
                        else
                            builder.Build(model);
                    }
                    else if (string.Compare(buildArguments[0],"mode") ==0) {
                    }
                    else
                        Program.Exit("Unknown parameter");
                }

            }
#if DEBUG
            catch (Exception e)
            {
                //Popup in visual studio if attached 
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();

                Program.Exit(string.Format("Unknown exception : {0}", e.StackTrace));
            }
#else
            catch (Exception)
            {
                Program.Exit("An unknown error occurred");  
            }
#endif
        }

        private static void PromptUsage()
        {
            Console.WriteLine("Usage: ");
            Console.WriteLine(" Spike.Build <source> <build>");
            Console.WriteLine();
            Console.WriteLine(" build: ");
            foreach (var key in Builders.Keys)
                Console.WriteLine("  -{0}[:output_path]", key);
            Console.WriteLine();
            Console.WriteLine(" source could be either: ");
            Console.WriteLine("  URL ( ex: http://www.spike-engine.com/spml?file=MyChatProtocol or http://www.spike-engine.com/spml/all )");
            Console.WriteLine("  File ( ex: test.spml )");
        }
    }
}
