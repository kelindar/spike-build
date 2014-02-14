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
using System.Threading;
using Spike.Build.Client;

namespace Spike.Build.Compilers
{
    public class AS3Compiler
    {
		/// <summary>
		/// Gets or sets the client builder.
		/// </summary>
        private AS3Builder Builder;

        private const string SwcName = "spike-sdk.as3.swc";
		
		/// <summary>
		/// Gets or sets the list of Flex SDK directories.
		/// </summary>
        private List<string> SdkDirectories = new List<string>()
        {
            @"C:\Program Files (x86)\Adobe\Adobe Flash Builder 4\sdks\4.1.0\",
            @"C:\Program Files (x86)\Adobe\Adobe Flash Builder 4\sdks\4.0.0\",
            @"C:\Program Files\Adobe\Adobe Flash Builder 4\sdks\4.0.0\",
            @"C:\Program Files\Flex\", // Probably custom defined one
            @"C:\Program Files (x86)\Adobe\Flex Builder 3\sdks\3.2.0\",
            @"C:\Program Files\Adobe\Flex Builder 3\sdks\3.2.0\",
            @"G:\Infographie\Creative Suite 5\Adobe Flash Builder 4\sdks\4.0.0\",
            
        };
		
		/// <summary>
		/// Initializes a new instance of the <see cref="Spike.Build.Compilers.AS3Compiler"/> class.
		/// </summary>
		/// <param name='builder'>
		/// The client builder.
		/// </param>
		public AS3Compiler(AS3Builder builder)
		{
			if(builder == null)
				throw new ArgumentNullException("builder");
			
			this.Builder = builder;
		}

        #region AS3 Compile SWC
        internal void Compile(string SrcToCompilePath, string OutputPath)
        {
            var sdk = ResolveSdk();
            if (sdk != null)
            {
                // Get version
                var compilerVersion = String.Format(@"{0}bin\compc.exe", sdk);
                var version = CommandLine.Run(compilerVersion, "-version").ToLower().Replace("version", "").Trim().Substring(0, 3);

                // Use Flex Component Compiler
                // Documentation: http://livedocs.adobe.com/flex/3/html/help.html?content=compilers_14.html#157203

                var compc = String.Format(@"""{0}bin\compc.exe""", sdk);
                var flags = String.Format(" -optimize=true -strict=true -use-network=true");
                var sourcePath = String.Format(@"  -source-path ""{0}""", Path.GetFullPath(SrcToCompilePath));
                var includeSources = String.Format(@"  -include-sources ""{0}""", Path.GetFullPath(SrcToCompilePath));
                var libPath = String.Format(@"  -compiler.library-path ""{0}frameworks\libs""", sdk);
                var bundles = String.Format(@"  -resource-bundle-list ""{0}frameworks\projects\framework\bundles.properties""", sdk);
                var output = String.Format(@"  -output ""{0}\Client.AS3.v{1}\{2}""", Path.GetFullPath(OutputPath), version, SwcName);

                // Construct the whole commandvar sdk = SdkDirectories.Where(dir => Directory.Exists(dir)).FirstOrDefault();
                var command = compc + flags + sourcePath + includeSources + libPath + output + bundles;

                // Correct dashes
                command = /*Kernel.Unix ? command.Replace(@"\", @"/") :*/ command.Replace(@"/", @"\");

                // Save the command for debug
                //File.WriteAllText("compileswc.bat", command + Environment.NewLine + "pause");

                // Compile asyncronously
                Console.WriteLine("Compiling ActionScript Network Library...");
                ExecuteCommandSync(command);
                //ExecuteCommandAsync(command);

                /* Weird, but seems like the direction of dashes matters :/
                 * 
                 * "C:\Program Files (x86)\Adobe\Flex Builder 3\sdks\3.2.0\bin\compc" 
                 * -source-path "D:\Workspace\Personal\Projects_.Net\SpaceX/Client.NetLib/src" 
                 * -include-sources "D:\Workspace\Personal\Projects_.Net\SpaceX/Client.NetLib/src" 
                 * -compiler.library-path "C:\Program Files (x86)\Adobe\Flex Builder 3\sdks/3.2.0/frameworks/libs" 
                 * -output "D:\Workspace\Personal\Projects_.Net\SpaceX/Client.NetLib/libNet.swc"*/
            }
            else
            {
                this.Builder.OnError(3, "Error: Flex SDK not found, please check the path configuration", 0, 0);
                //BaseBuilder.Out.WriteLine(ConsoleColor.Red, "Error: Flex SDK not found, please check the path configuration");
            }

        }

        public void ExecuteCommandAsync(string command)
        {
            try
            {
                //Asynchronously start the Thread to process the Execute command request.
                Thread objThread = new Thread(new ParameterizedThreadStart(ExecuteCommandSync));

                //Make the thread as background thread.
                objThread.IsBackground = true;

                //Set the Priority of the thread.
                objThread.Priority = ThreadPriority.AboveNormal;

                //Start the thread.
                objThread.Start(command);
            }
            catch { }
        }


        public void ExecuteCommandSync(object command)
        {
            try
            {
                CommandLine.Run(command.ToString());
                
            }
            catch (CommandLine.CommandLineException ex)
            {
                var lines = ex.Message.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    var split = line.Contains("Error:") ? line.Split(new string[] { "Error:" }, StringSplitOptions.None) : null;
                    if (split != null && split.Length == 2)
                    {
                        BuilderBase.Out.Write(ConsoleColor.Red, "Error:");
                        BuilderBase.Out.WriteLine(ConsoleColor.DarkRed, " {0}", split[1]);
                        BuilderBase.Out.WriteLine("File: {0}", split[0]);
                    }
                    else
                    {
                        BuilderBase.Out.WriteLine(line);
                    }
                }
                throw new CodeCompilationException("Protocol compilation error has occured");
            }
        }



        #endregion

		/// <summary>
		/// Resolves the sdk path.
		/// </summary>
		private string ResolveSdk()
		{
			if(!ContainsCompiler(this.Builder.FlexSdkPath))
				return this.Builder.FlexSdkPath;
			
			return SdkDirectories
				.Where(dir => Directory.Exists(dir))
				.FirstOrDefault();	
		}
		
		/// <summary>
		/// Checks whether the compiler exists in the provided directory.
		/// </summary>
		private bool ContainsCompiler(string directory)
		{
			if(String.IsNullOrWhiteSpace(this.Builder.FlexSdkPath))
				return false;
			if(!Directory.Exists(directory))
				return false;
			
			var compc = new string[]{"compc", "compc.exe"};
			return compc
				.Select( c => Path.Combine(directory, c))
				.Any(c => File.Exists(c));
		}

    }
}

