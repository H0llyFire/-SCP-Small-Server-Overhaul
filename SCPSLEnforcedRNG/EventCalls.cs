using MapGeneration;
using MEC;
using RemoteAdmin.Communication;
using Synapse;
using Synapse.Api;
using Synapse.Api.Events.SynapseEventArguments;
using Synapse.Api.Plugin;

namespace SCPSLEnforcedRNG
{
    public static class EventCalls
    {
        public static void SetupEvents(PluginConfig ServerConfigs)
        {
            SynapseController.Server.Events.Round.RoundStartEvent += OnRoundStart;
            SynapseController.Server.Events.Player.PlayerJoinEvent += OnPlayerJoin;
            SynapseController.Server.Events.Player.PlayerLeaveEvent += OnPlayerLeave;
            SynapseController.Server.Events.Round.TeamRespawnEvent += OnTeamRespawn;
            SynapseController.Server.Events.Player.PlayerGeneratorInteractEvent += OnGeneratorInteract;
            SynapseController.Server.Events.Player.PlayerDeathEvent += OnPlayerDeath;

            GameTech.ServerConfigs = ServerConfigs;
        }

        public static void OnRoundStart()
        {
            GameTech.SetupMapStart();
            DebugTranslator.Console("ROUND STARTED", 0, true);
        }
        public static void OnPlayerDeath(PlayerDeathEventArgs args)
        {
            if (GameTech.GetRoleAmount(1) == 0)
            {
                Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_A).Open = true;
                Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_A).Locked = true;
                Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_B).Open = true;
                Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_B).Locked = true;
            }
        }
        public static void OnPlayerJoin(PlayerJoinEventArgs args)
        {
            Timing.CallDelayed(2f, () =>
            {
                DebugTranslator.Console(GameTech.playerList.Count.ToString(), 1);

                GameTech.playerList.Add(new PlayerInfo(args.Player, args.Player.UserId));

                DebugTranslator.Console(
                    "Player Joined.\n" +
                    "PlayerID: " + args.Player.PlayerId + "\n" +
                    "SteamID: " + args.Player.UserId
                    , 0, true);

                //playerList[playerList.Count-1].PrintInfo();
            });
        }
        public static void OnPlayerLeave(PlayerLeaveEventArgs args)
        {
            /*
            foreach (var player in playerList)
                if (player.PlayerId == args.Player.UserId)
                {
                    playerList.Remove(player);
                    break;
                }
            DebugTranslator.Console("Player " + args.Player.NickName + " has left the server. Players Left: " + playerList.Count, 0, true);*/
        }
        public static void OnTeamRespawn(TeamRespawnEventArgs args)
        {
            args.Players.Clear();
            foreach (var player in GameTech.playerList)
            {
                if (player.PlayerPtr.RoleID == 14)
                {
                    args.Players.Add(player.PlayerPtr);
                }
            }
            if (args.Players.Count == 0) { DebugTranslator.Console("0 Players Waiting for respawn"); return; }

            if (GameTech.CheckGeneratorsOvercharge() == 3 && args.Players.Count > 0 && args.TeamID == 2)
            {
                Map.Get.Cassie("ATTENTION . ALL SECURITY PERSONNEL . CHAOS .g3 INSURGENCY HASENTERED . LETHAL FORCE .g4 AUTHORIZED");
                Map.Get.SendBroadcast(7, "Chaos Entered Facility");
            }


            DebugTranslator.Console(
                "Spawned team: " + args.TeamID + "\n" +
                "Spawned player amount: " + args.Players.Count);
        }
        public static void OnGeneratorInteract(PlayerGeneratorInteractEventArgs args)
        {
            if (args.GeneratorInteraction == Synapse.Api.Enum.GeneratorInteraction.Activated) GameTech.CheckGeneratorsOvercharge();
        }

    }


}
