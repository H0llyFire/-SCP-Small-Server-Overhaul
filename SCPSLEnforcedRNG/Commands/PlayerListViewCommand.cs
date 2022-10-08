using Synapse.Command;

namespace SCPSLEnforcedRNG
{
    [CommandInformation(
        Name = "PlayerListView", // The main name and parameter for your command
        Aliases = new string[] { "plv" }, // Aliases you can use instead of main command name
        Description = "Prints all players", // A Description for the Commad
        Permission = "", // The permission which the player needs to execute the Command
        Platforms = new[] { Platform.ServerConsole }, // The platforms the command can be used
        Usage = "Don't", // A message how to use the command
        Arguments = new[] { "" } //The Arguments that the will be displayed in the 
        //RemoteAdmin(only) to help the user to understand how to execute the command
        )]
    public class PlayerListViewCommand : ISynapseCommand
    {
        private PluginClass Plugin { get; }
        public PlayerListViewCommand(PluginClass plugin) => Plugin = plugin;

        public CommandResult Execute(CommandContext context)
        {
            var result = new CommandResult();
            string tempText = "";
            foreach (var player in GameTech.playerList)
                tempText += player.PlayerPtr.NickName + " " + player.PlayerId + " => " + player.NotSCP + "-" + player.NotGuard + "-" + player.NotDboi + "-" + player.NotScientist +"\n";

            result.Message = tempText;
            result.State = CommandResultState.Ok;
            return result;
        }
    }
}
