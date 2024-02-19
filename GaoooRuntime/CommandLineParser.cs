using CommandLine;
using System;
using System.IO;

namespace GaoooRuntime
{
    internal class CommandLineParser
    {
        public static CommandLineArgs Parse()
        {
            var args = Environment.GetCommandLineArgs();

            try
            {
                var result = Parser.Default.ParseArguments<CommandLineArgs>(args).WithParsed((CommandLineArgs args) =>
                {
                    if (string.IsNullOrEmpty(args.WorkSpace))
                    {
                        args.WorkSpace = "./data";
                    }

                    if (!Directory.Exists(args.WorkSpace))
                    {
                        throw new ArgumentException($"ERROR: {args.WorkSpace} does not exist.");
                    }
                });

                return result.Value;
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}
