using MapGeneration;
using MEC;
using RemoteAdmin.Communication;
using Synapse;
using Synapse.Api;
using Synapse.Api.Events.SynapseEventArguments;
using Synapse.Api.Items;
using Synapse.Api.Plugin;
using System.Dynamic;

namespace SCPSLEnforcedRNG
{
    public static class EventCalls
    {
        public static void SetupEvents(PluginConfig ServerConfigs)
        {
            SynapseController.Server.Events.Round.RoundStartEvent += OnRoundStart;
            SynapseController.Server.Events.Round.TeamRespawnEvent += OnTeamRespawn;
            SynapseController.Server.Events.Round.RoundEndEvent += OnRoundEnd;
            SynapseController.Server.Events.Round.RoundRestartEvent += OnRoundRestart;



            SynapseController.Server.Events.Player.PlayerJoinEvent += OnPlayerJoin;
            SynapseController.Server.Events.Player.PlayerLeaveEvent += OnPlayerLeave;

            SynapseController.Server.Events.Player.PlayerEscapesEvent += OnPlayerEscape;
            SynapseController.Server.Events.Player.PlayerEscapesEvent += OnEscape;

            SynapseController.Server.Events.Player.PlayerDeathEvent += OnPlayerDeath;
            SynapseController.Server.Events.Player.PlayerDamageEvent += OnPlayerDamage;
            SynapseController.Server.Events.Player.PlayerHealEvent += OnHeal;
            SynapseController.Server.Events.Player.PlayerShootEvent += OnShoot;

            SynapseController.Server.Events.Player.PlayerGeneratorInteractEvent += OnGeneratorInteract;
            SynapseController.Server.Events.Player.PlayerRadioInteractEvent += OnRadio;
            SynapseController.Server.Events.Player.PlayerFlipCoinEvent += OnCoinFlip;
            SynapseController.Server.Events.Player.PlayerItemUseEvent += OnItemUse;
            SynapseController.Server.Events.Player.PlayerChangeItemEvent += OnItemChange;
            


            SynapseController.Server.Events.Map.DoorInteractEvent += OnDoor;
            SynapseController.Server.Events.Map.WarheadDetonationEvent += OnNuke;
            
            
            //SynapseController.Server.Events.Scp.ScpAttackEvent

            GameTech.ServerConfigs = ServerConfigs;
        }

        public static void OnItemChange(PlayerChangeItemEventArgs args)
        {
            List<ItemType> cards = new() { ItemType.KeycardChaosInsurgency, ItemType.KeycardNTFCommander, ItemType.KeycardFacilityManager, ItemType.KeycardContainmentEngineer, ItemType.KeycardNTFOfficer, ItemType.KeycardNTFLieutenant, ItemType.KeycardResearchCoordinator, ItemType.KeycardGuard, ItemType.KeycardZoneManager, ItemType.KeycardScientist, ItemType.KeycardJanitor};
            if (args.NewItem.ItemCategory == ItemCategory.Keycard)
            {
                
                var player = PlayerInfo.GetPlayerByID(args.Player.UserId);
                if (cards.IndexOf(player.StatTrackRound.HighestKeycardHeld) > cards.IndexOf(args.NewItem.ItemType))
                    player.StatTrackRound.HighestKeycardHeld = args.NewItem.ItemType;
            }
        }
        public static void OnShoot(PlayerShootEventArgs args)
        {
            PlayerInfo.GetPlayerByID(args.Player.UserId).StatTrackRound.ShotsFired++;
        }
        public static void OnHeal(PlayerHealEventArgs args)
        {
            PlayerInfo.GetPlayerByID(args.Player.UserId).StatTrackRound.DamageHealed += args.Amount;
            
        }
        public static void OnItemUse(PlayerItemInteractEventArgs args)
        {
            if (args.State == ItemInteractState.Finalizing)
            {
                List<ItemType> types = new List<ItemType>() 
                    { ItemType.SCP018, ItemType.SCP1853, ItemType.SCP207, ItemType.SCP2176, ItemType.SCP244a, ItemType.SCP244b, ItemType.SCP268, ItemType.SCP330, ItemType.SCP500 };

                if(types.Contains(args.CurrentItem.ItemType))
                {
                    PlayerInfo.GetPlayerByID(args.Player.UserId).StatTrackRound.SCPItemsUsed++;
                }
            }
        }
        public static void OnEscape(PlayerEscapeEventArgs args)
        {
                 if ( args.IsClassD && !args.IsCuffed)  GameTech.RoundStats.EscapedDClass++;
            else if ( args.IsClassD &&  args.IsCuffed)  GameTech.RoundStats.EscapedScientist++;
            else if (!args.IsClassD && !args.IsCuffed)  GameTech.RoundStats.EscapedScientist++;
            else if (!args.IsClassD &&  args.IsCuffed)  GameTech.RoundStats.EscapedDClass++;

            PlayerInfo.GetPlayerByID(args.Player.UserId).StatTrackRound.EscapeTime = Timing.LocalTime - GameTech.roundStartTime;
            DebugTranslator.Console(args.Player.RoleName + " escaped.");
        }
        public static void OnPlayerDamage(PlayerDamageEventArgs args)
        {
            /*
            if 
            (
                args.Victim.RoleType == RoleType.Scp049   || 
                args.Victim.RoleType == RoleType.Scp173   || 
                args.Victim.RoleType == RoleType.Scp096   ||
                args.Victim.RoleType == RoleType.Scp106   ||
                args.Victim.RoleType == RoleType.Scp93953 ||
                args.Victim.RoleType == RoleType.Scp93989 ||
                args.Victim.RoleType == RoleType.Scp0492
            )
                PlayerInfo.GetPlayerByID(args.Killer.UserId).StatTrackRound.SCPDamageDealt += args.Damage;
            else
                PlayerInfo.GetPlayerByID(args.Killer.UserId).StatTrackRound.DamageDealt += args.Damage;
            GameTech.RoundStats.TotalDamageDealt += args.Damage;
            */
        }
        public static void OnCoinFlip(PlayerFlipCoinEventArgs args)
        {
            foreach (var player in PlayerInfo.playerList)
            {
                if (player.PlayerId == args.Player.UserId)
                {
                    player.StatTrackRound.CoinFlips++;
                    break;
                }
            }
            
        }
        public static void OnDoor(DoorInteractEventArgs args)
        {
            GameTech.RoundStats.DoorInteracts++;
            PlayerInfo.GetPlayerByID(args.Player.UserId).StatTrackRound.DoorInteracts++;
        }
        public static void OnNuke()
        {
            if (GameTech.omegaWarhead) return;
            int scp = 0;
            int mtf = 0;
            int cis  = 0;
            int dcl  = 0;

            foreach(var player in PlayerInfo.playerList)
            {
                switch(player.PlayerPtr.Team)
                {
                    case Team.SCP:
                        scp++;
                        break;
                    case Team.MTF:
                    case Team.RSC:
                    case Team.RIP:
                        mtf++;
                        break;
                    case Team.CDP:
                        dcl++;
                        break;
                    case Team.CHI:
                        cis++;
                        break;
                    default:
                        break;
                }
            }
            if      (scp > 0 && (mtf > 0 || dcl > 0)) GameTech.RoundCoroutines.Add(Timing.RunCoroutine(GameTech.OmegaWarheadTimer()));
            else if (mtf > 0 && (cis > 0 || dcl > 0)) GameTech.RoundCoroutines.Add(Timing.RunCoroutine(GameTech.OmegaWarheadTimer()));
        }
        public static void OnRadio(PlayerRadioInteractEventArgs args)
        {
            args.Radio.Durabillity = 100;
        }
        public static void OnRoundStart()
        {
            GameTech.SetupMapStart();
            foreach (var player in PlayerInfo.playerList)
            {
                player.StatTrackRound = new()
                {
                    PlayerID = player.PlayerId,
                    PlayerName = player.Name
                };
            }


            DebugTranslator.Console("ROUND STARTED", 0, true);
        }
        public static void OnRoundEnd()
        {
            foreach(var item in Map.Get.Items)
            {
                if (item.ItemType == ItemType.MicroHID)
                    GameTech.RoundStats.HIDUsage = 100f-item.Durabillity;
            }



            foreach (var player in PlayerInfo.playerList)
            {
                player.AddUpStats();
            }
            //StatTrack.PrintOutStats();
            StatTrack.MakeNewDBEntry();
        }
        public static void OnRoundRestart()
        {
                PlayerInfo.playerList.Clear();
                PlayerInfo.playerCount = 0;
                GameTech.LastGeneratorCheck = 0;
                GameTech.omegaWarhead = false;
        }
        public static void OnPlayerEscape(PlayerEscapeEventArgs args)
        {
            if (args.IsClassD && GameTech.GetRoleAmount(1) == 1)
            {
                DebugTranslator.Console("Last Dclass Escaped.");
                Timing.CallDelayed(35f, () =>
                {
                    Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_A).Open = true;
                    Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_A).Locked = true;
                    Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_B).Open = true;
                    Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_B).Locked = true;
                    Map.Get.Cassie("ATTENTION . NO .g1 CLASS D PERSONNEL DETECTED INSIDE .g2 THE FACILITY . ALL .g3 FACILITY GATES HAVE BEEN OPENED . SCIENCE .g4 PERSONNEL SHOULD EVACUATE .g1 IMMEDIATELY");
                    Map.Get.SendBroadcast(10, "All Gates Have Been Opened.");
                });
            }
        }
        public static void OnPlayerDeath(PlayerDeathEventArgs args)
        {
            if (args.DamageType == Synapse.Api.Enum.DamageType.Tesla) GameTech.RoundStats.TeslaGateKills++;
            if (args.Victim.UserId == PlayerInfo.RonID && (args.Killer.RoleType == RoleType.Scp93953 || args.Killer.RoleType == RoleType.Scp93989) &&
               (args.Victim.Room.RoomType == RoomName.HczCheckpointA ||
                args.Victim.Room.RoomType == RoomName.HczCheckpointB ||
                args.Victim.Room.RoomType == RoomName.LczCheckpointA ||
                args.Victim.Room.RoomType == RoomName.LczCheckpointB ||
                args.Victim.Room.RoomType == RoomName.HczCheckpointToEntranceZone
                )) 
                    GameTech.RoundStats.TimesRonDiedToDoggoInCP++;


            if (args.Victim.RoleType == RoleType.ClassD && GameTech.GetRoleAmount(1) == 1)
            {
                DebugTranslator.Console("Last Dclass Died.");
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
            if(args.Victim.RoleType==RoleType.Scp93953)
            {
                Timing.KillCoroutines(GameTech.doggoAlive);
                Timing.KillCoroutines(GameTech.doggoLightsFlash);
            }

            if(args.Killer!=null)
            {
                PlayerInfo.GetPlayerByID(args.Killer.UserId).StatTrackRound.TotalKills++;
            }


        }
        public static void OnPlayerJoin(PlayerJoinEventArgs args)
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

                //playerList[playerList.Count-1].PrintInfo();
            });
        }
        public static void OnPlayerLeave(PlayerLeaveEventArgs args)
        {
            
            foreach (var player in PlayerInfo.playerList)
                if (player.PlayerId == args.Player.UserId)
                {
                    PlayerInfo.playerList.Remove(player);
                    break;
                }
            DebugTranslator.Console("Player " + args.Player.NickName + " has left the server. Players Left: " + PlayerInfo.playerList.Count, 0, true);
        }
        public static void OnTeamRespawn(TeamRespawnEventArgs args)
        {
            if (args.TeamID == 1) GameTech.RoundStats.TotalMTFSpawns++;
            else if (args.TeamID == 2) GameTech.RoundStats.TotalCISpawns++;

            args.Players.Clear();
            foreach (var player in PlayerInfo.playerList)
            {
                if (player.PlayerPtr.RoleID == 14)
                {
                    args.Players.Add(player.PlayerPtr);
                    player.StatTrackRound.TimesRespawned++;
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
            if (args.GeneratorInteraction == Synapse.Api.Enum.GeneratorInteraction.Activated) PlayerInfo.GetPlayerByID(args.Player.UserId).StatTrackRound.GeneratorsActivated++;
            else if (args.GeneratorInteraction == Synapse.Api.Enum.GeneratorInteraction.Disabled) PlayerInfo.GetPlayerByID(args.Player.UserId).StatTrackRound.GeneratorsStopped++;
            //if (args.GeneratorInteraction == Synapse.Api.Enum.GeneratorInteraction.Activated) GameTech.CheckGeneratorsOvercharge();
        }

    }


}
