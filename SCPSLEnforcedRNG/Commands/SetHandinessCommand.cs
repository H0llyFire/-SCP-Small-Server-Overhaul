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
        Name = "SetHand",
        Aliases = new string[] { "SH" },
        Description = "Switches your primary hand",
        Permission = "",
        Platforms = new[] { Platform.ClientConsole, Platform.RemoteAdmin },
        Usage = ".SH",
        Arguments = new[] { "" }
        )]

    public class SetHandinessCommand : ISynapseCommand
    {
        private PluginClass Plugin { get; }
        public SetHandinessCommand(PluginClass plugin) => Plugin = plugin;

        public CommandResult Execute(CommandContext context)
        {
            var size = context.Player.Scale;
            size.x *= -1;
            context.Player.Scale = size;

            var result = new CommandResult();
            result.Message = "Switched Hand. " + context.Player.Scale;
            result.State = CommandResultState.Ok;
            return result;
        }
    }
}
