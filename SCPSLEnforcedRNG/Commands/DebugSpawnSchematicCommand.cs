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

namespace SCPSLEnforcedRNG.Commands
{
    [CommandInformation(
        Name = "DebugSpawnSchematic",
        Aliases = new string[] { "SSCh" },
        Description = "Spawns a Schematic on your position",
        Permission = "SCPSLEnforcedRNG.SpawnSchematic",
        Platforms = new[] { Platform.ClientConsole, Platform.RemoteAdmin },
        Usage = "SCh SchematicIDint RotationXfloat RotationYfloat RotationZfloat",
        Arguments = new[] { "SchematicIDint", "RotationXfloat", "RotationYfloat", "RotationZfloat" }
        )]

    public class DebugSpawnSchematicCommand : ISynapseCommand
    {
        private PluginClass Plugin { get; }
        public DebugSpawnSchematicCommand(PluginClass plugin) => Plugin = plugin;

        public CommandResult Execute(CommandContext context)
        {
            var pos = context.Player.Position;

            var schematic = Server.Get.Schematic.SpawnSchematic(int.Parse(context.Arguments.ElementAt(0)), pos, new UnityEngine.Vector3(float.Parse(context.Arguments.ElementAt(1)), float.Parse(context.Arguments.ElementAt(2)), float.Parse(context.Arguments.ElementAt(3))));
            Modules.SchematicsModule.schematics.Add(schematic);


            string output = "Spawned Schematic " + context.Arguments.ElementAt(0);

            var result = new CommandResult();
            result.Message = output;
            DebugTranslator.Console(output);
            result.State = CommandResultState.Ok;
            return result;
        }
    }
}
