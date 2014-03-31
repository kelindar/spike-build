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
using Spike.Build.Runtime.Properties;

namespace Spike.Build.Client
{
    public class JavaBuilder : ClientBuilder
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
            UnzipSource(SrcOutputPath, Spike.Build.Java.Properties.Resources.JavaSrc);

            // Build the library
            base.OnBuildLibrary();

            // Continue build, global files
            JavaTcpChannelBuilder.GenerateCode(this);

            // Create Source Package
            var package = GenerateSourcePackage(this);
            if (package != null)
                output.Add(package);

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
        private string fPacketsPath = @"Generated\com\misakai\spike\network\packets".AsPath();
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
        /// Gets the output source packets path.
        /// </summary>
        internal string PacketsPath
        {
            get { return fPacketsPath; }
        }

        /// <summary>
        /// Gets the user-friendly name of the language for which this client builder generates the
        /// code.
        /// </summary>
        [PropertyText]
        public override string Language
        {
            get { return "Java"; }
        }

        /// <summary>
        /// Gets the user-friendly description of the language for which this client builder generates the
        /// code.
        /// </summary>
        [PropertyText]
        public override string Description
        {
            get { return @"Java is a computer programming language that is concurrent, class-based, object-oriented, and specifically designed to have as few implementation dependencies as possible. It is intended to let application developers ""write once, run anywhere"" (WORA), meaning that code that runs on one platform does not need to be recompiled to run on another."; }
        }

        /// <summary>
        /// Gets the user-friendly description of the ideal usage the language for which this client builder generates the code.
        /// </summary>
        [PropertyText]
        public override string Usage
        {
            get { return "Jdk, Android"; }
        }
        #endregion

        #region IBuilder Members
        public override string GenerateCode(string inputFileContent)
        {
            // Declare builders
            var BuilderForPacket = new JavaPacketBuilder();

            // Begin code generation
            var xml = Protocol.Deserialize(inputFileContent);
            if (xml != null)
            {
                // Preprocessing: mutate by cloning the protocol and transforming it
                var protocol = Model.Mutate(xml);
                
                // Generate All Packets
                foreach (var packet in protocol.GetAllPackets())
                {
                    BuilderForPacket.GenerateCode(packet, this);
                }
            }
            return String.Empty;
        }
        #endregion

        #region Generate Source Package
        private static BuildResult GenerateSourcePackage(JavaBuilder builder)
        {
            try
            {
                var PackageOutput = String.Format(@"{0}\Client.Java.Source\", Path.Combine(builder.RootFolder, builder.BinOutputPath)).AsPath();
                if (Directory.Exists(PackageOutput))
                    Directory.Delete(PackageOutput, true);
                Directory.CreateDirectory(PackageOutput);

                var zip = new ICSharpCode.SharpZipLib.Zip.FastZip();
                var src = Path.Combine(builder.RootFolder, builder.SrcOutputPath);
                var dst = Path.Combine(PackageOutput, "spike-sdk.java.src.zip");


                zip.CreateEmptyDirectories = true;
                zip.CreateZip(dst, src, true, "");

                // Our output
                return new BuildResult(builder, "Java Source Code Package (.zip)", dst.AsPath());
            }
            catch (Exception ex)
            {
                builder.OnError(1, ex.Message, 0, 0);
                return null;
            }
        }
        #endregion

    }


}
