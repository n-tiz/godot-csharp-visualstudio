using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace GodotAddinVS
{
    public class GeneralOptionsPage : DialogPage
    {
        [Category("Debugging")]
        [DisplayName("Always Use Configured Executable")]
        [Description("When disabled, Visual Studio will attempt to get the Godot executable path from a running Godot editor instance")]
        public bool AlwaysUseConfiguredExecutable { get; set; } = false;

        [Category("Debugging")]
        [DisplayName("Godot Executable Path")]
        [Description("Path to the Godot executable to use when launching the application for debugging")]
        public string GodotExecutablePath { get; set; } = "";

        [Category("Debugging")]
        [DisplayName("Debugger Listen Timeout")]
        [Description("Time in milliseconds after which the debugging session will end if no debugger is connected")]
        public int DebuggerListenTimeout { get; set; } = 10000;

    }
}
