using Synapse.Command;
using Synapse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine.Android;
using UnityEngine;

namespace SCPSLEnforcedRNG.Commands
{
    [CommandInformation(
        Name = "DebugSpawnSchematicInRoom",
        Aliases = new string[] { "SpSchR" },
        Description = "Spawns a schematic relative to Room Position",
        Permission = "SCPSLEnforcedRNG.SpawnSchematic",
        Platforms = new[] { Platform.ClientConsole, Platform.RemoteAdmin },
        Usage = "",
        Arguments = new[] { "SchematicID", "PosX", "PosY", "PosZ", "RotX = 0", "RotY = 0", "RotZ = 0" }
        )]

    public class DebugSpawnSchematicInRoomCommand : ISynapseCommand
    {
        private PluginClass Plugin { get; }
        public DebugSpawnSchematicInRoomCommand(PluginClass plugin) => Plugin = plugin;

        public CommandResult Execute(CommandContext context)
        {
            var room = context.Player.Room;
            Vector3 pos = new(float.Parse(context.Arguments.ElementAt(1)), float.Parse(context.Arguments.ElementAt(2)), float.Parse(context.Arguments.ElementAt(3)));
            Vector3 rot = new(0f, 0f, 0f);
            if (context.Arguments.Count==7)
            { 
                rot.x = float.Parse(context.Arguments.ElementAt(4)); 
                rot.y = float.Parse(context.Arguments.ElementAt(5)); 
                rot.z = float.Parse(context.Arguments.ElementAt(6)); 
            }

            Modules.SchematicsModule.CreateSchematicInRoom(room, int.Parse(context.Arguments.ElementAt(0)), pos, rot);

            DebugTranslator.Console("Spawned Schematic at x" + pos.x + " y" + pos.y + " z" + pos.z + " in " + room.RoomName);
            string output = "Spawned Schematic " + context.Arguments.ElementAt(0);

            var result = new CommandResult();
            result.Message = output;
            DebugTranslator.Console(output);
            result.State = CommandResultState.Ok;
            return result;
        }
    }
}
