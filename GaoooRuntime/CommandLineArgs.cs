using CommandLine;
using System;
using System.IO;

namespace GaoooRuntime
{
    public class CommandLineArgs
    {
        [Option("ws", Required = false)]
        public string WorkSpace { get; set; }
    }
}
