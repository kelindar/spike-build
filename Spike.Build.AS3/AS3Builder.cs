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
using System.Reflection;
using Spike.Build.Runtime.Properties;

namespace Spike.Build.Client
{
    public class AS3Builder : ClientBuilder
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
            UnzipSource(HandlerPath, Spike.Build.AS3.Properties.Resources.AS3Src);

            // Build the library
            base.OnBuildLibrary();

            // Continue build, global files
            AS3ReaderBuilder.GenerateCode(this);
            AS3ChannelBuilder.GenerateCode(this);
			
			// Save Source Package
			var package = GenerateSourcePackage(this);
            if (package != null)
                output.Add(package);

            // Compile everything
            if (!this.Compiler.OnlyBuild)
            {
				var compiler = new AS3Compiler(this);
                compiler.Compile(
                    Path.Combine(RootFolder, SrcOutputPath),
                    Path.Combine(RootFolder, BinOutputPath));

                // Compiled output
                output.Add(new BuildResult(this, "Precompiled Adobe Flash/Flex Component (.swc)", BinOutputPath));
            }

            // Move output to directory
            CopyFilesRecursively(
                Path.Combine(RootFolder, BinOutputPath),
                this.Compiler.OutputDirectory);

            // Return the generated info
            return output.ToArray();
        }

        #region Properties
        private string fDataPath = @"Generated\network\data".AsPath();
        private string fPacketsPath = @"Generated\network\packets".AsPath();
        private string fHandlerPath = @"Generated\network".AsPath();
        private string fSrcOutputPath = @"Generated".AsPath();
        private string fBinOutputPath = @"Output".AsPath();

        /// <summary>
        /// Gets or sets the output path for generated source-code.
        /// </summary>
        [PropertyPath]
		public string SrcOutputPath
        {
            get { return fSrcOutputPath; }
            set 
            {
                value          = value.RemoveLastSlash();
                fSrcOutputPath = value.AsPath(); 
                fDataPath      = (value + @"\network\data").AsPath();
                fPacketsPath   = (value + @"\network\packets").AsPath();
                fHandlerPath   = (value + @"\network").AsPath();
            }
        }

        /// <summary>
        /// Gets or sets the output path for compiled swc.
        /// </summary>
		[PropertyPath]
        public string BinOutputPath
        {
            get { return fBinOutputPath; }
            set { fBinOutputPath = value.AsPath(); }
        }
		
		/// <summary>
        /// Gets or sets flex SDK path containing the compiler.
        /// </summary>
		[PropertyPath]
        public string FlexSdkPath
        {
            get;
            set;
        }
		
        /// <summary>
        /// Gets the output source data path.
        /// </summary>
        internal string DataPath
        {
            get { return fDataPath; }
        }

        /// <summary>
        /// Gets the output source packets path.
        /// </summary>
        internal string PacketsPath
        {
            get { return fPacketsPath; }
        }

        /// <summary>
        /// Gets the output source handlers path.
        /// </summary>
        internal string HandlerPath
        {
            get { return fHandlerPath; }
        }


        /// <summary>
        /// Gets the user-friendly name of the language for which this client builder generates the
        /// code.
        /// </summary>
        [PropertyText]
        public override string Language
        {
            get { return "ActionScript3"; }
        }

        /// <summary>
        /// Gets the user-friendly description of the language for which this client builder generates the
        /// code.
        /// </summary>
        [PropertyText]
        public override string Description
        {
            get { return "ActionScript is an object-oriented language. It is a dialect of ECMAScript and is used primarily for the development of websites and software targeting the Adobe Flash Player platform."; }
        }

        /// <summary>
        /// Gets the user-friendly description of the ideal usage the language for which this client builder generates the code.
        /// </summary>
        [PropertyText]
        public override string Usage
        {
            get { return "Adobe Flex, Adobe Flash"; }
        }
        #endregion

        #region IBuilder Members

        public override string GenerateCode(string inputFileContent)
        {
            // Declare builders
            var BuilderForElement = new AS3ElementBuilder();
            var BuilderForPacket = new AS3PacketBuilder();

            // Begin code generation
            using (var writer = new CodeWriter())
            {
                var xml = Protocol.Deserialize(inputFileContent);
                if (xml != null)
                {
                    // Preprocessing: mutate by cloning the protocol and transforming it
                    var protocol = Model.Mutate(xml);


                    // 1st step: transform the model for AS3 Client
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
            element.InternalType = GetPropertyType(element);
            element.InternalName = element.Name;

            switch (element.Type)
            {
                case ElementType.DynamicType:
                {
                    element.InternalType = element.InternalElementType = element.Class = "Object";
                    break;
                }

                case ElementType.ComplexType:
                {
                    element.InternalElementType = element.InternalType;
                    break;
                }

                case ElementType.ListOfByte:
                {
                    element.InternalType = element.Class = "ByteArray";
                    element.InternalElementType = "byte";
                    break;
                }

                case ElementType.ListOfDynamicType:
                {
                    element.InternalType = element.Class = "Array";
                    element.InternalElementType = "Object";
                    break;
                }

                case ElementType.ListOfComplexType:
                {
                    element.InternalType = "Array";
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
                        element.InternalElementType = GetPropertyType(element, (ElementType) Enum.Parse(typeof(ElementType), simpleType, true));
                        element.Class = "Array";
                    }
                    break;
                }
            }            
        }

        internal static string GetPropertyType(Element element)
        {
            return GetPropertyType(element, element.Type);
        }

        internal static string GetPropertyType(Element element, ElementType type)
        {
            if (type == ElementType.Enum)
                return "int";
            else if (type == ElementType.UInt16)
                return "uint";
            else if (type == ElementType.Int16)
                return "int";
            else if (type == ElementType.UInt32)
                return "uint";
            else if (type == ElementType.Int32)
                return "int";
            else if (type == ElementType.Int64)
                return "Int64";
            else if (type == ElementType.UInt64)
                return "UInt64";
            else if (type == ElementType.DateTime)
                return "Date";
            else if (type == ElementType.String)
                return "String";
            else if (type == ElementType.Byte)
                return "uint";
            else if (type == ElementType.Boolean)
                return "Boolean";
            else if (type == ElementType.Double)
                return "Number";
            else if (type == ElementType.Single)
                return "Number";
            else if (type == ElementType.DynamicType)
                return "Object";
            else if (type == ElementType.ListOfByte)
                return "ByteArray";
            else if (element.IsList)
                return "Array";
            else if (element.IsComplexType)
                return element.Class + "Entity";

            return type.ToString();
        }


        #endregion

        #region GenerateHeader
        internal static void GenerateHeader(TextWriter writer)
        {
            writer.WriteLine("import flash.utils.ByteArray;");
            writer.WriteLine("import network.*;");
            writer.WriteLine("import network.packets.*;");
            writer.WriteLine();
        }
        #endregion
		
		#region Generate Source Package
        private static BuildResult GenerateSourcePackage(AS3Builder builder)
        {
			try
			{
	            var PackageOutput = String.Format(@"{0}\Client.AS3.Source\", Path.Combine(builder.RootFolder, builder.BinOutputPath)).AsPath();
	            if (Directory.Exists(PackageOutput))
	                Directory.Delete(PackageOutput, true);
	            Directory.CreateDirectory(PackageOutput);


                // Convention: product-name.plugin-ver.sion.filetype.js

				var zip = new ICSharpCode.SharpZipLib.Zip.FastZip();
                var src = Path.Combine(builder.RootFolder, builder.SrcOutputPath);
				var dst = Path.Combine(PackageOutput, "spike-sdk.as3.src.zip");
				zip.CreateEmptyDirectories = true;
				zip.CreateZip(dst,src, true, "");

                // Our output
                return new BuildResult(builder, "ActionScript3 Source Code Package (.zip)", dst.AsPath());

			}
			catch(Exception ex)
			{
				builder.OnError(1, ex.Message,0,0);
                return null;
			}
			
        }
        #endregion

    }
}
