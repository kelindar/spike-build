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
using Spike.Build.Compilers;
using Spike.Build.Runtime.Properties;

namespace Spike.Build.Client
{
    public class CSharpBuilder : ClientBuilder
    {

        /// <summary>
        /// Occurs during the build process.
        /// </summary>
        /// <param name="definitions">The protocol definitions to build.</param>
        /// /// <returns>The collection of build outputs.</returns>
        protected override BuildResult[] OnBuildLibrary()
        {
            // List of outputs
            var output = new List<BuildResult>();

            // Unzip manually created sources
            UnzipSource(SrcOutputPath, Spike.Build.CSharp.Properties.Resources.CSharpSrc);

            // Build the library
            base.OnBuildLibrary();

            // Continue build, global files
            CSharpReaderBuilder.GenerateCode(this);
            CSharpTcpChannelBuilder.GenerateCode(this);
            //CSharpUdpChannelBuilder.GenerateCode(this);

            // Create Source Package
            var package = GenerateSourcePackage(this);
            if (package != null)
                output.Add(package);

            // Create Unity Script
            var single = GenerateSingleScript(this);
            if (single != null)
                output.Add(single);

            // Compile everything
            if (!this.Compiler.OnlyBuild)
            {
                var compiled = CSharpCompiler.CompileAll(
                    Path.Combine(RootFolder, SrcOutputPath),
                    Path.Combine(RootFolder, BinOutputPath),
                    this);
                output.AddRange(compiled);
            }

            // Move output to directory
            CopyFilesRecursively(
                Path.Combine(RootFolder, BinOutputPath),
                this.Compiler.OutputDirectory);

            // Return the generated info
            return output.ToArray();
        }

        #region Properties
        private string fSrcOutputPath = @"Generated".AsPath();
        private string fBinOutputPath = @"Output".AsPath();

        /// <summary>
        /// Gets or sets the output path for generated source-code.
        /// </summary>
        [PropertyPath]
		public string SrcOutputPath
        {
            get { return fSrcOutputPath; }
            set { fSrcOutputPath = value.AsPath(); }
        }

        /// <summary>
        /// Gets or sets the output path for compiled library.
        /// </summary>
        [PropertyPath]
		public string BinOutputPath
        {
            get { return fBinOutputPath; }
            set { fBinOutputPath = value.AsPath(); }
        }

        /// <summary>
        /// Gets the user-friendly name of the language for which this client builder generates the
        /// code.
        /// </summary>
        [PropertyText]
		public override string Language
        {
            get { return "C#"; }
        }

        /// <summary>
        /// Gets the user-friendly description of the language for which this client builder generates the
        /// code.
        /// </summary>
        [PropertyText]
        public override string Description
        {
            get { return "C# is a multi-paradigm programming language encompassing strong typing, imperative, declarative, functional, generic, object-oriented, and component-oriented programming disciplines. It is one of the programming languages designed for the Common Language Infrastructure of .Net envoronment."; }
        }

        /// <summary>
        /// Gets the user-friendly description of the ideal usage the language for which this client builder generates the code.
        /// </summary>
        [PropertyText]
        public override string Usage
        {
            get { return ".Net, Mono, Unity3D, MonoTouch, MonoDroid"; }
        }
        #endregion

        #region IBuilder Members

        public override string GenerateCode(string inputFileContent)
        {
            // Declare builders
            var BuilderForElement = new CSharpElementBuilder();
            var BuilderForPacket = new CSharpPacketBuilder();

            // Begin code generation
            using (var writer = new CodeWriter())
            {
                var xml = Protocol.Deserialize(inputFileContent);
                if (xml != null)
                {
                    // Preprocessing: mutate by cloning the protocol and transforming it
                    var protocol = Model.Mutate(xml);


                    // 1st step: transform the model for C# Client
                    var elements = protocol.GetAllPackets()
                        .SelectMany(packet => packet.GetAllMembers()).ToList();
                    foreach (var element in elements)
                        InitializeElement(element);

                    // 2nd step, generate the code
                    {

                        // All Entities
                        protocol.GetAllComplexElementsDistinct()
                            .ForEach(element => BuilderForElement.GenerateCode(element, this));


                        // All Packets
                        protocol.GetAllPackets()
                            .ForEach(packet => BuilderForPacket.GenerateCode(packet, this));
                    }



                }
                return writer.ToString();
            }
        }


        /// <summary>
        /// Prepares the elements, altering the model
        /// </summary>
        private static void InitializeElement(Element element)
        {
            element.InternalType = element.Class;
            element.InternalName = element.Name;

            switch (element.Type)
            {
                case ElementType.Enum:
                {
                    element.InternalType = element.InternalElementType = element.Class = "Int32";
                    break;
                }

                case ElementType.DynamicType:
                {
                    element.InternalType = element.InternalElementType = element.Class = "Object";
                    break;
                }

                case ElementType.ComplexType:
                {
                    element.InternalType = element.InternalElementType = String.Format("{0}Entity", element.Class);
                    break;
                }

                case ElementType.ListOfByte:
                {
                    element.InternalType = element.Class = "byte[]";
                    element.InternalElementType = "byte";
                    break;
                }

                case ElementType.ListOfDynamicType:
                {
                    element.InternalType = element.Class = "List<Object>";
                    element.InternalElementType = "Object";
                    break;
                }

                case ElementType.ListOfComplexType:
                {
                    element.InternalType = String.Format("List<{0}Entity>", element.Class);
                    element.InternalElementType = String.Format("{0}Entity", element.Class);
                    break;
                }

                default:
                {
                    element.InternalElementType = element.Class;

                    // List of primitives
                    if (element.IsList && element.IsSimpleType)
                    {
                        var simpleType = element.Class.Replace("ListOf", "");
                        element.InternalType = String.Format("List<{0}>", simpleType);
                        element.InternalElementType = String.Format("{0}", simpleType);
                        element.Class = String.Format("List<{0}>", simpleType);
                    }
                    break;
                }
            }
        }


        #endregion

        #region GenerateHeader
        internal static void GenerateHeader(TextWriter writer)
        {
            writer.WriteLine("using System;");
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine("using System.Text;");
            writer.WriteLine("using System.IO;");
            writer.WriteLine("using System.Net;");
            writer.WriteLine();
        }
        #endregion

        #region Generate Source Package
        private static BuildResult GenerateSourcePackage(CSharpBuilder builder)
        {
            try
            {
                var PackageOutput = String.Format(@"{0}\Client.CSharp.Source\", Path.Combine(builder.RootFolder, builder.BinOutputPath)).AsPath();
                if (Directory.Exists(PackageOutput))
                    Directory.Delete(PackageOutput, true);
                Directory.CreateDirectory(PackageOutput);

                var zip = new ICSharpCode.SharpZipLib.Zip.FastZip();
                var src = Path.Combine(builder.RootFolder, builder.SrcOutputPath);
                var dst = Path.Combine(PackageOutput, "spike-sdk.csharp.src.zip");


                zip.CreateEmptyDirectories = true;
                zip.CreateZip(dst, src, true, "");

                // Our output
                return new BuildResult(builder, "C# 2.0 Source Code Package (.zip)", dst.AsPath());
            }
            catch (Exception ex)
            {
                builder.OnError(1, ex.Message, 0, 0);
                return null;
            }

        }
        #endregion

        #region Generate Single Script
        private static BuildResult GenerateSingleScript(CSharpBuilder builder)
        {
            var UnityOutput = String.Format(@"{0}\Client.CSharp.Script\", Path.Combine(builder.RootFolder,builder.BinOutputPath)).AsPath();
            if (Directory.Exists(UnityOutput))
                Directory.Delete(UnityOutput, true);
            Directory.CreateDirectory(UnityOutput);
            UnityOutput += "spike-sdk.csharp.src.cs";

            var bigScript = builder.Sources
                .Where(info => !info.FileName.Contains("AssemblyInfo.cs"))
                .Select(source => source.Source)
                .Select(source => source.Split(new string[] { Environment.NewLine }, StringSplitOptions.None))
                .ToList()
                .Select(file =>
                {
                    var filteredSource = file
                    .Where(line => !(line.TrimStart().StartsWith("using ") && !line.Contains("(")))
                    .Where(line => !line.TrimStart().StartsWith("namespace "))
                    .Aggregate((a, b) => a + Environment.NewLine + b);

                    filteredSource = filteredSource.Remove(filteredSource.IndexOf('{'), 1);
                    filteredSource = filteredSource.Remove(filteredSource.LastIndexOf('}'), 1);

                    return filteredSource;
                })
                .Aggregate((a, b) => a + Environment.NewLine + b);

            using (var writer = new CodeWriter())
            {
                writer.WriteLine(@"// ------------------------------------------------------------------------------");
                writer.WriteLine(@"//  <auto-generated>");
                writer.WriteLine(@"//     This code was generated by a tool (Spike Build).");
                writer.WriteLine(@"//     Runtime Version: " + Environment.Version.ToString());
                writer.WriteLine(@"//");
                writer.WriteLine(@"//     Changes to this file may cause incorrect behavior and will be lost if");
                writer.WriteLine(@"//     the code is regenerated.");
                writer.WriteLine(@"//  </auto-generated>");
                writer.WriteLine(@"//------------------------------------------------------------------------------");
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine("using System.Text;");
                writer.WriteLine("using System.IO;");
                writer.WriteLine("using System.Runtime.Serialization;");
                writer.WriteLine("using System.Threading;");
                writer.WriteLine("using System.Security.Cryptography;");
				writer.WriteLine("using System.Net;");
                writer.WriteLine("using System.Net.Sockets;");
                writer.WriteLine("using System.IO.Compression;");
                writer.WriteLine();
                writer.WriteLine("namespace Spike.Network"); // Begin package
                writer.WriteLine("{");
                writer.WriteLine(bigScript);
                writer.WriteLine("}");
                File.WriteAllText(UnityOutput, writer.ToString());
            }

            return new BuildResult(builder, "C# 2.0 Source Code in a Single File (.cs)", UnityOutput);
        }
        #endregion


    }
}
