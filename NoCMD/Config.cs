using System.Collections.Generic;
using NoCMD.Extensions;

namespace NoCMD
{
    public class Config
    {
        private static readonly Dictionary<string, string> Switches = new Dictionary<string, string>
        {
            { "", nameof(Command) },
            { "/o", nameof(OutFileName) },
            { "/out", nameof(OutFileName) },
            { "/w", nameof(Wait) },
            { "/wait", nameof(Wait) }
        };

        private static readonly Dictionary<string, bool> IsValueNeeded = new Dictionary<string, bool>
        {
            { nameof(Command), true },
            { nameof(OutFileName), true },
            { nameof(Wait), false }
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
                OutFileName = values.TryGetValue(nameof(OutFileName), null),
                Wait = values.ContainsKey(nameof(Wait))
            };
        }

        public string Command { get; private set; }

        public string OutFileName { get; private set; }

        public bool Wait { get; private set; }
    }
}