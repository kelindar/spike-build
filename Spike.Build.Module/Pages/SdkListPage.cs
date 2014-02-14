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
using Spike.Web;
using System.IO;

namespace Spike.Build
{
    public class SdkListPage : BuiltinPageHandler
    {
        public override bool CanHandle(HttpContext context, HttpVerb verb, string url)
        {
            return verb == HttpVerb.Get && url == "/sdk";
        }

        public override string Title
        {
            get { return "Client SDKs | Spike Engine"; }
        }

        public override void WriteBody(HttpContext context)
        {
            HttpRequest  request  = context.Request;
            HttpResponse response = context.Response;

            response.Write("<h1>Client Software Development Kits</h1>");
            response.Write("<ul class=\"pk_menu\">");
            var directories = Directory.EnumerateDirectories(SelfCompiler.OutputDirectory);
            foreach (var directory in directories)
            {
                ProcessDirectory(response, directory); 
            }
            response.Write("</ul>");
        }

        public static void Extender(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            if (String.IsNullOrWhiteSpace(SelfCompiler.OutputDirectory))
                return;

            response.Write("<div class=\"gadget\"><h2 class=\"star\">Client SDKs</h2>");
            response.Write("<ul class=\"pk_menu\">");
            var directories = Directory.EnumerateDirectories(SelfCompiler.OutputDirectory);
            foreach (var directory in directories)
            {
                ProcessDirectory(response, directory);
            }
            response.Write("</ul>");
            response.Write("</div>");
        }

        private static void ProcessDirectory(HttpResponse response, string directory)
        {
            var info = new DirectoryInfo(directory);
            var name = "Unknown";

            
			if (info.Name.StartsWith("Client.AS3.Source"))
            {
                name = "ActionScript3 Source Code Package (.zip)";
            }
			else if (info.Name.StartsWith("Client.AS3."))
            {
                name = "Precompiled Adobe Flash/Flex Component (.swc)";
            }
            else if (info.Name.StartsWith("Client.JavaScript.Source"))
            {
                name = "JavaScript Source Code Package (.zip)";
            }
            else if (info.Name.StartsWith("Client.JavaScript.Script"))
            {
                name = "JavaScript Source Code in a Script File (.js)";
            }
            else if (info.Name.StartsWith("Client.JavaScript.Optimized"))
            {
                name = "JavaScript Source Code in an Optimized Script File (.js)";
            }
            else if (info.Name.StartsWith("Client.CSharp.Script"))
            {
                name = "C# 2.0 Source Code in a Single File (.cs)";
            }
            else if (info.Name.StartsWith("Client.CSharp.v"))
            {
                name = "Precompiled .NET Assembly for Microsoft or Mono Framework v." + info.Name.Replace("Client.CSharp.v","") + " (.dll)";
            }

            foreach(var file in info.GetFiles())
            {
                string filename = info.Name + "|" + file.Name;
                response.Write("<li><a href='/sdk?package=" + filename + "' >[Dowload]</a> - " + name + "</li>");
            }
        }

    }
}
