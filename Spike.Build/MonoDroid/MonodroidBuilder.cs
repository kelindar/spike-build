using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spike.Build.MonoDroid
{
    internal class MonoDroidBuilder : IBuilder
    {
        public void Build(Model model, string output) {
            if (string.IsNullOrEmpty(output))
                output = @"MonoDroid";
        }        
    }
}
