using SCPSLEnforcedRNG.Modules;
using Synapse.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine.Android;

namespace SCPSLEnforcedRNG.Commands
{
    [CommandInformation(
        Name = "DebugTestRandom",
        Aliases = new string[] { "" },
        Description = "Tests out RNG",
        Permission = "",
        Platforms = new[] { Platform.ServerConsole},
        Usage = "",
        Arguments = new[] { "MIN", "MAX", "TESTS" }
        )]

    public class TestRandomness : ISynapseCommand
    {
        private PluginClass Plugin { get; }
        public TestRandomness(PluginClass plugin) => Plugin = plugin;

        public CommandResult Execute(CommandContext context)
        {
            if (context.Arguments.Count == 0)
                MainModule.TestRandom();
            else if (context.Arguments.Count == 1)
                MainModule.TestRandom(int.Parse(context.Arguments.ElementAt(0)));
            else if (context.Arguments.Count == 2)
                MainModule.TestRandom(int.Parse(context.Arguments.ElementAt(0)), int.Parse(context.Arguments.ElementAt(1)));
            else if (context.Arguments.Count == 3)
                MainModule.TestRandom(int.Parse(context.Arguments.ElementAt(0)), int.Parse(context.Arguments.ElementAt(1)), int.Parse(context.Arguments.ElementAt(2)));

            var result = new CommandResult();
            result.Message = "Testing RNG Finished";
            result.State = CommandResultState.Ok;
            return result;
        }
    }
}
