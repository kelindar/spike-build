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
using Spike.Build.CSharp5;
using Spike.Build.Java;


namespace Spike.Build
{
    internal static class Program
    {

        internal static bool Verbose = false;
        internal static IBuilder Builder = null; //--platform -p
        internal static List<string> Sources { get; } = new List<string>(); // --input -i
        internal static string Destination = null; // --output -o 
        internal static string Format = null; // --format -f 
        internal static string Namespace = null; // --namespace -n


        static internal void ShowUsageAndExit(string error = null)
        {
            if (error != null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Error : ");
                Console.WriteLine(error);
                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            PromptUsage();
            Console.Read();
            Environment.Exit(-1);
        }

        static private Dictionary<string, IBuilder> Builders = new Dictionary<string, IBuilder>(StringComparer.CurrentCultureIgnoreCase) {
            { "CSharp5", new CSharp5Builder() },
            { "Java", new JavaBuilder() },
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

                //If arguments parsing become more complex Consider using NDesk.Option

                if (args.Length == 0)
                    ShowUsageAndExit();

                for (int index = 0; index < args.Length;)
                {
                    var command = args[index++].ToLower();
                    switch (command)
                    {
                        case "-v":
                        case "--verbose":
                            if (index < args.Length && args[index][0] != '-') // no "-v blabla" 
                                ShowUsageAndExit("--verbose take no parameters");

                            Verbose = true;
                            break;
                        case "-h":
                        case "--help":
                            ShowUsageAndExit();
                            break;
                        case "-p":
                        case "--platform":
                            if (index >= args.Length || args[index][0] == '-') //no -p -i
                                ShowUsageAndExit("You must define a platform after --platform");

                            var builderName = args[index++];
                            if (Builder != null ||  //no -p java -i file.spml -p xamarin
                                (index < args.Length && args[index][0] != '-')) //no -p java xamarin
                                ShowUsageAndExit("Only one builder");

                            if (!Builders.TryGetValue(builderName, out Builder)) //no -p unknown
                                ShowUsageAndExit("Unknown platform");
                            break;
                        case "-i":
                        case "--input":
                            if (index >= args.Length || args[index][0] == '-')
                                ShowUsageAndExit("You must define a input");

                            do
                                Sources.Add(args[index++]);
                            while (index < args.Length && args[index][0] != '-');

                            break;
                        case "-o":
                        case "--output":
                            if (index >= args.Length || args[index][0] == '-')
                                ShowUsageAndExit("You must define a output");

                            var destination = args[index++];
                            if (Destination != null ||  
                                (index < args.Length && args[index][0] != '-')) 
                                ShowUsageAndExit("Only one output");

                            Destination = destination;
                            break;
                        case "-f":
                        case "--format":
                            if (index >= args.Length || args[index][0] == '-')
                                ShowUsageAndExit("You must define a format");

                            var format = args[index++];
                            if (Format != null ||  
                                (index < args.Length && args[index][0] != '-')) 
                                ShowUsageAndExit("Only one format");

                            Format = format;
                            break;
                        case "-n":
                        case "--namespace":
                            if (index >= args.Length || args[index][0] == '-')
                                ShowUsageAndExit("You must define a namespace");

                            var ns = args[index++];
                            if (Namespace != null ||  //already set by previex --platform
                                (index < args.Length && args[index][0] != '-')) //if the next argument is not a command
                                ShowUsageAndExit("Only one namespace");

                            Namespace = ns;
                            break;
                    }

                }
                
                if (Builder == null)
                    ShowUsageAndExit("You must define a platform");

                if (Sources.Count <= 0)
                    ShowUsageAndExit("You must define a source");

                
                // Get Model
                var model = new Model();

                foreach (var source in Sources)
                {
                    var modelFile = source.TrimEnd('/');

                    if (modelFile.EndsWith("/spml/all", StringComparison.CurrentCultureIgnoreCase))
                    {
                        using (var client = new HttpClient())
                        {
                            var result = client.GetAsync(modelFile).Result;
                            if (result.IsSuccessStatusCode)
                            {
                                var protocols = result.Content.ReadAsStringAsync().Result.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                                if (protocols.Length > 0)
                                {
                                    var baseUrl = modelFile.Substring(0, modelFile.Length - 4);
                                    foreach (var protocol in protocols)
                                    {
                                        Console.WriteLine(protocol);
                                        model.Load(string.Format("{0}?file={1}", baseUrl, protocol));
                                    }
                                }
                                else
                                    ShowUsageAndExit("No Protocols");
                            }
                            else
                                ShowUsageAndExit("Host unreachable");
                        }
                    }
                    else
                        model.Load(modelFile);
                }

                Builder.Build(model, Destination);
            }
#if DEBUG
            catch (Exception e)
            {
                //Popup in visual studio if attached 
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();

                ShowUsageAndExit(string.Format("Unknown exception : {0}", e.StackTrace));
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
