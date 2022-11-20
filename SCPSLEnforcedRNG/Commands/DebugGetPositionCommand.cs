using MapGeneration;
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
        Name = "DebugGetPosition",
        Aliases = new string[] { "GetPos", "GP" },
        Description = "Prints your position",
        Permission = "SCPSLEnforcedRNG.GetPosition",
        Platforms = new[] { Platform.ClientConsole, Platform.RemoteAdmin },
        Usage = ".SH",
        Arguments = new[] { "" }
        )]

    public class DebugGetPositionCommand : ISynapseCommand
    {
        private PluginClass Plugin { get; }
        public DebugGetPositionCommand(PluginClass plugin) => Plugin = plugin;

        public CommandResult Execute(CommandContext context)
        {
            var pos = context.Player.Position;
            var room = context.Player.Room;
            var zone = context.Player.Zone;
            var mapPoint = context.Player.MapPoint.Position;

            string output =
                "Position: x" + pos.x + " y" + pos.y + " z" + pos.z + "\n" +
                "Room: " + room.RoomName + "\n" +
                "Zone: " + zone + "\n" +
                "Map Point:x" + mapPoint.x + " y" + mapPoint.y + " z" + mapPoint.z;

            var result = new CommandResult();
            result.Message = output;
            DebugTranslator.Console(output);
            result.State = CommandResultState.Ok;
            return result;
        }
    }
}
