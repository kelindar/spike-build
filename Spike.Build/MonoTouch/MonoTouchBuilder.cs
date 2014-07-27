using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Spike.Build.MonoTouch
{
    internal class MonoTouchBuilder : IBuilder
    {
        public void Build(Model model, string output) {
            if (string.IsNullOrEmpty(output))
                output = @"MonoTouch";
        }        
    }
}
