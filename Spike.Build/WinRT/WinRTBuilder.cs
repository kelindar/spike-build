using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Spike.Build.WinRT
{
    

    internal class WinRTBuilder : IBuilder
    {        
        public void Build(Model model, string output) {
            if (string.IsNullOrEmpty(output))
                output = @"WinRT";

            var networkDirectory = Path.Combine(output, "Spike", "Network");
            //var entitiesDirectory = Path.Combine(output, "Spike", "Entities");            

            if (!Directory.Exists(networkDirectory))
                Directory.CreateDirectory(networkDirectory);

            Extentions.CopyFromRessources("Spike.Build.WinRT.CLZF.cs", Path.Combine(networkDirectory, @"CLZF.cs"));
            Extentions.CopyFromRessources("Spike.Build.WinRT.TcpChannelBase.cs", Path.Combine(networkDirectory, @"TcpChannelBase.cs"));

            //hostspecific="true"

            var template = new TcpChannelTemplate();
            var session = new Dictionary<string, object>();
            template.Session = session;

            session["Model"] = model;
            template.Initialize();
            
            var code = template.TransformText();
            File.WriteAllText("test.cs", code);

            
        }
    }
}
