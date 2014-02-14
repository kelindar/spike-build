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
using System.Reflection;
using Microsoft.CSharp;
using System.IO;
using System.CodeDom.Compiler;
using Spike.Build.Client;
using Spike.Build.Runtime.Properties;
using System.Security.Policy;

namespace Spike.Build.Compilers
{
    public static class CSharpCompiler
    {
        public static string BinaryFilename = "Spike.Sdk.dll";
        public static string CompilerOptions = "/target:library /optimize";
        public static string[] Versions = new string[]
        {
            "v2.0",
            "v3.5",
            "v4.0"
        };

        private static IEnumerable<string> GetAllAssemblies(string compilerVersion)
        {
            var windir = Environment.GetEnvironmentVariable("SystemRoot");
            var assemblies = new List<string>();

            // Predefinded referenced assemblies
            if (compilerVersion == "v2.0" || compilerVersion == "v3.5")
            {
                //assemblies.Add(windir + @"\Microsoft.NET\Framework\v2.0.50727\mscorlib.dll");
                assemblies.Add(windir + @"\Microsoft.NET\Framework\v2.0.50727\System.dll".AsPath());
            }
            if (compilerVersion == "v4.0")
            {
                //assemblies.Add(windir + @"\Microsoft.NET\Framework\v4.0.30319\mscorlib.dll");
                assemblies.Add(windir + @"\Microsoft.NET\Framework\v4.0.30319\System.dll".AsPath());
            }

            return assemblies.Distinct();
        }

        public static BuildResult[] CompileAll(string sourceToCompilePath, string outputPath, ClientBuilder builder)
        {
            var output = new List<BuildResult>();
            foreach (var version in Versions)
            {
                try
                {
                    // Compile
                    var buildInfo = Compile(builder, version, sourceToCompilePath, outputPath);
                    if (buildInfo != null)
                        output.Add(buildInfo);
                }
                catch (Exception ex)
                {
                    BuilderBase.Out.WriteLine(ConsoleColor.Red, String.Format("Could not compile {0} :", version));
                    BuilderBase.Out.WriteLine(ConsoleColor.DarkRed, " {0}", ex.Message);
                }
            }

            // Return the output
            return output.ToArray();
        }

        public static BuildResult Compile(ClientBuilder builder, string compilerVersion, string sourceToCompilePath, string outputPath)
        {
            outputPath = String.Format(@"{0}\Client.CSharp.{1}\", outputPath, compilerVersion).AsPath();
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);
            
            using (var csp = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", compilerVersion } }))
            {
                var lines = builder.Sources.Select(source => source.Source.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).Count()).Sum();
                var batch = builder.Sources.Select(source => Path.GetFullPath(Path.Combine(sourceToCompilePath, source.FileName))).ToArray();

                Console.WriteLine("Compiling C# Network Library for Framework {0} ...", compilerVersion);
#pragma warning disable 618
                var cc = csp.CreateCompiler();
#pragma warning restore  618
                var parameters = new CompilerParameters()
                {
                    GenerateExecutable = false,
                    GenerateInMemory = false,
                    OutputAssembly = Path.Combine(outputPath, BinaryFilename),
                    CompilerOptions = CompilerOptions
                };
                var assemblies = GetAllAssemblies(compilerVersion);
                foreach (var assembly in assemblies)
                {
                    //Console.WriteLine(assembly.Location);
                    parameters.ReferencedAssemblies.Add(assembly);
                }

                var results = cc.CompileAssemblyFromFileBatch(parameters, batch);
                if (results.Errors.HasErrors)
                {
                    BuilderBase.Out.WriteLine("Compiler Errors:{0}", Environment.NewLine);
                    //var errors = new StringBuilder("Compiler Errors :\r\n");
                    foreach (CompilerError error in results.Errors)
                    {
                        //var text = File.ReadAllLines(error.FileName);
                        var text = builder.Sources.ElementAtOrDefault(batch.Select(item => item.ToLower()).ToList().IndexOf(error.FileName.ToLower()));
                        var code = text != null ? text.Source.Split(new string[] { Environment.NewLine }, StringSplitOptions.None) : null;
                        var file = text != null ? text.FileName : "UNKNOWN";

                        if (code != null && error.Line > 0 && error.Line <= code.Length)
                        {
                            builder.OnError(4, error.ErrorText, (uint)error.Line, (uint)error.Column);

                            BuilderBase.Out.WriteLine();
                            BuilderBase.Out.WriteLine(ConsoleColor.Red, "Error:");
                            BuilderBase.Out.WriteLine(ConsoleColor.DarkRed, " {0} in {1}", error.ErrorText, file);

                            var index = error.Line - 1;

                            if (index - 2 >= 0) BuilderBase.Out.WriteLine(ConsoleColor.DarkYellow, String.Format("Line {0}: {1}", error.Line - 2, code[index - 2]));
                            if (index - 1 >= 0) BuilderBase.Out.WriteLine(ConsoleColor.DarkYellow, String.Format("Line {0}: {1}", error.Line - 1, code[index - 1]));

                            BuilderBase.Out.WriteLine(ConsoleColor.DarkRed, "Line {0}: {1}", error.Line, code[index]);

                            if (index + 1 < code.Length) BuilderBase.Out.WriteLine(ConsoleColor.DarkYellow, String.Format("Line {0}: {1}", error.Line + 1, code[index + 1]));
                            if (index + 2 < code.Length) BuilderBase.Out.WriteLine(ConsoleColor.DarkYellow, String.Format("Line {0}: {1}", error.Line + 2, code[index + 2]));

                        }
                        else
                        {
                            BuilderBase.Out.WriteLine(ConsoleColor.Red, "Line {2},{3}{1}Error: {0} in {4}", error.ErrorText, Environment.NewLine, error.Line, error.Column, file);
                        }

                        //errors.AppendFormat("Line {0},{1}\t: {2}\n", error.Line, error.Column, error.ErrorText);
                    }
                    BuilderBase.Out.WriteLine();
                    throw new CodeCompilationException("Protocol compilation error has occured");
                }
                else
                {
                    return new BuildResult(builder, "Precompiled .NET Assembly for Microsoft or Mono Framework " + compilerVersion + " (.dll)", outputPath);
                }

            }
        }


    }
}
