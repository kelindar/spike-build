using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spike.Build.Xamarin
{    
    internal class XamarinBuilder : IBuilder
    {
        public void Build(Model model, string output)
        {
            if (string.IsNullOrEmpty(output))
                output = @"Xamarin";
        }
    }
}
