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
        Name = "EditConfigs", // The main name and parameter for your command
        Aliases = new string[] { "EC" }, // Aliases you can use instead of main command name
        Description = "Edits a config", // A Description for the Commad
        Permission = "SCPSLEnforcedRNG.EditConfigs", // The permission which the player needs to execute the Command
        Platforms = new[] { Platform.ServerConsole, Platform.RemoteAdmin }, // The platforms the command can be used
        Usage = ".EC {option} {value}", // A message how to use the command
        Arguments = new[] { "option", "value" } //The Arguments that the will be displayed in the 
        //RemoteAdmin(only) to help the user to understand how to execute the command
        )]
    public class EditConfigsCommand : ISynapseCommand
    {
        private PluginClass Plugin { get; }
        public EditConfigsCommand(PluginClass plugin) => Plugin = plugin;

        public CommandResult Execute(CommandContext context)
        {
            string tempResultText = "";
            if (context.Arguments.Count == 0)
            {
                tempResultText = "\n[EnforcedRNG]: Changable configs available: \n" +
                    "[EnforcedRNG]: RolePicks {string}\n" +
                    "[EnforcedRNG]: -Role order that will be picked on round start\n" +
                    "[EnforcedRNG]: - 0 => SCP\n" +
                    "[EnforcedRNG]: - 1 => PC\n" +
                    "[EnforcedRNG]: - 2 => Guard\n" +
                    "[EnforcedRNG]: - 3 => D-Class\n" +
                    "[EnforcedRNG]: - 4 => Scientist\n" +
                    "[EnforcedRNG]: ChaosChance {float}\n" +
                    "[EnforcedRNG]: - The chance from 0 to 1 for chaos to spawn instead of MTF\n" +
                    "[EnforcedRNG]: RespawnTime {int}\n" +
                    "[EnforcedRNG]: - How long does it take for respawns to occur\n" +
                    "[EnforcedRNG]: RespawnTimeRange {int}\n" +
                    "[EnforcedRNG]: - The Max time added or removed from the base respawn for a random effect\n" +
                    "[EnforcedRNG]: LightsOutMode {bool}\n" +
                    "[EnforcedRNG]: - If all lights should be off on round start\n" +
                    "[EnforcedRNG]: StartingLightsOff {int}\n" +
                    "[EnforcedRNG]: - Amount of Lights to be set off at RoundStart. Should be dividable by 3 {Otherwise Undefined Behavior}";
            }
            else
            {
                switch(context.Arguments.ElementAt(0))
                {
                    case "RolePicks":
                        if (context.Arguments.Count == 1) tempResultText = "\n[EnforcedRNG]: " + PluginClass.ServerConfigs.RolePicks;
                        else
                        {
                            IEnumerable<char> allowedInputs = "01234";
                            if(context.Arguments.ElementAt(1).All(allowedInputs.Contains)) PluginClass.ServerConfigs.RolePicks = context.Arguments.ElementAt(1);
                            else tempResultText = "\n[EnforcedRNG]: Invalid Value";
                        }
                        break;
                    case "ChaosChance":
                        if (context.Arguments.Count == 1) tempResultText = "\n[EnforcedRNG]: " + PluginClass.ServerConfigs.chaosChance.ToString();
                        else
                        {
                            float newValue;
                            try { newValue = float.Parse(context.Arguments.ElementAt(1)); }
                            catch
                            {
                                tempResultText = "\n[EnforcedRNG]: Invalid Value";
                                break;
                            }
                            if (newValue >= 0f && newValue <= 1f) PluginClass.ServerConfigs.chaosChance = newValue;
                            else tempResultText = "\n[EnforcedRNG]: Invalid Value";
                        }
                        break;
                    case "RespawnTime":
                        if (context.Arguments.Count == 1) tempResultText = "\n[EnforcedRNG]: " + PluginClass.ServerConfigs.respawnTime.ToString();
                        else
                        {
                            int newValue;
                            try { newValue = int.Parse(context.Arguments.ElementAt(1)); }
                            catch
                            {
                                tempResultText = "\n[EnforcedRNG]: Invalid Value";
                                break;
                            }
                            PluginClass.ServerConfigs.respawnTime = newValue;
                        }
                        break;
                    case "RespawnTimeRange":
                        if (context.Arguments.Count == 1) tempResultText = "\n[EnforcedRNG]: " + PluginClass.ServerConfigs.respawnTimeRange.ToString();
                        else
                        {
                            int newValue;
                            try { newValue = int.Parse(context.Arguments.ElementAt(1)); }
                            catch
                            {
                                tempResultText = "\n[EnforcedRNG]: Invalid Value";
                                break;
                            }
                            PluginClass.ServerConfigs.respawnTimeRange = newValue;
                        }
                        break;
                    case "LightsOutMode":
                        if (context.Arguments.Count == 1) tempResultText = "\n[EnforcedRNG]: " + PluginClass.ServerConfigs.LightsOutMode.ToString();
                        else
                        {
                            bool newValue;
                            try { newValue = bool.Parse(context.Arguments.ElementAt(1)); }
                            catch
                            {
                                tempResultText = "\n[EnforcedRNG]: Invalid Value";
                                break;
                            }
                            PluginClass.ServerConfigs.LightsOutMode = newValue;
                        }
                        break;
                    case "StartingLightsOff":
                        if (context.Arguments.Count == 1) tempResultText = "\n[EnforcedRNG]: " + PluginClass.ServerConfigs.StartingLightsOff.ToString();
                        else
                        {
                            int newValue;
                            try { newValue = int.Parse(context.Arguments.ElementAt(1)); }
                            catch
                            {
                                tempResultText = "\n[EnforcedRNG]: Invalid Value";
                                break;
                            }
                            PluginClass.ServerConfigs.StartingLightsOff = newValue;
                        }
                        break;
                    default:
                        tempResultText = "\n[EnforcedRNG]: Invalid Option.";
                        break;
                }
            }

            if (tempResultText == "") tempResultText = "[EnforcedRNG]: Value Succesfully Changed";

            tempResultText += "\n[EnforcedRNG]: Args: " + context.Arguments.Count;
            
            var result = new CommandResult();
            result.Message = tempResultText;
            result.State = CommandResultState.Ok;
            return result;
        }
    }
}
