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
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Security.Permissions;


namespace Spike.Build
{
    [SecurityPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
    public class CommandLine
    {
        private delegate string StringDelegate();

        public static string Run(string fileName, string arguments,
            out string errorMessage)
        {
            errorMessage = "";
            Process cmdLineProcess = new Process();
            using (cmdLineProcess)
            {
                cmdLineProcess.StartInfo.FileName = fileName;
                cmdLineProcess.StartInfo.Arguments = arguments;
                cmdLineProcess.StartInfo.UseShellExecute = false; // false
                cmdLineProcess.StartInfo.CreateNoWindow = true; //true
                cmdLineProcess.StartInfo.RedirectStandardOutput = true;
                cmdLineProcess.StartInfo.RedirectStandardError = true;

                if (cmdLineProcess.Start())
                {
                    return ReadProcessOutput(cmdLineProcess, ref errorMessage,
                        fileName);
                }
                else
                {
                    throw new CommandLineException(String.Format(
                        "Could not start command line process: {0}",
                        fileName));
                    /* Note: arguments aren't also shown in the 
                     * exception as they might contain privileged 
                     * information (such as passwords).
                     */
                }
            }
        }

        private static string ReadProcessOutput(Process cmdLineProcess,
            ref string errorMessage, string fileName)
        {
            StringDelegate outputStreamAsyncReader
                = new StringDelegate(cmdLineProcess.StandardOutput.ReadToEnd);
            StringDelegate errorStreamAsyncReader
                = new StringDelegate(cmdLineProcess.StandardError.ReadToEnd);

            IAsyncResult outAR
                = outputStreamAsyncReader.BeginInvoke(null, null);
            IAsyncResult errAR = errorStreamAsyncReader.BeginInvoke(null, null);

            /* WaitHandle.WaitAll fails on single-threaded 
             * apartments. Poll for completion instead:
             */
            int i = 0;
            while (!(outAR.IsCompleted && errAR.IsCompleted))
            {
                /* Check again every 10 milliseconds: */
                Thread.Sleep(10);
                i++;
            }

            string results = outputStreamAsyncReader.EndInvoke(outAR);
            errorMessage = errorStreamAsyncReader.EndInvoke(errAR);

            /* At this point the process should surely have exited,
             * since both the error and output streams have been fully read.
             * To be paranoid, let's check anyway...
             */
            if (!cmdLineProcess.HasExited)
            {
                cmdLineProcess.WaitForExit();
            }

            return results;
        }

        public static string Run(string fileName, string arguments)
        {
            string result;
            string errorMsg = String.Empty;

            result = Run(fileName, arguments, out errorMsg);

            if (errorMsg.Length > 0)
                throw new CommandLineException(errorMsg);

            return result;
        }

        public static string Run(string fileName)
        {
            return Run(fileName, "");
        }

        [Serializable]
        public class CommandLineException : Exception
        {
            public CommandLineException()
                : base()
            {
            }

            public CommandLineException(string message)
                : base(message)
            {
            }

            public CommandLineException(string message, Exception innerException)
                : base(message, innerException)
            {
            }

            protected CommandLineException(
                System.Runtime.Serialization.SerializationInfo info,
                System.Runtime.Serialization.StreamingContext context)
                : base(info, context)
            {
            }
        }
    }
}
