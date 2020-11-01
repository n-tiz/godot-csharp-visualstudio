using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using GodotAddinVS.Debugging;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Events;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace GodotAddinVS
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideProjectFactory(typeof(GodotFlavoredProjectFactory), "Godot.Project", null, "csproj", "csproj", null,
        LanguageVsTemplate = "CSharp", TemplateGroupIDsVsTemplate = "Godot")]
    [ProvideOptionPage(typeof(GeneralOptionsPage),
        "Godot", "General", 0, 0, true)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionOpening_string, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class GodotPackage : AsyncPackage
    {
        /// <summary>
        /// GodotPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "fbf828da-088b-482a-a550-befaed4b5d25";

        internal static GodotPackage Instance { get; private set; }

        internal GodotSolutionHandler GodotSolutionHandler { get; private set; }
        internal GodotDebugTargetSelection DebugTargetSelection { get; } = new GodotDebugTargetSelection();
        internal GodotVSLogger Logger { get; } = new GodotVSLogger();

        public GodotPackage()
        {
            Instance = this;
        }

        private async Task<bool> IsSolutionLoadedAsync()
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();
            var solService = await GetServiceAsync(typeof(SVsSolution)) as IVsSolution;

            if (solService == null) return false;

            ErrorHandler.ThrowOnFailure(solService.GetProperty((int)__VSPROPID.VSPROPID_IsSolutionOpen, out object value));

            return value is bool isSolOpen && isSolOpen;
        }

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            RegisterProjectFactory(new GodotFlavoredProjectFactory());

            if (await IsSolutionLoadedAsync())
            {
                SolutionOpen();
            }

            SolutionEvents.OnAfterOpenSolution += SolutionOpen;
            SolutionEvents.OnBeforeCloseSolution += SolutionClosed;
        }


        #region Events handlers

        private void SolutionOpen(object sender = null, EventArgs e = null)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var service = GetService(typeof(SVsSolution)) as IVsSolution;
            GodotSolutionHandler = new GodotSolutionHandler(service, this);
            foreach (var project in VsItemsHelper.GetProjectsInSolution(service, __VSENUMPROJFLAGS.EPF_LOADEDINSOLUTION))
            {
                GodotSolutionHandler.OnProjectOpened(project);
            }
            _ = Task.Run(HandleLauncherMessage);
        }

        private void SolutionClosed(object sender, EventArgs e)
        {
            GodotSolutionHandler?.OnClosingSolution();
        }

        #endregion

        public async Task ShowErrorMessageBoxAsync(string title, string message)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            var uiShell = (IVsUIShell)await GetServiceAsync(typeof(SVsUIShell));

            if (uiShell == null)
                throw new ServiceUnavailableException(typeof(SVsUIShell));

            var clsid = Guid.Empty;
            ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                0,
                ref clsid,
                title,
                message,
                string.Empty,
                0,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                OLEMSGICON.OLEMSGICON_CRITICAL,
                0,
                pnResult: out _));
        }

        void HandleLauncherMessage()
        {
            while (true)
            {
                // Won't work if multiple instances of visual studio use the Addin at the same time (PackageGuidString)
                using var pipeServer = new NamedPipeServerStream(PackageGuidString, PipeDirection.In, 1);
                using var streamReader = new StreamReader(pipeServer);
                pipeServer.WaitForConnection();
                var buffer = streamReader.ReadLine();
                Enum.TryParse(buffer, out ExecutionType argsAsEnum);
                switch (argsAsEnum)
                {
                    case ExecutionType.PlayInEditor:

                        break;
                    case ExecutionType.Launch:

                        break;
                    case ExecutionType.Attach:

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                while ((buffer = streamReader.ReadLine()) != null)
                {
                }
                pipeServer.Disconnect();
            }
        }

        protected override void Dispose(bool disposing)
        {
            SolutionEvents.OnAfterOpenSolution -= SolutionOpen;
            SolutionEvents.OnBeforeCloseSolution -= SolutionClosed;

            GodotSolutionHandler?.Dispose();
            GodotSolutionHandler = null;
            base.Dispose(disposing);
        }
    }
}
