using Synapse.Command;

namespace SCPSLEnforcedRNG
{
    [CommandInformation(
        Name = "PlayerListView", // The main name and parameter for your command
        Aliases = new string[] { "plv" }, // Aliases you can use instead of main command name
        Description = "Prints all players", // A Description for the Commad
        Permission = "", // The permission which the player needs to execute the Command
        Platforms = new[] { Platform.ServerConsole, Platform.ClientConsole, Platform.RemoteAdmin }, // The platforms the command can be used
        Usage = ".plv", // A message how to use the command
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
            string tempText = "\nSCP|PC |Dcl|Sci|Grd|Player\n";
            foreach (var player in GameTech.playerList)
                tempText += 
                    player.NotSCP.ToString("D3")         + "|" +
                    player.NotPC.ToString("D3")          + "|" +
                    player.NotDboi.ToString("D3")        + "|" +
                    player.NotScientist.ToString("D3")   + "|" +
                    player.NotGuard.ToString("D3")       + "|" +
                    player.Name + "@" + player.PlayerId + "\n";

            result.Message = tempText;
            result.State = CommandResultState.Ok;
            return result;
        }
    }
}
