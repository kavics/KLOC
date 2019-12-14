using System;
using System.Linq;

namespace KLOC
{
    internal class Arguments
    {
        public string ProjectDirectory { get; private set; }
        public bool IsContainer { get; private set; }
        public bool IsHelp { get; private set; }
        public string[] EnabledFileExtensions { get; private set; }

        internal static Arguments Parse(string[] args, out string message)
        {
            var result = new Arguments();
            foreach (var arg in args)
            {
                if (arg[0] == '-' || arg[0] == '/')
                {
                    var argName = arg.ToLowerInvariant().Substring(1);

                    if (argName.StartsWith("ext:"))
                    {
                        if (argName.Length <= 4)
                        {
                            message = "Missing extension list after -ext:";
                            return null;
                        }
                        result.EnabledFileExtensions = ParseFileExtensions(argName.Substring(4));
                        continue;
                    }
                    switch (argName)
                    {
                        case "?":
                            result.IsHelp = true;
                            break;
                        case "c":
                            result.IsContainer = true;
                            break;
                        default:
                            message = "Unknown argument: " + arg;
                            return null;
                    }
                    continue;
                }

                result.ProjectDirectory = arg;
            }

            message = null;
            return result;
        }

        private static string[] ParseFileExtensions(string list)
        {
            return list.Split(new[] {','}, StringSplitOptions.None)
                .Select(x => x.Trim().ToLowerInvariant())
                .ToArray();
        }
    }
}
