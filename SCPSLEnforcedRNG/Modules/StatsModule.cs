using MapGeneration;
using MEC;
using Synapse.Api;
using Synapse.Api.Events.SynapseEventArguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLEnforcedRNG.Modules
{
    public class Stats : BaseModule
    {
        //-------------------------------------------------------------------------------
        //Override
        public override string ModuleName { get { return "Stats"; } }
        public override void Activate()
        {
            SynapseController.Server.Events.Player.PlayerChangeItemEvent += TrackKeyCards;
            SynapseController.Server.Events.Player.PlayerShootEvent += TrackShotsFired;
            SynapseController.Server.Events.Player.PlayerHealEvent += TrackHealedAmount;
            SynapseController.Server.Events.Player.PlayerItemUseEvent += TrackSCPItemsUsage;
            SynapseController.Server.Events.Player.PlayerEscapesEvent += TrackEscapes;
            SynapseController.Server.Events.Player.PlayerDamageEvent += TrackDamage;
            SynapseController.Server.Events.Player.PlayerFlipCoinEvent += TrackCoinFlips;
            SynapseController.Server.Events.Map.DoorInteractEvent += TrackDoorInteractions;
            SynapseController.Server.Events.Round.RoundEndEvent += TrackHIDUsage;
            SynapseController.Server.Events.Round.RoundEndEvent += SaveStats;
            SynapseController.Server.Events.Player.PlayerDeathEvent += TrackCPDeath;
            SynapseController.Server.Events.Player.PlayerDeathEvent += TrackTeslaDeaths;
            SynapseController.Server.Events.Player.PlayerDeathEvent += TrackKills;
            SynapseController.Server.Events.Round.TeamRespawnEvent += TrackRespawns;
            SynapseController.Server.Events.Player.PlayerGeneratorInteractEvent += TrackGenerators;
        }
        public override void SetUpRound()
        {
            RoundStats = new();
            foreach (var player in PlayerInfo.playerList)
                player.StatTrackRound = new()
                {
                    PlayerID = player.PlayerId,
                    PlayerName = player.Name
                };
        }

        //-------------------------------------------------------------------------------
        //Main
        public static RoundStatTrack? RoundStats { get; set; }
        public static PlayerStatTrack? BestUserStats { get; set; }

        public static void SaveStats()
        {
            foreach (var player in PlayerInfo.playerList)
            {
                player.AddUpStats();
            }
            StatTrack.PrintOutStats();
            StatTrack.MakeNewDBEntry();
        }

        //-------------------------------------------------------------------------------
        //Events
        public static void TrackGenerators(PlayerGeneratorInteractEventArgs args)
        {
            if (args.GeneratorInteraction == Synapse.Api.Enum.GeneratorInteraction.Activated) PlayerInfo.GetPlayerByID(args.Player.UserId).StatTrackRound.GeneratorsActivated++;
            else if (args.GeneratorInteraction == Synapse.Api.Enum.GeneratorInteraction.Disabled) PlayerInfo.GetPlayerByID(args.Player.UserId).StatTrackRound.GeneratorsStopped++;
        }
        public static void TrackRespawns(TeamRespawnEventArgs args)
        {
            if (args.TeamID == 1) RoundStats.TotalMTFSpawns++;
            else if (args.TeamID == 2) RoundStats.TotalCISpawns++;
        }
        public static void TrackKills(PlayerDeathEventArgs args)
        {
            if (args.Killer != null)
            {
                PlayerInfo.GetPlayerByID(args.Killer.UserId).StatTrackRound.TotalKills++;
            }
        }
        public static void TrackCPDeath(PlayerDeathEventArgs args)
        {
            if (args.Killer == null) return;
            List<RoomName> rooms = new() { RoomName.HczCheckpointA, RoomName.HczCheckpointB, RoomName.LczCheckpointA, RoomName.LczCheckpointB, RoomName.HczCheckpointToEntranceZone};
            if (args.Victim.UserId == PlayerInfo.RonID && (args.Killer.RoleType == RoleType.Scp93953 || args.Killer.RoleType == RoleType.Scp93989) && 
               (rooms.Contains(args.Victim.Room.RoomType)))
                    RoundStats.TimesRonDiedToDoggoInCP++;
        }
        public static void TrackTeslaDeaths(PlayerDeathEventArgs args)
        {
            if (args.DamageType == Synapse.Api.Enum.DamageType.Tesla) RoundStats.TeslaGateKills++;
        }
        public static void TrackHIDUsage()
        {
            foreach (var item in Map.Get.Items)
            {
                if (item.ItemType == ItemType.MicroHID)
                    RoundStats.HIDUsage = 100f - item.Durabillity;
            }
        }
        public static void TrackKeyCards(PlayerChangeItemEventArgs args)
        {
            List<ItemType> cards = new() { ItemType.KeycardChaosInsurgency, ItemType.KeycardNTFCommander, ItemType.KeycardFacilityManager, ItemType.KeycardContainmentEngineer, ItemType.KeycardNTFOfficer, ItemType.KeycardNTFLieutenant, ItemType.KeycardResearchCoordinator, ItemType.KeycardGuard, ItemType.KeycardZoneManager, ItemType.KeycardScientist, ItemType.KeycardJanitor };
            if (args.NewItem.ItemCategory == ItemCategory.Keycard)
            {

                var player = PlayerInfo.GetPlayerByID(args.Player.UserId);
                if (cards.IndexOf(player.StatTrackRound.HighestKeycardHeld) > cards.IndexOf(args.NewItem.ItemType))
                    player.StatTrackRound.HighestKeycardHeld = args.NewItem.ItemType;
            }
        }
        public static void TrackShotsFired(PlayerShootEventArgs args)
        {
            PlayerInfo.GetPlayerByID(args.Player.UserId).StatTrackRound.ShotsFired++;
        }
        public static void TrackHealedAmount(PlayerHealEventArgs args)
        {
            if (args.Player == null) return;
            PlayerInfo.GetPlayerByID(args.Player.UserId).StatTrackRound.DamageHealed += args.Amount;
        }
        public static void TrackSCPItemsUsage(PlayerItemInteractEventArgs args)
        {
            if (args.State == ItemInteractState.Finalizing)
            {
                List<ItemType> types = new List<ItemType>()
                    { ItemType.SCP018, ItemType.SCP1853, ItemType.SCP207, ItemType.SCP2176, ItemType.SCP244a, ItemType.SCP244b, ItemType.SCP268, ItemType.SCP330, ItemType.SCP500 };

                if (types.Contains(args.CurrentItem.ItemType))
                {
                    PlayerInfo.GetPlayerByID(args.Player.UserId).StatTrackRound.SCPItemsUsed++;
                }
            }
        }
        public static void TrackEscapes(PlayerEscapeEventArgs args)
        {
            if (args.IsClassD && !args.IsCuffed) RoundStats.EscapedDClass++;
            else if (args.IsClassD && args.IsCuffed) RoundStats.EscapedScientist++;
            else if (!args.IsClassD && !args.IsCuffed) RoundStats.EscapedScientist++;
            else if (!args.IsClassD && args.IsCuffed) RoundStats.EscapedDClass++;

            PlayerInfo.GetPlayerByID(args.Player.UserId).StatTrackRound.EscapeTime = Timing.LocalTime - MainModule.RoundStartTime;
            DebugTranslator.Console(args.Player.RoleName + " escaped.");
        }
        public static void TrackDamage(PlayerDamageEventArgs args)
        {
            if (args.Killer == null) return;
            if
            (
                args.Victim.RoleType == RoleType.Scp049 ||
                args.Victim.RoleType == RoleType.Scp173 ||
                args.Victim.RoleType == RoleType.Scp096 ||
                args.Victim.RoleType == RoleType.Scp106 ||
                args.Victim.RoleType == RoleType.Scp93953 ||
                args.Victim.RoleType == RoleType.Scp93989 ||
                args.Victim.RoleType == RoleType.Scp0492
            )
                PlayerInfo.GetPlayerByID(args.Killer.UserId).StatTrackRound.SCPDamageDealt += args.Damage > args.Victim.Health ? args.Victim.Health : args.Damage;
            else
                PlayerInfo.GetPlayerByID(args.Killer.UserId).StatTrackRound.DamageDealt += args.Damage > args.Victim.Health ? args.Victim.Health : args.Damage;
            RoundStats.TotalDamageDealt += args.Damage > args.Victim.Health ? args.Victim.Health : args.Damage;
            
        }
        public static void TrackCoinFlips(PlayerFlipCoinEventArgs args)
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
        public static void TrackDoorInteractions(DoorInteractEventArgs args)
        {
            if (args.Player == null) return;
            RoundStats.DoorInteracts++;
            PlayerInfo.GetPlayerByID(args.Player.UserId).StatTrackRound.DoorInteracts++;
        }
    }
}
