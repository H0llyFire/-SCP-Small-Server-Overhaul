using MEC;
using Synapse;
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
        Name = "ForceRespawns", // The main name and parameter for your command
        Aliases = new string[] { "FR" }, // Aliases you can use instead of main command name
        Description = "Forces a Random Wave to respawn", // A Description for the Commad
        Permission = "admin", // The permission which the player needs to execute the Command
        Platforms = new[] { Platform.RemoteAdmin, Platform.ServerConsole }, // The platforms the command can be used
        Usage = ".FR", // A message how to use the command
        Arguments = new[] { "" } //The Arguments that the will be displayed in the 
        //RemoteAdmin(only) to help the user to understand how to execute the command
        )]
    public class ForceRespawnsCommand : ISynapseCommand
    {
        private PluginClass Plugin { get; }
        public ForceRespawnsCommand(PluginClass plugin) => Plugin = plugin;

        public CommandResult Execute(CommandContext context)
        {
            bool isChaos = UnityEngine.Random.Range(0f, 1f) + PluginClass.ServerConfigs.chaosChance > 1f;
            var result = new CommandResult();

            result.Message = "Initiating Respawn Sequence";

            Round.Get.SpawnVehicle(isChaos);
            Timing.CallDelayed(10f, () => Round.Get.MtfRespawn(isChaos));


            result.State = CommandResultState.Ok;

            return result;
        }
    }
}
