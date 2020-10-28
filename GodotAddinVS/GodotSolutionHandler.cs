using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using EnvDTE;
using GodotAddinVS.Debugging;
using GodotAddinVS.GodotMessaging;
using GodotTools.IdeMessaging;
using GodotTools.IdeMessaging.Requests;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GodotAddinVS
{
    internal class GodotSolutionHandler : IDisposable
    {
        private readonly IVsSolution _solution;
        private readonly IServiceProvider _serviceProvider;
        private static readonly object RegisterLock = new object();
        private bool _registered;
        private string _godotProjectDir;

        private DebuggerEvents DebuggerEvents { get; set; }

        private IServiceContainer ServiceContainer => (IServiceContainer)_serviceProvider;

        public string SolutionDir
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                _solution.GetSolutionInfo(out string solutionDir, out string solutionFile, out string userOptsFile);
                _ = solutionFile;
                _ = userOptsFile;
                return solutionDir;
            }
        }

        public Client GodotMessagingClient { get; private set; }

        public GodotSolutionHandler(IVsSolution solution, IServiceProvider serviceProvider)
        {
            _solution = solution;
            _serviceProvider = serviceProvider;
            ThreadHelper.ThrowIfNotOnUIThread();
        }

        private static IEnumerable<Guid> ParseProjectTypeGuids(string projectTypeGuids)
        {
            string[] strArray = projectTypeGuids.Split(';');
            var guidList = new List<Guid>(strArray.Length);

            foreach (string input in strArray)
            {
                if (Guid.TryParse(input, out var result))
                    guidList.Add(result);
            }

            return guidList.ToArray();
        }

        private static bool IsGodotProject(IVsHierarchy hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!(hierarchy is IVsAggregatableProject aggregatableProject))
                return false;
            if (aggregatableProject.GetAggregateProjectTypeGuids(out string projectTypeGuids) != 0) 
                return false;
            return ParseProjectTypeGuids(projectTypeGuids).Any(g => g == typeof(GodotFlavoredProjectFactory).GUID);
        }

        public int OnProjectOpened(IVsHierarchy hierarchy)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!IsGodotProject(hierarchy))
                return VSConstants.S_OK;

            lock (RegisterLock)
            {
                if (_registered)
                    return VSConstants.S_OK;

                _godotProjectDir = SolutionDir;

                DebuggerEvents = _serviceProvider.GetService<DTE>().Events.DebuggerEvents;
                DebuggerEvents.OnEnterDesignMode += DebuggerEvents_OnEnterDesignMode;

                GodotMessagingClient?.Dispose();
                GodotMessagingClient = new Client(identity: "VisualStudio",
                    _godotProjectDir, new MessageHandler(), GodotPackage.Instance.Logger);
                GodotMessagingClient.Connected += OnClientConnected;
                GodotMessagingClient.Start();

                ServiceContainer.AddService(typeof(Client), GodotMessagingClient);

                _registered = true;
            }

            return VSConstants.S_OK;
        }

        public int OnClosingSolution()
        {
            lock (RegisterLock)
                _registered = false;
            Close();
            return VSConstants.S_OK;
        }

        public void Dispose()
        {
            Close();
        }

        private void OnClientConnected()
        {
            var options = (GeneralOptionsPage)GodotPackage.Instance.GetDialogPage(typeof(GeneralOptionsPage));

            // If the setting is not yet assigned any value, set it to the currently connected Godot editor path
            if (string.IsNullOrEmpty(options.GodotExecutablePath))
            {
                string godotPath = GodotMessagingClient?.GodotEditorExecutablePath;
                if (!string.IsNullOrEmpty(godotPath) && File.Exists(godotPath))
                    options.GodotExecutablePath = godotPath;
            }
        }

        private void DebuggerEvents_OnEnterDesignMode(dbgEventReason reason)
        {
            if (reason != dbgEventReason.dbgEventReasonStopDebugging)
                return;

            if (GodotMessagingClient == null || !GodotMessagingClient.IsConnected)
                return;

            var currentDebugTarget = GodotDebugTargetSelection.Instance.CurrentDebugTarget;

            if (currentDebugTarget != null && currentDebugTarget.ExecutionType == ExecutionType.PlayInEditor)
                _ = GodotMessagingClient.SendRequest<StopPlayResponse>(new StopPlayRequest());
        }

        private void Close()
        {
            if (GodotMessagingClient != null)
            {
                ServiceContainer.RemoveService(typeof(Client));
                GodotMessagingClient.Dispose();
                GodotMessagingClient = null;
            }

            if (DebuggerEvents != null)
            {
                DebuggerEvents.OnEnterDesignMode -= DebuggerEvents_OnEnterDesignMode;
                DebuggerEvents = null;
            }
        }
    }
}
