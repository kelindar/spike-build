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
    public class SdkPage : IHttpHandler
    {
        public bool CanHandle(HttpContext context, HttpVerb verb, string url)
        {
            return verb == HttpVerb.Get && url.StartsWith("/sdk?package=");
        }


        public void ProcessRequest(HttpContext context)
        {
            HttpRequest  request  = context.Request;
            HttpResponse response = context.Response;

            FileInfo packageFile;
            if (TryGetPackage(request.Path, out packageFile))
            {
                byte[] packageData = File.ReadAllBytes(packageFile.FullName);
                response.ContentType = "application/octet-stream";
                response.SetHeader("Content-Disposition", "attachment; filename=" + packageFile.Name);
                response.Write(packageData, 0, packageData.Length);
            }
            else
            {
                response.Status = "404";
            }

        }

        private bool TryGetPackage(string path, out FileInfo packageFile)
        {
            packageFile = null;
            path = path.Replace("/sdk?package=", "");

            var parameters = path.Split('|');
            if (parameters.Length != 2)
                return false;

            packageFile = new FileInfo(Path.Combine(SelfCompiler.OutputDirectory, Path.Combine(parameters[0], parameters[1])));
            if (packageFile == null || !packageFile.Exists)
            {
                packageFile = null;
                return false;
            }

            return true;
        }




    }
}
