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
using Spike.Network;
using Spike.Network.Http;
using System.IO;
using Spike.Build.Client;
using System.Reflection;

namespace Spike.Build
{
    /// <summary>
    /// The static class that represents a self-compiler module for Spike.Build
    /// </summary>
    public static class SelfCompiler
    {
        private static Compiler Compiler = new Build.Compiler();
        private static BuildErrorEventArgs LastError = null;
        private static IHttpHandler JavaScriptProxyHost;
        private static bool BuildAtInitialize = true;

        /// <summary>
        /// Automatically invoked when server starts
        /// </summary>
        [InvokeAt(InvokeAtType.Initialize)]
        public static void Initialize()
        {
            Service.Http.Register(new SdkListPage());
            Service.Http.Register(new SdkPage());

            // When the server starts, build
            Compiler.Error += (sender, e) => LastError = e ;
            Service.Started += OnServerStarted;
        }

        /// <summary>
        /// Gets or sets output directory for the compiler
        /// </summary>
        public static string OutputDirectory
        {
            get 
			{ 
				if (Compiler.OutputDirectory == null)
                	Compiler.OutputDirectory = Path.Combine(Service.DataDirectory, "ClientSDK");
				return Compiler.OutputDirectory; 
			}
            set { Compiler.OutputDirectory = value; }
        }

        /// <summary>
        /// Gets or sets whether the self-compiler should be enabled or not. 
        /// By default it is enabled.
        /// </summary>
        public static bool Enabled
        {
            get { return BuildAtInitialize; }
            set { BuildAtInitialize = value; }
        }

        /// <summary>
        /// Rebuilds the client SDKs
        /// </summary>
        public static void RebuildAll()
        {
            if (!Directory.Exists(OutputDirectory))
                Directory.CreateDirectory(OutputDirectory);

            bool ErrorOccured = false;
            try
            {
                // Get all definition files and load them in the Spike.Build format
                Compiler.AddDefinition(
                    ProtocolInfo
                    .Definitions
                    .Select(d => new ProtocolDefinitionFile(d.ProtocolName, d.Spml))
                    );

                // Run the compiler
                Compiler.Build(new string[]{
                    "-out:" + OutputDirectory
                });
            }
            catch (Exception ex)
            {
                ErrorOccured = true;
                Service.Logger.Log(ex);
            }
            finally
            {
                if (LastError != null)
                {
                    ErrorOccured = true;
                    Service.Logger.Log(LogLevel.Error, LastError.Message);
                }
            }

            if (!ErrorOccured)
            {
                // Do something?
            }
        }

        /// <summary>
        /// Invoked when the server starts
        /// </summary>
        private static void OnServerStarted()
        {
			// Make sure the output path is not null
			OutputDirectory.GetType();
			
			// Add the extender
            var page = Service.Http.FindHandler((m) => m.GetType() == typeof(Spike.Web.BuiltinStatusPage)) as Spike.Web.BuiltinStatusPage;
            if (page != null)
                page.AddSideBarExtender(SdkListPage.Extender);
			
			// Rebuild if needed
            if (BuildAtInitialize)
                RebuildAll();
			
		    // Host JavaScript Proxy
            if (Service.Http != null)
            {
                // Make sure we unregister the existing handler
                if(JavaScriptProxyHost != null)
                    Service.Http.Unregister(JavaScriptProxyHost);

                // If there's a javascript builder, host the script file
                var jsBuilder = Compiler.GetBuilder("JavaScript");
                if (jsBuilder != null)
                {
                    // Get dynamically the value of the script file property
                    var scriptProp = jsBuilder.GetType().GetProperty("ScriptFile");
                    if (scriptProp == null)
                        return;

                    var scriptFile = scriptProp.GetValue(jsBuilder, null);
                    if (scriptFile == null)
                        return;

                    // Host the proxy file
                    JavaScriptProxyHost = Service.Http.Host("/proxy.js", scriptFile.ToString());
                }
            }
        }
    }
}
