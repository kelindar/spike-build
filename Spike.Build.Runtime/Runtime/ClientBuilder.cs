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
using ICSharpCode.SharpZipLib.Zip;

namespace Spike.Build.Client
{
    /// <summary>
    /// Base class for a client builder
    /// </summary>
    public abstract class ClientBuilder : BuilderBase
    {
		private string fRootFolder = null;
        private string fName = null;

        /// <summary>
        /// Gets the collection of generated source files.
        /// </summary>
        public List<BuildFileInfo> Sources 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// Gets the collection of protocol definitions used for build process.
        /// </summary>
        internal List<ProtocolDefinitionFile> Definitions 
        {
            get; 
            private set; 
        }

        /// <summary>
        /// Gets the internal protocol model.
        /// </summary>
        public ProtocolModel Model 
        {
            get; 
            private set; 
        }

        /// <summary>
        /// Gets the root folder for the builder.
        /// </summary>
        public string RootFolder 
		{ 
			get 
			{
                if (fRootFolder == null)
                    fRootFolder = GetAppDataPath(GetType().Name.Replace("Builder", ""));
				return fRootFolder;
			} 
		}

        /// <summary>
        /// Gets the compiler which is used to build the client library.
        /// </summary>
        protected Compiler Compiler
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the name of this client builder.
        /// </summary>
        public virtual string Name
        {
            get
            {
                if (fName == null)
                {
                    string name = this.GetType().Name;
                    if(name.EndsWith("Builder"))
                        name = name.Remove(name.Length - 7, 7);
                    fName = name;
                }
                return fName;
            }
            set { fName = value; }
        }

        /// <summary>
        /// Gets the user-friendly name of the language for which this client builder generates the
        /// code.
        /// </summary>
        public abstract string Language
        {
            get;
        }

        /// <summary>
        /// Gets the user-friendly description of the language for which this client builder generates the
        /// code.
        /// </summary>
        public abstract string Description
        {
            get;
        }

        /// <summary>
        /// Gets the user-friendly description of the ideal usage the language for which this client builder generates the code.
        /// </summary>
        public abstract string Usage
        {
            get;
        }


        /// <summary>
        /// Builds the client proxy library.
        /// </summary>
        /// <param name="compiler">The compiler that requests the build operation.</param>
        /// <param name="definitions">The protocol definitions to build.</param>
        /// <returns>The collection of build outputs.</returns>
        public BuildResult[] BuildLibrary(Compiler compiler, IEnumerable<ProtocolDefinitionFile> definitions)
        {
            // Set the properties & definitions
            this.Compiler = compiler;
            this.Definitions = definitions.ToList();
            if (Sources == null)
                Sources = new List<BuildFileInfo>();
            Model = new ProtocolModel();

            // Forward
            return OnBuildLibrary();
        }

        /// <summary>
        /// Occurs during the build process.
        /// </summary>
        /// <param name="definitions">The protocol definitions to build.</param>
        /// /// <returns>The collection of build outputs.</returns>
        protected virtual BuildResult[] OnBuildLibrary()
        {
            // Generate for each file
            foreach (var file in this.Definitions)
            {
                Validate(file.Spml, "");
                GenerateCode(file.Spml);
            }

            // by default, return null
            return null;
        }

        #region Path Retrieval

        public string GetAppDataPath(string subFolder)
        {
            string AppDataDirectory = Compiler.WorkingDirectory.AsPath();
            string AppDataSpikeDirectory = Path.GetFullPath(String.Format(@"{0}\Spike", AppDataDirectory)).AsPath();

            try
            {
                if (!Directory.Exists(AppDataSpikeDirectory))
                    Directory.CreateDirectory(AppDataSpikeDirectory);

                if (!String.IsNullOrWhiteSpace(subFolder))
                {
                    string DestinationDirectory = Path.GetFullPath(String.Format(@"{0}\Spike\{1}\", AppDataDirectory, subFolder)).AsPath();
                    if (!Directory.Exists(DestinationDirectory))
                        Directory.CreateDirectory(DestinationDirectory);

                    return DestinationDirectory;
                }
                else
                {
                    return AppDataSpikeDirectory;
                }

            }
            catch
            {
                return GetTempPath(subFolder);
            }
        }

        public string GetTempPath(string subFolder)
        {
            string AppDataDirectory = Path.GetTempPath().AsPath();
            string AppDataSpikeDirectory = Path.GetFullPath(String.Format(@"{0}Spike", AppDataDirectory)).AsPath();

            try
            {
                if (!Directory.Exists(AppDataSpikeDirectory))
                    Directory.CreateDirectory(AppDataSpikeDirectory);

                if (!String.IsNullOrWhiteSpace(subFolder))
                {

                    string DestinationDirectory = Path.GetFullPath(String.Format(@"{0}Spike\{1}\", AppDataDirectory, subFolder)).AsPath();
                    if (!Directory.Exists(DestinationDirectory))
                        Directory.CreateDirectory(DestinationDirectory);

                    return DestinationDirectory;
                }
                else
                {
                    return AppDataSpikeDirectory;
                }
            }
            catch
            {
                return "";
            }


        }
        #endregion

        #region Add Source
        public virtual void AddSourceFile(string filename, TextWriter source)
        {
            AddSourceFile(filename, source.ToString());
        }

        public virtual void AddSourceFile(string subDirectory, string filename, TextWriter source)
        {
            AddSourceFile(Path.Combine(subDirectory, filename), source.ToString());
        }
		
		public virtual void AddSourceFile(string subDirectory, string filename, string source)
        {
            AddSourceFile(Path.Combine(subDirectory, filename), source);
        }
		
		public virtual void AddSourceFile(string path, string source)
        {
            var file = new FileInfo(Path.Combine(RootFolder, path));
            if (file.Exists)
                file.Delete();

            if (!file.Directory.Exists)
                file.Directory.Create();

            Sources.Add(new BuildFileInfo(file.FullName, source));
            File.WriteAllText(file.FullName, source);
        }
        #endregion

        #region Unzip Resource & CopyFilesRecursively
        public void UnzipSource(string subFolder, byte[] zip)
        {
            // Clean the destination
            string destinationFolder = Path.Combine(RootFolder + subFolder);
            if(Directory.Exists(destinationFolder))
                Directory.Delete(destinationFolder, true);

            // Unzip original sources
            using (var zipStream = new ZipInputStream(new MemoryStream(zip)))
            {
                ZipEntry theEntry;
                while ((theEntry = zipStream.GetNextEntry()) != null)
                {

                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);
                    
                    // create directory
                    Directory.CreateDirectory(destinationFolder);
                    if (fileName != String.Empty)
                    {
                        var info = new FileInfo(Path.Combine(destinationFolder, theEntry.Name));
                        if (!info.Directory.Exists)
                            info.Directory.Create();

                        FileStream streamWriter = File.Create(info.FullName);
                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = zipStream.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                        streamWriter.Close();

                        // Also, add to generated
                        if (Sources == null)
                            Sources = new List<BuildFileInfo>();
                        Sources.Add(new BuildFileInfo(info.FullName, File.ReadAllText(info.FullName)));
                    }
                }
            }
        }

        public static void CopyFilesRecursively(string sourceDir, string targetDir)
        {
            CopyFilesRecursively(new DirectoryInfo(sourceDir), new DirectoryInfo(targetDir));
        }

        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                if (!dir.Name.StartsWith("."))
                    CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            }
            foreach (FileInfo file in source.GetFiles())
            {
                var to = Path.Combine(target.FullName, file.Name);
                if (File.Exists(to)) File.Delete(to);
                file.CopyTo(Path.Combine(target.FullName, file.Name));
            }
        }


        #endregion
    }

}
