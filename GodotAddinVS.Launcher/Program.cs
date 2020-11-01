using System;
using System.Diagnostics;
using System.Linq;
using GodotAddinVS.Debugging;

namespace GodotAddinVS.Launcher
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Debug.WriteLine("Invalid number of arguments, expected 1");
                return;
            }

            switch (args.Single())
            {
                case "PlayInEditor":
                    break;
                case "Launch":
                    break;
                case "Attach":
                    break;
                default:
                    Debug.WriteLine("Invalid argument, expected PlayInEditor, Launch or Attach");
                    return;
            }

            Console.WriteLine(args.Single());
            Enum.TryParse(args.Single(), out ExecutionType argsAsEnum);

            AddinPipe pipe = new AddinPipe(argsAsEnum);
            pipe.Start();


            Console.ReadLine();
            pipe.Dispose();
        }
    }
}
