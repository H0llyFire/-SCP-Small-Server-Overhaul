using MEC;
using Synapse.Api;
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
        Name = "RespawnTime", // The main name and parameter for your command
        Aliases = new string[] { "Respawns", "RT" }, // Aliases you can use instead of main command name
        Description = "Prints when the next MTF or Chaos Respawn is", // A Description for the Commad
        Permission = "SCPSLEnforcedRNG.RespawnTime", // The permission which the player needs to execute the Command
        Platforms = new[] { Platform.ServerConsole, Platform.ClientConsole, Platform.RemoteAdmin }, // The platforms the command can be used
        Usage = ".Respawns", // A message how to use the command
        Arguments = new[] { "" } //The Arguments that the will be displayed in the 
        //RemoteAdmin(only) to help the user to understand how to execute the command
        )]
    public class RespawnTimeCommand : ISynapseCommand
    {
        private PluginClass Plugin { get; }
        public RespawnTimeCommand(PluginClass plugin) => Plugin = plugin;

        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();
            result.Message = DebugTranslator.TranslatePrefix("Time Remaining: " + (GameTech.respawnTimer-Timing.LocalTime));
            result.State = CommandResultState.Ok;
            return result;
        }
    }
}
