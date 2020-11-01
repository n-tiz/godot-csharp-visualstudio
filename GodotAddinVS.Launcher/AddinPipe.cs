using System;
using System.IO;
using System.IO.Pipes;
using GodotAddinVS.Debugging;

namespace GodotAddinVS.Launcher
{
    public class AddinPipe : IDisposable
    {
        private readonly ExecutionType _execution;
        private StreamWriter _streamWriter;
        private NamedPipeClientStream _pipeClient;

        public AddinPipe(ExecutionType execution)
        {
            _execution = execution;
        }

        public void Start()
        {
            _pipeClient = new NamedPipeClientStream(".", GodotPackage.PackageGuidString, PipeDirection.Out);
            _pipeClient.Connect();
            _streamWriter = new StreamWriter(_pipeClient)
            {
                AutoFlush = true
            };
            _streamWriter.WriteLine(_execution.ToString());
            _pipeClient.WaitForPipeDrain();
        }

        public void Dispose()
        {
            _streamWriter?.Close();
            _streamWriter?.Dispose();
            _pipeClient?.Close();
            _pipeClient?.Dispose();
        }
    }
}