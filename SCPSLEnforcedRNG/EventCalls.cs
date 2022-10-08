using MapGeneration;
using MEC;
using RemoteAdmin.Communication;
using Synapse;
using Synapse.Api;
using Synapse.Api.Events.SynapseEventArguments;
using Synapse.Api.Plugin;
using System.Dynamic;

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
            SynapseController.Server.Events.Player.PlayerEscapesEvent += OnPlayerEscape;
            SynapseController.Server.Events.Round.RoundEndEvent += OnRoundEnd;
            SynapseController.Server.Events.Round.RoundRestartEvent += OnRoundEnd;
            SynapseController.Server.Events.Player.PlayerRadioInteractEvent += OnRadio;
            //SynapseController.Server.Events.Scp.ScpAttackEvent

            GameTech.ServerConfigs = ServerConfigs;
        }
        public static void OnRadio(PlayerRadioInteractEventArgs args)
        {
            args.Radio.Durabillity = 1000;
        }
        public static void OnRoundStart()
        {
            GameTech.SetupMapStart();
            DebugTranslator.Console("ROUND STARTED", 0, true);
        }
        public static void OnRoundEnd()
        {
            GameTech.playerList.Clear();
            PlayerInfo.playerCount = 0;
        }
        public static void OnPlayerEscape(PlayerEscapeEventArgs args)
        {
            if (args.IsClassD && GameTech.GetRoleAmount(1) == 1)
                Timing.CallDelayed(20f, () =>
            {
                Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_A).Open = true;
                Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_A).Locked = true;
                Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_B).Open = true;
                Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_B).Locked = true;
                Map.Get.Cassie("ATTENTION . NO .g1 CLASS D PERSONNEL DETECTED INSIDE .g2 THE FACILITY . ALL .g3 FACILITY GATES HAVE BEEN OPENED . SCIENCE .g4 PERSONNEL SHOULD EVACUATE .g1 IMMEDIATELY");
                Map.Get.SendBroadcast(10, "All Gates Have Been Opened.");
            });
        }
        public static void OnPlayerDeath(PlayerDeathEventArgs args)
        {
            if (args.Victim.RoleType == RoleType.ClassD && GameTech.GetRoleAmount(1) == 1)
                Timing.CallDelayed(20f, () =>
                {
                    Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_A).Open = true;
                    Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_A).Locked = true;
                    Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_B).Open = true;
                    Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_B).Locked = true;
                    Map.Get.Cassie("ATTENTION . NO .g1 CLASS D PERSONNEL DETECTED INSIDE .g2 THE FACILITY . ALL .g3 FACILITY GATES HAVE BEEN OPENED . SCIENCE .g4 PERSONNEL SHOULD EVACUATE .g1 IMMEDIATELY");
                    Map.Get.SendBroadcast(10, "All Gates Have Been Opened.");
                });
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
            foreach (var player in GameTech.playerList)
                if (player.PlayerId == args.Player.UserId)
                {
                    GameTech.playerList.Remove(player);
                    break;
                }
            DebugTranslator.Console("Player " + args.Player.NickName + " has left the server. Players Left: " + GameTech.playerList.Count, 0, true);*/
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
