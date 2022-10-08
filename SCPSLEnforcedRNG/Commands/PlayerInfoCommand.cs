using Synapse.Command;

namespace SCPSLEnforcedRNG
{
    [CommandInformation(
        Name = "PlayerInfo", // The main name and parameter for your command
        Aliases = new string[] { "pi" }, // Aliases you can use instead of main command name
        Description = "Prints your Player Info", // A Description for the Commad
        Permission = "", // The permission which the player needs to execute the Command
        Platforms = new[] { Platform.ClientConsole }, // The platforms the command can be used
        Usage = "Don't", // A message how to use the command
        Arguments = new[] { "" } //The Arguments that the will be displayed in the 
        //RemoteAdmin(only) to help the user to understand how to execute the command
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
