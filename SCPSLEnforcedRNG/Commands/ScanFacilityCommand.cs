using MEC;
using Synapse.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine.Android;

namespace SCPSLEnforcedRNG
{
    [CommandInformation(
        Name = "ScanFacility",
        Aliases = new string[] { "SF" },
        Description = "Starts a Facility Scan",
        Permission = "SCPSLEnforcedRNG.FacilityScan",
        Platforms = new[] { Platform.ServerConsole, Platform.RemoteAdmin },
        Usage = ".ScanFacility",
        Arguments = new[] { "" }
        )]
    
    public class ScanFacilityCommand : ISynapseCommand
    {
        private PluginClass Plugin { get; }
        public ScanFacilityCommand(PluginClass plugin) => Plugin = plugin;

        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();
            result.Message = "Scan Initiated.";
            GameTech.ScanFacility();
            result.State = CommandResultState.Ok;
            return result;
        }
    }
    
}
