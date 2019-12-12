namespace KLOC
{
    internal class Arguments
    {
        public string ProjectDirectory { get; private set; }
        public bool IsContainer { get; private set; }
        public bool WriteDeatails { get; private set; }
        public bool WritePath { get; private set; }
        public bool SimpleCount { get; private set; }
        public bool IsHelp { get; private set; }

        internal static Arguments Parse(string[] args)
        {
            var result = new Arguments {WriteDeatails = true};
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
                            result.WriteDeatails = false;
                            break;
                        case "k":
                            result.SimpleCount = false;
                            result.WritePath = false;
                            result.WriteDeatails = false;
                            break;
                        case "l":
                            result.SimpleCount = true;
                            result.WritePath = false;
                            result.WriteDeatails = false;
                            break;
                        case "kp":
                            result.SimpleCount = false;
                            result.WritePath = true;
                            result.WriteDeatails = false;
                            break;
                        case "lp":
                            result.SimpleCount = true;
                            result.WritePath = true;
                            result.WriteDeatails = false;
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
