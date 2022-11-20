using Synapse.Command;
using Synapse;
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
        Name = "DebugClearAllSchematics",
        Aliases = new string[] { "ClSch" },
        Description = "Clears All Schematics",
        Permission = "SCPSLEnforcedRNG.ClearSchematics",
        Platforms = new[] { Platform.ClientConsole, Platform.RemoteAdmin, Platform.ServerConsole },
        Usage = "ClSch",
        Arguments = new[] { "" }
        )]

    public class DebugClearAllSchematicsCommand : ISynapseCommand
    {
        private PluginClass Plugin { get; }
        public DebugClearAllSchematicsCommand(PluginClass plugin) => Plugin = plugin;

        public CommandResult Execute(CommandContext context)
        {
            foreach(var schematic in Modules.SchematicsModule.schematics)
            {
                schematic.Destroy();
            }
            Modules.SchematicsModule.schematics.Clear();

            string output = "Cleared all schematics";

            var result = new CommandResult();
            result.Message = output;
            DebugTranslator.Console(output);
            result.State = CommandResultState.Ok;
            return result;
        }
    }
}
