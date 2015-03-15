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
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Net.Http;

using Spike.Build.WinRT;
using Spike.Build.Xamarin;
using Spike.Build.CSharp5;
using Spike.Build.Java;
using System.IO;
using Spike.Build.JavaScript;
using Spike.Build.Go;
using Spike.Build.AS3;


namespace Spike.Build
{
    
    internal static class Program
    {
        private static bool Verbose = false;
        private static IBuilder Builder = null; //--platform -p
        private static List<string> Sources = new List<string>(); // --input -i
        private static string Destination = null; // --output -o 
        private static string Format = null; // --format -f 
        private static string Namespace = null; // --namespace -n
        private static Model Model = null; 


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

        static private Dictionary<string, IBuilder> Builders = new Dictionary<string, IBuilder>(StringComparer.CurrentCultureIgnoreCase) 
        {
            { "AS3" , new AS3Builder() },
            { "CSharp5", new CSharp5Builder() },
            { "Java", new JavaBuilder() },
            { "WinRT", new WinRTBuilder() },
            { "Xamarin", new XamarinBuilder() },
            { "JavaScript", new JavaScriptBuilder() },
            { "Go", new GoBuilder() }
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
                                ShowUsageAndExit("--verbose shouldn't have any parameters");

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
                                ShowUsageAndExit("Input must be specified.");

                            do
                                Sources.Add(args[index++]);
                            while (index < args.Length && args[index][0] != '-');

                            break;
                        case "-o":
                        case "--output":
                            if (index >= args.Length || args[index][0] == '-')
                                ShowUsageAndExit("Output must be specified.");

                            var destination = args[index++];
                            if (Destination != null ||  
                                (index < args.Length && args[index][0] != '-')) 
                                ShowUsageAndExit("Only one output");

                            Destination = destination;
                            break;
                        case "-f":
                        case "--format":
                            if (index >= args.Length || args[index][0] == '-')
                                ShowUsageAndExit("Format must be specified.");

                            var format = args[index++];
                            if (Format != null ||  
                                (index < args.Length && args[index][0] != '-')) 
                                ShowUsageAndExit("Only one format");

                            Format = format;
                            break;
                        case "-n":
                        case "--namespace":
                            if (index >= args.Length || args[index][0] == '-')
                                ShowUsageAndExit("Namespace must be specified.");

                            var ns = args[index++];
                            if (Namespace != null ||  //already set by previex --platform
                                (index < args.Length && args[index][0] != '-')) //if the next argument is not a command
                                ShowUsageAndExit("Only one namespace");

                            Namespace = ns;
                            break;
                    }

                }
                
                if (Builder == null)
                    ShowUsageAndExit("Platform must be specified.");

                if (Sources.Count <= 0)
                    ShowUsageAndExit("Source must be specified.");


                // Get Model
                Model = new Model();

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
                                        Console.WriteLine("Building Protocol: " + protocol + "...");
                                        Model.Load(string.Format("{0}?file={1}", baseUrl, protocol));
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
                        Model.Load(modelFile);
                }

                Builder.Build(Model,Destination,Format);
            }
            catch (Exception ex)
            {
#if DEBUG
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
#endif

                if (ex is ProtocolMalformedException)
                    Program.ShowUsageAndExit(ex.Message);
                if (ex is FileNotFoundException)
                    Program.ShowUsageAndExit(ex.Message);

                Program.ShowUsageAndExit(string.Format("An unexpected error has occured : {0}", ex.StackTrace));
            }
        }

        private static void PromptUsage()
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            Console.WriteLine("Usage: SBuild [OPTIONS]");
            Console.WriteLine(currentAssembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description);
            Console.WriteLine();
            Console.WriteLine("Options: ");

            Console.WriteLine("  -h, --help             Prints out this usage help documentation");
            Console.WriteLine();

            Console.WriteLine("  -i, --input <source>   Specifies the input protocol definition location as:");
            Console.WriteLine("                         - (URL) ex: http://127.0.0.1:8002/spml/MyProtocol or");
            Console.WriteLine("                                     http://127.0.0.1:8002/spml/all");
            Console.WriteLine("                         - (FILE) ex: MyProtocol.spml");
            Console.WriteLine();

            Console.WriteLine("  -p, --platform <type>  Sets the output platform for which the source code");
            Console.WriteLine("                         should be generated. Available generators:");
            foreach (var builder in Builders.Keys)
                Console.WriteLine("                         - \"" + builder.ToLower() + "\": for " + builder + " code generator");
            Console.WriteLine();

            Console.WriteLine("  -o, --output <path>    Specify the output path (directory) where the");
            Console.WriteLine("                         generated source code should be placed");
            Console.WriteLine();

            Console.WriteLine("  -f, --format <single>  Specifies the format of the code generation,");
            Console.WriteLine("                         currently only \"single\" is available for ");
            Console.WriteLine("                         JavaScript builder");
            //Console.WriteLine("\t-n, --namespace");
            //Console.WriteLine("\t-v, --verbose");

        }
    }
}
