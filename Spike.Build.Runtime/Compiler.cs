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
using System.Reflection;
using Spike.Build.Client;
using System.Net;


namespace Spike.Build
{
	/// <summary>
	/// The static class that exposes Spike.Build compilation machinery.
	/// </summary>
	public class Compiler
	{
		#region Private Fields
		private List<string> Flags = new List<string>();
		private List<Assembly> Assemblies = new List<Assembly>();
		private List<Type> fBuilderTypes = new List<Type>();
		private List<ClientBuilder> fBuilders = new List<ClientBuilder>();
		private List<ProtocolDefinitionFile> fDefinitions = new List<ProtocolDefinitionFile>();
        private string fWorkingDirectory = null;
		private string BuilderFlag = null; // default: build all
		#endregion
	
		#region Build Method
		/// <summary>
		/// Performs the build process on the specified arguments.
		/// </summary>
		/// <param name="args">Arguments used for the build process.</param>
        /// <returns>Information about output of the build process.</returns>
		public BuildResult[] Build(params string[] args)
		{
			try
			{
				// Figures out the builders
				AutoResolveModules();

				// Parse arguments and load files
				Initialize(args);

				// Check if we have input
				if (Definitions.Count() == 0)
				{
					PromptCritical(@"No protocol definitions have been specified in the input. Either specify the assemblies that expose .spml files as embedded resources or specify the URL of a running Spike-Engine instance, ending with /spml (e.g.: http://127.0.0.1/spml).");
					return new BuildResult[0];
				}

				// Set output directory
				if (String.IsNullOrEmpty(OutputDirectory) || !Directory.Exists(OutputDirectory))
					OutputDirectory = Directory.GetCurrentDirectory();
			}
			catch(Exception ex)
			{
                Console.WriteLine("Build error: " + ex.Message);
                return new BuildResult[0];
			}

            // Our output
            var output = new List<BuildResult>();

			// Iterate through all bound builders
			foreach (var builder in this.Builders)
			{
				// Prepare builder
				try
				{
					// Check if we should build only one 
					if (!String.IsNullOrEmpty(BuilderFlag))
					{
						if (builder.Name.ToLower() != BuilderFlag.ToLower())
							continue;
					}

					builder.Error += (sender, error) => {
						Console.WriteLine("Error: {0}", error.Message);
						if(this.Error != null) // Redirect error
							this.Error(sender, error);
					};

					// Build the library
					var libraryOutput = builder.BuildLibrary(this, fDefinitions);
                    if (libraryOutput != null)
                        output.AddRange(libraryOutput);
				}
				catch (Exception ex) 
				{
					Console.WriteLine("Error: " + ex.Message);
					if (this.Error != null) // Redirect error
						this.Error(null, new BuildErrorEventArgs(3, ex.Message, 0, 0));
				}
			}

            // Return our output
            return output.ToArray();
		}
		#endregion

		#region Public Properties

		/// <summary>
		/// Invoked when an error occurs during the build process.
		/// </summary>
		public event EventHandler<BuildErrorEventArgs> Error;

		/// <summary>
		/// Gets the list of protocol definitions currently loaded.
		/// </summary>
		public IEnumerable<ProtocolDefinitionFile> Definitions
		{
			get { return fDefinitions; }
		}
		
		/// <summary>
		/// Gets or sets output directory for the compiler
		/// </summary>
		public string OutputDirectory
		{
			get;
			set;
		}

        /// <summary>
        /// Gets the working directory for all client builders.
        /// </summary>
        public string WorkingDirectory
        {
            get
            {
                if (fWorkingDirectory == null)
                    fWorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                return fWorkingDirectory;
            }
            set
            {
                fWorkingDirectory = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the compiler should only build and not attempt to also compile.
        /// </summary>
        public bool OnlyBuild
        {
            get;
            set;
        }

		/// <summary>
		/// Gets the collection of client proxy builders which is used by this compiler.
		/// </summary>
		public IEnumerable<ClientBuilder> Builders
		{
			get { return fBuilders; }
		}
		#endregion

		#region Public Members

		/// <summary>
		/// Adds a collection of protocol definitions
		/// </summary>
		/// <param name="definitions">Definitions to add</param>
		public void AddDefinition(IEnumerable<ProtocolDefinitionFile> definitions)
		{
			fDefinitions.AddRange(definitions);
		}

        /// <summary>
        /// Adds a single protocol definition 
        /// </summary>
        /// <param name="definition">Definition to add</param>
        public void AddDefinition(ProtocolDefinitionFile definition)
        {
            fDefinitions.Add(definition);
        }

		/// <summary>
		/// Adds a definition from the specified URI
		/// </summary>
		/// <param name="uri">The URI that contains the definition.</param>
		public void AddDefinition(Uri uri)
		{
            // For quick checks
            var fullUri = uri.ToString().ToLower();

            // If we have a uri for one definition, just add this
            if (fullUri.Contains("?file=") && !fullUri.EndsWith("?file="))
            {
                // Retrieve the name
                var definitionName = uri.AbsoluteUri.Split(new string[] { "?file=" },
                    StringSplitOptions.RemoveEmptyEntries)[1];

                // Retrieve the body
                using (var web = new WebClient())
                {
                    // Download the definition
                    fDefinitions.Add(
                        new ProtocolDefinitionFile(definitionName, web.DownloadString(uri))
                        );
                }
            }
            else
            {
                // Ensure we have a correct path
                if (fullUri.EndsWith("/spml/"))
                    fullUri += "all";
                else if (fullUri.EndsWith("/spml"))
                    fullUri += "/all";
                else if (!fullUri.EndsWith("/spml/all"))
                    fullUri += fullUri.EndsWith("/") 
                        ? "smpl/all"
                        : "/spml/all";

                // Retrieve the list
                var result = new List<ProtocolDefinitionFile>();
                var uriBase = fullUri.Remove(fullUri.Length - 4, 4) + "?file=";
                using (var web = new WebClient())
                {
                    var spmlList = web.DownloadString(uri).Split('|');
                    foreach (var filename in spmlList)
                        result.Add(new ProtocolDefinitionFile(filename, web.DownloadString(new Uri(uriBase + filename))));
                }

                fDefinitions.AddRange(result);
            }
		}

		/// <summary>
		/// Adds a definition from the assembly. The assembly must expose the definition spml file as an
		/// embedded resource.
		/// </summary>
		/// <param name="assembly">The assembly that contains definitions.</param>
		public void AddDefinition(Assembly assembly)
		{
			var result = new List<ProtocolDefinitionFile>();
			var resourceNames = assembly.GetManifestResourceNames();
			var spmlResources = resourceNames.Where(res => res.ToLower().EndsWith(".spml"));
			foreach (var item in spmlResources)
			{
				var stream = assembly.GetManifestResourceStream(item);
				using (var reader = new StreamReader(stream))
					result.Add(new ProtocolDefinitionFile(item, reader.ReadToEnd()));
			}

			fDefinitions.AddRange(result);
		}

		/// <summary>
		/// Adds a client builder to the collection of active builders for this compiler.
		/// </summary>
		/// <param name="builder">The instance of a client builder to add.</param>
		public void AddBuilder(ClientBuilder builder)
		{
			if (builder == null)
				return;

			var type = builder.GetType();
			if (!fBuilderTypes.Contains(type))
				fBuilderTypes.Add(type);

			this.fBuilders.Add(builder);
		}

		/// <summary>
		/// Adds a client builder to the collection of active builders for this compiler.
		/// </summary>
		/// <typeparam name="T">The type of the builder to add.</typeparam>
		public void AddBuilder<T>()
			where T: ClientBuilder, new()
		{
			var type = typeof(T);
			if (!fBuilderTypes.Contains(type))
				fBuilderTypes.Add(type);

			this.fBuilders.Add(new T());
		}

		/// <summary>
		/// Gets the builder by the name. The name is defined in the Name property of the client
		/// builder.
		/// </summary>
		/// <param name="name">The name of the builder to get.</param>
		/// <returns>The provider if found, null otherwise.</returns>
		public ClientBuilder GetBuilder(string name)
		{
			return this.fBuilders.Find((b) => b.Name == name);
		}

		#endregion

		#region Metadata Members
		/// <summary>
		/// Attempts to resolve module assemblies automatically
		/// </summary>
		private void AutoResolveModules()
		{
			// Get the assembly and the executing path.
			var entryAssembly   = Assembly.GetEntryAssembly();
			var runtimeAssembly = Assembly.GetExecutingAssembly();

			// Get the path
			var baseDirectory = Assembly.GetEntryAssembly().Location;
			if (baseDirectory.Length > 0)
				baseDirectory = Path.GetDirectoryName(baseDirectory);

			// Get referenced assemblies
			var comparer = new AssemblyNameComparer();
			var assemblies = entryAssembly.GetReferencedAssemblies()
				.ToList();

			// Get root folder assemblies
			var dlls = Directory.GetFiles(baseDirectory, "*.dll");
			for (int i = 0; i < dlls.Length; ++i)
			{
				try
				{
					AssemblyName name = AssemblyName.GetAssemblyName(dlls[i]);
					if (!assemblies.Contains(name, comparer))
						assemblies.Add(name);
				}
				catch { }
			}

			int assembliesCount = assemblies.Count;
			for (int i = 0; i < assembliesCount; i++)
			{
				AssemblyName name = assemblies[i];
				Assembly assembly = Assembly.ReflectionOnlyLoad(name.FullName);
				AssemblyName[] refs = assembly.GetReferencedAssemblies();
				for (int j = 0; j < refs.Length; ++j)
				{
					AssemblyName reference = refs[j];
					if (String.Equals(reference.FullName, runtimeAssembly.FullName))
					{
						if (assembly.ReflectionOnly)
							assembly = Assembly.Load(assembly.FullName);
						
						FindBuilder(assembly);
					}
					else
					{
						if (!assemblies.Contains(reference, comparer))
							assemblies.Add(reference);
					}
				}
			}
		}

		/// <summary>
		/// Attempts to find builders in an assembly.
		/// </summary>
		/// <param name="assembly">The assembly to search</param>
		private void FindBuilder(Assembly assembly)
		{
			try
			{
				var types = assembly.GetTypes();
				for (int i = 0; i < types.Length; ++i)
				{
					var type = types[i];
					if (type.IsSubclassOf(typeof(ClientBuilder)))
					{
						// Make sure we don't add it twice
						if (fBuilderTypes.Contains(type))
							continue;
						fBuilderTypes.Add(type);

						try
						{
							// Auto-instanciate the builder and add it.
							var builder = Activator.CreateInstance(type);
							this.AddBuilder(builder as ClientBuilder);
						}
						catch
						{
							// Skip, failed to auto-instanciate.
						}
					}
				}
			}
			catch (ReflectionTypeLoadException)
			{
				// "Error: Loading of assembly {0} has thrown an exception: {1} Skipping...", assembly.FullName, ex.Message)
			}
		}

		#endregion

		#region Usage & Flags

		private void Initialize(string[] args)
		{
			if (this.fBuilders.Count == 0)
			{
				PromptCritical(@"Unable to find any registered builders. Make sure the Spike.Build.* Assemblies are present next to this program. Alternatively you can use Compiler.AddBuilder() API to register builders manually.");
				return;
			}

			if (args.Length == 0)
			{
				PromptUsage();
				throw new ApplicationException();
			}

			foreach (var argument in args)
			{
				if (argument.StartsWith("-") || argument.StartsWith("/"))
				{
					string flag = argument.Substring(1, argument.Length - 1);
					if (flag.ToLower().StartsWith("builder:"))
						BuilderFlag = flag.Substring(8, flag.Length - 8);
					if (flag.ToLower().StartsWith("out:"))
						OutputDirectory = flag.Substring(4, flag.Length - 4);

					// Add to flags list
					Flags.Add(flag);
				}
				else if (argument.ToLower().EndsWith("/spml"))
				{
					AddDefinition(new Uri(argument.ToLower() + "/all"));
				}
				else if (argument.ToLower().EndsWith("/spml/all"))
				{
					AddDefinition(new Uri(argument.ToLower()));
				}
				else if (File.Exists(argument)) // Not a parameter, probably an assembly 
				{
					if (argument.ToLower().EndsWith(".dll") || argument.ToLower().EndsWith(".exe"))
					{
						var assembly = Assembly.LoadFile(Path.GetFullPath(argument));
						Assemblies.Add(assembly);
						AddDefinition(assembly);
					}
					else
					{
						Console.WriteLine("File not supported: " + argument);
						throw new NotImplementedException();
					}
				}
				else Console.WriteLine("Unrecognized argument: " + argument);
			}
		}

		private void PromptUsage()
		{
			PromptCopyright();
			PromptBuilders();
			Console.WriteLine();
			Console.WriteLine(" Usage: ");
			Console.WriteLine("  Spike.Build [Input List] [Parameter List]");
            Console.WriteLine();
            Console.WriteLine(" Input: ");
            Console.WriteLine("  Input could be either:");
            Console.WriteLine("    (a) A list of assemblies (.dll) that contain SPML files as embedded resources, separated by space.");
            Console.WriteLine("    (b) A URL of a running Spike-Engine instance with a /spml/all suffix. (ex: http://127.0.0.1:8002/spml/all )");
			Console.WriteLine();
			Console.WriteLine(" Parameters: ");
			Console.WriteLine("  -out:<PATH> Defines the output path");
			Console.WriteLine("  -builder:<CSharp|AS3|JavaScript> Defines the name of the builder to use to generate the client library");

		}

		private void PromptBuilders()
		{
			Console.WriteLine();
			Console.WriteLine("Loaded Builders:");
			foreach(var builder in this.Builders)
				Console.WriteLine(" - Name: {0}, Language: {1}", builder.Name, builder.Language);
		}

		private void PromptCopyright()
		{
			Console.WriteLine("Spike Engine Builder, Version {0}", Assembly.GetEntryAssembly().GetName().Version);
			Console.WriteLine("Copyright (C) Roman Atachiants (email: roman@spike-engine.com)");
		}

		private void PromptCritical(string error)
		{
			Console.WriteLine(@" Critical Error: ");
			Console.WriteLine(" " + error);
			throw new ApplicationException();
		}
		#endregion

		#region AssemblyNameComparer
		private class AssemblyNameComparer : IEqualityComparer<AssemblyName>
		{
			public bool Equals(AssemblyName x, AssemblyName y)
			{
				if (Object.ReferenceEquals(x, y)) return true;
				if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
					return false;
				return x.FullName == y.FullName;
			}

			public int GetHashCode(AssemblyName name)
			{
				if (Object.ReferenceEquals(name, null)) return 0;
				return name.FullName == null ? 0 : name.FullName.GetHashCode();
			}
		}
		#endregion
	}
}
