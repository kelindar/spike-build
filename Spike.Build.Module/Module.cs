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
