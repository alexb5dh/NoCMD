using System.Collections.Generic;
using NoCMD.Extensions;

namespace NoCMD
{
    public class Config
    {
        private static readonly Dictionary<string, string> Switches = new Dictionary<string, string>
        {
            { "", nameof(Command) },

            { "/w", nameof(Wait) },
            { "/wait", nameof(Wait) },

            { "/o", nameof(OutFileName) },
            { "/out", nameof(OutFileName) },

            { "/e", nameof(ErrorFileName) },
            { "/error", nameof(ErrorFileName) }
        };

        private static readonly Dictionary<string, bool> IsValueNeeded = new Dictionary<string, bool>
        {
            { nameof(Command), true },
            { nameof(Wait), false },
            { nameof(OutFileName), true },
            { nameof(ErrorFileName), true }
        };

        public static Config ParseCommandLine(string[] args)
        {
            string valueName = null;
            var values = new Dictionary<string, string>();

            // Todo: refactor ParseCommandLine loop
            foreach (var arg in args)
            {
                if (valueName != null)
                {
                    values[valueName] = arg;
                    valueName = null;
                }

                else if (Switches.ContainsKey(arg))
                {
                    valueName = Switches[arg];

                    if (!IsValueNeeded[valueName])
                    {
                        values[valueName] = valueName;
                        valueName = null;
                    }
                }

                else
                {
                    values[Switches[""]] = arg;
                }
            }

            return new Config()
            {
                Command = values[nameof(Command)],
                Wait = values.ContainsKey(nameof(Wait)),
                OutFileName = values.TryGetValue(nameof(OutFileName), null),
                ErrorFileName = values.TryGetValue(nameof(ErrorFileName), null)
            };
        }

        public string Command { get; private set; }

        public string OutFileName { get; private set; }

        public string ErrorFileName { get; private set; }

        public bool Wait { get; private set; }
    }
}