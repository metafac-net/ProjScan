using Spectre.Console;
using System;

namespace ProjScan
{
    internal class Options
    {
        public bool Parsed { get; }
        public string PathRoot { get; } = ".";
        public bool Unattended { get; }

        public Options(string[] args)
        {
            try
            {
                int argn = 0;
                while (argn < args.Length)
                {
                    string arg = args[argn];
                    if (string.Equals(arg, "-u"))
                    {
                        Unattended = true;
                    }
                    else if (string.Equals(arg, "-p"))
                    {
                        argn++;
                        PathRoot = args[argn];
                    }
                    else
                    {
                        throw new ArgumentException("Unknown argument", arg);
                    }

                    argn++;
                }

                // success
                Parsed = true;
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
        }

    }
}