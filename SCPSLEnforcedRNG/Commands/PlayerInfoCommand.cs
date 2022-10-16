using Synapse.Command;

namespace SCPSLEnforcedRNG
{
    [CommandInformation(
        Name = "PlayerInfo", 
        Aliases = new string[] { "pi" }, 
        Description = "Prints your Player Info", 
        Permission = "", 
        Platforms = new[] { Platform.ClientConsole },
        Usage = ".pi",
        Arguments = new[] { "" }
        )]
    public class PlayerInfoCommand : ISynapseCommand
    {
        private PluginClass Plugin { get; }
        public PlayerInfoCommand(PluginClass plugin) => Plugin = plugin;

        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();
            
            foreach (var player in GameTech.playerList)
                if (player.PlayerId == context.Player.UserId) result.Message = player.PrintInfo();
            result.State = CommandResultState.Ok;

            return result;
        }
    }
}
