using Microsoft.VisualStudio;
using Mono.Debugging.Soft;
using Mono.Debugging.VisualStudio;
using System;
using System.Net;

namespace GodotAddinVS.Debugging
{
    internal class GodotDebuggableProject
    {
        private readonly EnvDTE.Project _baseProject;
        public ExecutionType ExecutionType { get; }

        public GodotDebuggableProject(EnvDTE.Project project, ExecutionType executionType)
        {
            _baseProject = project;
            ExecutionType = executionType;
        }

        public int DebugLaunch()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            var random = new Random(DateTime.Now.Millisecond);
            var port = 8800 + random.Next(0, 100);

            var startArgs = new SoftDebuggerListenArgs(_baseProject.Name, IPAddress.Loopback, port) {MaxConnectionAttempts = 3};

            var startInfo = new GodotStartInfo(startArgs, null, _baseProject)
            {
                WorkingDirectory = GodotPackage.Instance.GodotSolutionHandler?.SolutionDir
            };
            var session = new GodotDebuggerSession(ExecutionType);

            var launcher = new MonoDebuggerLauncher(new Progress<string>());

            launcher.StartSession(startInfo, session);

            return VSConstants.S_OK;
        }
    }
}
