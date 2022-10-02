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
        Name = "EditPlayerInfo", // The main name and parameter for your command
        Aliases = new string[] { "EPI" }, // Aliases you can use instead of main command name
        Description = "Edits Players information", // A Description for the Commad
        Permission = "admin", // The permission which the player needs to execute the Command
        Platforms = new[] { Platform.RemoteAdmin, Platform.ServerConsole }, // The platforms the command can be used
        Usage = ".FR {NickName} {PlayerInfo} {NewValue}, If no arguments prints available Player Info Arguments", // A message how to use the command
        Arguments = new[] { "Nickname", "PlayerInfo", "Value" } //The Arguments that the will be displayed in the 
        //RemoteAdmin(only) to help the user to understand how to execute the command
        )]
    public class EditPlayerInfoCommand : ISynapseCommand
    {
        private PluginClass Plugin { get; }
        public EditPlayerInfoCommand(PluginClass plugin) => Plugin = plugin;

        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();

            if(context.Arguments.Count==0)
            {
                result.Message = 
                    "Available PlayerInfo args:\n" +
                    "{scp}, {pc}, {guard}, {scientist}, {dclass}";
                result.State = CommandResultState.Ok;
                return result;
            }
            if(!(context.Arguments.Count==3))
            {
                result.Message = "Invalid amount of args";
                result.State = CommandResultState.Error;
                return result;
            }


            var player = PlayerInfo.repository.FindByNickname(context.Arguments.ElementAt(0));
            if (player == null)
            {
                result.Message = "Invalid Player";
                result.State = CommandResultState.Error;
                return result;
            }

            string playerInfo = context.Arguments.ElementAt(1).ToLower();
            int newValue;
            try
            { newValue = int.Parse(context.Arguments.ElementAt(2)); }
            catch
            { newValue = -1; }
            if(newValue < 0)
            {
                result.Message = "Invalid Value";
                result.State = CommandResultState.Error;
                return result;
            }

            switch(playerInfo)
            {
                case "scp":
                case "notscp":
                    player.NotSCP = (uint)newValue;
                    break;
                case "pc":
                case "notpc":
                    player.NotPC = (uint)newValue;
                    break;
                case "guard":
                case "notguard":
                    player.NotGuard = (uint)newValue;
                    break;
                case "scientist":
                case "notscientist":
                    player.NotScientist = (uint)newValue;
                    break;
                case "dboi":
                case "notdboi":
                case "dclass":
                case "notdclass":
                case "d-class":
                case "notd-class":
                    player.NotDboi = (uint)newValue;
                    break;
                default:
                    result.Message = "Invalid Player Info Argument";
                    result.State = CommandResultState.Error;
                    return result;
            }

            result.Message = "Successfully changed value.";
            result.State = CommandResultState.Ok;

            return result;
        }
    }
}
