using Microsoft.VisualStudio.Shell.Flavor;
using System;
using System.Runtime.InteropServices;
using GodotAddinVS.GodotMessaging;

namespace GodotAddinVS
{
    public class GodotFlavoredProjectFactory : FlavoredProjectFactoryBase
    {
        public GodotFlavoredProjectFactory()
        {
            //Used for debug breakpoints, do not remove
        }

        protected override object PreCreateForOuter(IntPtr outerProjectIUnknown)
        {
            return new GodotFlavoredProject();
        }
    }
}
