namespace KLOC
{
    internal class Arguments
    {
        public string ProjectDirectory { get; private set; }
        public bool IsContainer { get; private set; }
        public bool IsHelp { get; private set; }

        internal static Arguments Parse(string[] args)
        {
            var result = new Arguments();
            foreach (var arg in args)
            {
                if (arg[0] == '-' || arg[0] == '/')
                {
                    switch (arg.ToLowerInvariant().Substring(1))
                    {
                        case "?":
                            result.IsHelp = true;
                            break;
                        case "c":
                            result.IsContainer = true;
                            break;
                        default:
                            return null;
                    }
                    continue;
                }

                result.ProjectDirectory = arg;
            }

            return result;
        }
    }
}
