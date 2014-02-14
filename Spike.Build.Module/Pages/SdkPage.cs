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
