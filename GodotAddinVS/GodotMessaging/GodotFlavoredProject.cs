using System;
using System.Runtime.InteropServices;
using GodotAddinVS.Debugging;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Flavor;
using Microsoft.VisualStudio.Shell.Interop;

namespace GodotAddinVS.GodotMessaging
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    internal class GodotFlavoredProject : FlavoredProjectBase, IVsProjectFlavorCfgProvider
    {
        private IVsProjectFlavorCfgProvider _innerFlavorConfig;

        public int CreateProjectFlavorCfg(IVsCfg pBaseProjectCfg, out IVsProjectFlavorCfg ppFlavorCfg)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ppFlavorCfg = null;

            if (_innerFlavorConfig != null)
            {
                GetProperty(VSConstants.VSITEMID_ROOT, (int)__VSHPROPID.VSHPROPID_ExtObject, out var project);

                _innerFlavorConfig.CreateProjectFlavorCfg(pBaseProjectCfg, out IVsProjectFlavorCfg cfg);
                ppFlavorCfg = new GodotDebuggableProjectCfg(cfg, project as EnvDTE.Project);
            }

            return ppFlavorCfg != null ? VSConstants.S_OK : VSConstants.E_FAIL;
        }

        protected override void SetInnerProject(IntPtr innerIUnknown)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            object inner = Marshal.GetObjectForIUnknown(innerIUnknown);
            _innerFlavorConfig = inner as IVsProjectFlavorCfgProvider;

            if (serviceProvider == null)
                serviceProvider = GodotPackage.Instance;

            base.SetInnerProject(innerIUnknown);
        }
    }
}
