﻿using Synapse.Api;
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
                tempResultText = 
                    "Changable configs available: \n" +

                    "ShowDebugInConsole {bool}\n" +
                    "-Whether the Console should show Debug info\n" +

                    "RolePicks {string}\n" +
                    "-Role order that will be picked on round start\n" +
                    "- 0 => SCP\n" +
                    "- 1 => PC\n" +
                    "- 2 => Guard\n" +
                    "- 3 => D-Class\n" +
                    "- 4 => Scientist\n" +

                    "ChaosChance {float}\n" +
                    "- The chance from 0 to 1 for chaos to spawn instead of MTF\n" +

                    "RespawnTime {int}\n" +
                    "- How long does it take for respawns to occur\n" +

                    "RespawnTimeRange {int}\n" +
                    "- The Max time added or removed from the base respawn for a random effect\n" +

                    "LightsOutMode {bool}\n" +
                    "- If all lights should be off on round start\n" +

                    "StartingLightsOff {int}\n" +
                    "- Amount of Lights to be set off at RoundStart\n" +

                    "GeneratorLightsOn {int}\n" +
                    "- Amount of Lights that get switched on when a generator is turned on";
            }
            else
            {
                switch(context.Arguments.ElementAt(0))
                {
                    case "RolePicks":
                        if (context.Arguments.Count == 1) tempResultText = Modules.MainModule.ServerConfigs.RolePicks;
                        else if (context.Arguments.ElementAt(1).ToLower() == "default") Modules.MainModule.ServerConfigs.RolePicks = "303242334312303432";
                        else
                        {
                            IEnumerable<char> allowedInputs = "01234";
                            if (context.Arguments.ElementAt(1).All(allowedInputs.Contains)) Modules.MainModule.ServerConfigs.RolePicks = context.Arguments.ElementAt(1);
                            else tempResultText = "Invalid Value";
                        }
                        break;

                    case "ChaosChance":
                        if (context.Arguments.Count == 1) tempResultText = Modules.MainModule.ServerConfigs.chaosChance.ToString();
                        else if (context.Arguments.ElementAt(1).ToLower() == "default") Modules.MainModule.ServerConfigs.chaosChance = 0.25f;
                        else
                        {
                            float newValue;
                            try { newValue = float.Parse(context.Arguments.ElementAt(1)); }
                            catch
                            {
                                tempResultText = "Invalid Value";
                                break;
                            }
                            if (newValue >= 0f && newValue <= 1f) Modules.MainModule.ServerConfigs.chaosChance = newValue;
                            else tempResultText = "Invalid Value";
                        }
                        break;

                    case "RespawnTime":
                        if (context.Arguments.Count == 1) tempResultText = Modules.MainModule.ServerConfigs.respawnTime.ToString();
                        else if (context.Arguments.ElementAt(1).ToLower() == "default") Modules.MainModule.ServerConfigs.respawnTime = 420;
                        else
                        {
                            int newValue;
                            try { newValue = int.Parse(context.Arguments.ElementAt(1)); }
                            catch
                            {
                                tempResultText = "Invalid Value";
                                break;
                            }
                            Modules.MainModule.ServerConfigs.respawnTime = newValue;
                        }
                        break;

                    case "RespawnTimeRange":
                        if (context.Arguments.Count == 1) tempResultText = Modules.MainModule.ServerConfigs.respawnTimeRange.ToString();
                        else if (context.Arguments.ElementAt(1).ToLower() == "default") Modules.MainModule.ServerConfigs.respawnTimeRange = 30;
                        else
                        {
                            int newValue;
                            try { newValue = int.Parse(context.Arguments.ElementAt(1)); }
                            catch
                            {
                                tempResultText = "Invalid Value";
                                break;
                            }
                            Modules.MainModule.ServerConfigs.respawnTimeRange = newValue;
                        }
                        break;

                    case "LightsOutMode":
                        if (context.Arguments.Count == 1) tempResultText = Modules.MainModule.ServerConfigs.LightsOutMode.ToString();
                        else if (context.Arguments.ElementAt(1).ToLower() == "default") Modules.MainModule.ServerConfigs.LightsOutMode = false;
                        else
                        {
                            bool newValue;
                            try { newValue = bool.Parse(context.Arguments.ElementAt(1)); }
                            catch
                            {
                                tempResultText = "Invalid Value";
                                break;
                            }
                            Modules.MainModule.ServerConfigs.LightsOutMode = newValue;
                        }
                        break;

                    case "StartingLightsOff":
                        if (context.Arguments.Count == 1) tempResultText = Modules.MainModule.ServerConfigs.StartingLightsOff.ToString();
                        else if (context.Arguments.ElementAt(1).ToLower() == "default") Modules.MainModule.ServerConfigs.StartingLightsOff = 12;
                        else
                        {
                            int newValue;
                            try { newValue = int.Parse(context.Arguments.ElementAt(1)); }
                            catch
                            {
                                tempResultText = "Invalid Value";
                                break;
                            }
                            Modules.MainModule.ServerConfigs.StartingLightsOff = newValue;
                        }
                        break;

                    case "GeneratorLightsOn":
                        if (context.Arguments.Count == 1) tempResultText = Modules.MainModule.ServerConfigs.GeneratorLightsOn.ToString();
                        else if (context.Arguments.ElementAt(1).ToLower() == "default") Modules.MainModule.ServerConfigs.GeneratorLightsOn = 6;
                        else
                        {
                            int newValue;
                            try { newValue = int.Parse(context.Arguments.ElementAt(1)); }
                            catch
                            {
                                tempResultText = "Invalid Value";
                                break;
                            }
                            Modules.MainModule.ServerConfigs.GeneratorLightsOn = newValue;
                        }
                        break;

                    default:
                        tempResultText = "Invalid Option.";
                        break;
                }
            }

            if (tempResultText == "") tempResultText = "Value Succesfully Changed";

            tempResultText += "\nArgs: " + context.Arguments.Count;
            
            var result = new CommandResult();
            result.Message = DebugTranslator.TranslatePrefix(tempResultText);
            result.State = CommandResultState.Ok;
            return result;
        }
    }
}
