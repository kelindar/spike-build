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
