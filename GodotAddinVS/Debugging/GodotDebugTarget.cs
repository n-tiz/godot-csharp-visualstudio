using System;

namespace GodotAddinVS.Debugging
{
    public enum ExecutionType : uint
    {
        PlayInEditor = 0,
        Launch,
        Attach
    }

    public class GodotDebugTarget
    {
        public static readonly Guid DebugTargetsGuid = new Guid("4E50788E-B023-4F77-AFE9-797603876907");

        public Guid Guid => DebugTargetsGuid;

        public uint Id { get; }

        public string Name { get; }

        public ExecutionType ExecutionType { get; }

        public GodotDebugTarget(ExecutionType executionType, string name)
        {
            Id = 0x8192 + (uint) executionType;
            ExecutionType = executionType;
            Name = name;
        }
    }

}
