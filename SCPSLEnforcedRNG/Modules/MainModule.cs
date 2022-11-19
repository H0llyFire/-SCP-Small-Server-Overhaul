using MEC;
using Synapse.Api.Events.SynapseEventArguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLEnforcedRNG.Modules
{
    public class MainModule : BaseModule
    {
        //-------------------------------------------------------------------------------
        //Overrides
        public override string ModuleName { get { return "MainModule"; } }
        public override void Activate()
        {
            ServerConfigs = PluginClass.ServerConfigs;
            SynapseController.Server.Events.Round.RoundStartEvent += StartRound;
            SynapseController.Server.Events.Player.PlayerJoinEvent += CommitPlayer;
            SynapseController.Server.Events.Player.PlayerLeaveEvent += RemovePlayer;
            SynapseController.Server.Events.Round.RoundEndEvent += ClearPlayers;
            SynapseController.Server.Events.Round.RoundRestartEvent += ClearPlayers;
            
            ReadyModules.Add(this);
            if (ServerConfigs.BetterRolePicksModuleActive) ReadyModules.Add(new BetterRolePicks());
            if (ServerConfigs.BetterRespawnModuleActive) ReadyModules.Add(new BetterRespawns());
            if (ServerConfigs.MoreGeneratorFunctionsModuleActive) ReadyModules.Add(new MoreGeneratorFunctions());
            if (ServerConfigs.AntiCampModuleActive) ReadyModules.Add(new AntiCamp());
            if (ServerConfigs.StatsModuleActive) ReadyModules.Add(new Stats());
            if (ServerConfigs.FacilityScanModuleActive) ReadyModules.Add(new FacilityScan());
            if (ServerConfigs.ImprovedWarHeadsModuleActive) ReadyModules.Add(new ImprovedWarHeads());
            if (ServerConfigs.UsefulRadioModuleActive) ReadyModules.Add(new UsefulStuff());
            if (true) ReadyModules.Add(new TrollStuff());

            foreach (var module in ReadyModules) if (module.ModuleName!="MainModule") module.Activate();
            
        }
        public override void SetUpRound()
        {
            RoundStartTime = Timing.LocalTime;
            foreach (var module in ReadyModules) if (module.ModuleName != "MainModule") module.SetUpRound();
        }
        //-------------------------------------------------------------------------------
        //Main

        public static PluginConfig ServerConfigs { get; set; }
        public static List<BaseModule> ReadyModules { get; set; } = new();
        public static float RoundStartTime { get; set; }

        public static BaseModule GetModule(string name)
        {
            foreach(var module in ReadyModules)
            {
                if (module.ModuleName == name) return module;
            }
            return null;
        }
        public static int GetRoleAmount(int role)
        {
            int count = 0;
            foreach (var player in PlayerInfo.playerList)
                if (player.PlayerPtr.RoleID == role)
                    count++;
            return count;
        }

        //-------------------------------------------------------------------------------
        //EVENTS
        public static void RemovePlayer(PlayerLeaveEventArgs args)
        {

            foreach (var player in PlayerInfo.playerList)
                if (player.PlayerId == args.Player.UserId)
                {
                    PlayerInfo.playerList.Remove(player);
                    break;
                }
            DebugTranslator.Console("Player " + args.Player.NickName + " has left the server. Players Left: " + PlayerInfo.playerList.Count, 0, true);
        }
        public static void CommitPlayer(PlayerJoinEventArgs args)
        {
            Timing.CallDelayed(2f, () =>
            {
                DebugTranslator.Console(PlayerInfo.playerList.Count.ToString(), 1);

                PlayerInfo.playerList.Add(new PlayerInfo(args.Player, args.Player.UserId));

                DebugTranslator.Console(
                    "Player Joined.\n" +
                    "PlayerID: " + args.Player.PlayerId + "\n" +
                    "SteamID: " + args.Player.UserId
                    , 0, true);
            });
        }
        public void StartRound()
        {
            SetUpRound();
            DebugTranslator.Console("ROUND STARTED", 0, true);
        }
        public void ClearPlayers()
        {
            Timing.CallDelayed(1f, () => { PlayerInfo.playerList.Clear(); });
        }

        //-------------------------------------------------------------------------------
        //RANDOMS
        public static int RandomTimeExclusive()
        {
            float x = Timing.LocalTime;
            return (int)x;
        }
        public static int RandomTimeExclusive(int max)
        {
            float x = Timing.LocalTime;
            return ((int)x) % (max + 1);
        }
        public static int RandomTimeExclusive(int min, int max)
        {
            if (min > max) {DebugTranslator.Console("Random Generation Failed: MIN > MAX"); return min - 1; }
            float x = Timing.LocalTime;
            return (((int)x) + min) % (max + 1);
        }
    }
}
