using Discord;
using Synapse.Api;
using Synapse.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLEnforcedRNG
{
    public static class StatTrack
    {
        private static StatRepo _repo = new();
        private static int currentRound = 0;
        public static int CurrentRound { get; }
        private static int currentSession = 0;
        public static int CurrentSession { get; }
    
        public static void MakeNewDBEntry()
        {
            List<PlayerStatTrack> playersStats = new List<PlayerStatTrack>();
            foreach (var player in GameTech.playerList)
            {
                if(player.StatTrackRound!=null)
                    playersStats.Add(player.StatTrackRound);
            }

            StatsDB statsDB = _repo.Insert(new StatsDB()
            {
                Round = currentRound++,
                Session = currentSession,
                RoundStats = GameTech.RoundStats,
                PlayerStats = playersStats

            });
            DebugTranslator.Console("New Round Stats written. Round: " + (statsDB.Round));
        }
        public static void SetUpCurrentSession()
        {
            var lastRoundStats = _repo.FindLastRound();
            if (lastRoundStats == null)
            {
                currentRound = 0;
                currentSession = 0;
            }
            else
            {
                currentRound = lastRoundStats.Round + 1;
                currentSession = lastRoundStats.Session + 1;
            }
        }
        public static void PrintOutStats()
        {
            string roundStats =
                "Times Roun died to the fucking dough: " + GameTech.RoundStats.TimesRonDiedToDoggoInCP + "\n" +
                "Escaped D-Bois: " + GameTech.RoundStats.EscapedDClass + "\n" +
                "Escaped Science-Bois: " + GameTech.RoundStats.EscapedScientist + "\n" +
                "Total Damage Dealt: " + GameTech.RoundStats.TotalDamageDealt + "\n" +
                "Total MTF Spawns: " + GameTech.RoundStats.TotalMTFSpawns + "\n" +
                "Total Chaos Spawns: " + GameTech.RoundStats.TotalCISpawns + "\n" +
                "Total Door Interactions" + GameTech.RoundStats.DoorInteracts + "\n" +
                "Tesla Gate Deaths: " + GameTech.RoundStats.TeslaGateKills + "\n" +
                "Ammount of Candy Grabbed: " + GameTech.RoundStats.CandiesGrabbed + "\n" +
                "HID Usage %: " + GameTech.RoundStats.HIDUsage + "\n";

            foreach (var player in GameTech.playerList)
            {
                string playerStats = 
                    "Damage Dealt: " + player.StatTrackRound.DamageDealt + "\n" +
                    "Damage Dealt to SCP: " + player.StatTrackRound.SCPDamageDealt + "\n" +
                    "Total Kills: " + player.StatTrackRound.TotalKills + "\n" +
                    "Damage Healed: " + player.StatTrackRound.DamageHealed + "\n" +
                    "SCP Items Used: " + player.StatTrackRound.SCPItemsUsed + "\n" +
                    "Distance Walked: " + player.StatTrackRound.DistanceWalkedMaybe + "\n" +
                    "Times Respawned: " + player.StatTrackRound.TimesRespawned + "\n" +
                    "Escape Time: " + player.StatTrackRound.EscapeTime + "\n" +
                    "Door Interactions: " + player.StatTrackRound.DoorInteracts + "\n" +
                    "Shots Fired: " + player.StatTrackRound.ShotsFired + "\n" +
                    "Highest Keycard Held: " + player.StatTrackRound.HighestKeycardHeld + "\n" +
                    "Generators Activated: " + player.StatTrackRound.GeneratorsActivated + "\n" +
                    "Generators Stopped: " + player.StatTrackRound.GeneratorsStopped + "\n" +
                    "Coin Flips: " + player.StatTrackRound.CoinFlips + "\n";
                
                player.PlayerPtr.SendConsoleMessage(DebugTranslator.TranslatePrefix(roundStats), "green");
                player.PlayerPtr.SendConsoleMessage(DebugTranslator.TranslatePrefix(playerStats), "yellow");
                
            }
            Map.Get.SendBroadcast(10, "Check Stats In Console (~)");
        }
    }
    public class StatsDB : IDatabaseEntity
    {
        public int Id { get; set; }
        public int Round { get; set; } = 0;
        public int Session { get; set; } = 0;

        public RoundStatTrack RoundStats { get; set; }
        public List<PlayerStatTrack> PlayerStats { get; set; }

        public int GetId() => Id;
    }
    public class StatRepo : Repository<StatsDB>
    {
        public StatsDB FindByRound(int Round)
        {
            return Get(LiteDB.Query.EQ("Round", Round));
        }
        public StatsDB FindLastRound()
        {
            try
            {
                return this.Query(queryable => queryable.ToList()).Last();
            }
            catch
            {
                return null;
            }
        }
    }

    public class RoundStatTrack
    {
        public int TimesRonDiedToDoggoInCP { get; set; } = 0;
        public int EscapedDClass { get; set; } = 0;
        public int EscapedScientist { get; set; } = 0;
        public int TotalDamageDealt { get; set; } = 0;
        public int TotalMTFSpawns { get; set; } = 0;
        public int TotalCISpawns { get; set; } = 0;
        public int DoorInteracts { get; set; } = 0;
        public int TeslaGateKills { get; set; } = 0;
        public int CandiesGrabbed { get; set; } = 0;
        public int HIDUsage { get; set; } = 0;

    }
    public class PlayerStatTrack
    {
        public string PlayerID { get; set; }
        public string PlayerName { get; set; }

        public int DamageDealt { get; set; } = 0;
        public int SCPDamageDealt { get; set; } = 0;
        public int TotalKills { get; set; } = 0;
        public int DamageHealed { get; set; } = 0;
        public int SCPItemsUsed { get; set; } = 0;
        public int DistanceWalkedMaybe { get; set; } = 0;
        public int TimesRespawned { get; set; } = 0;
        public int EscapeTime { get; set; } = 0;
        public int DoorInteracts { get; set; } = 0;
        public int ShotsFired { get; set; } = 0;
        public ItemType HighestKeycardHeld { get; set; } = ItemType.None;
        public int GeneratorsActivated { get; set; } = 0;
        public int GeneratorsStopped { get; set; } = 0;
        public int CoinFlips { get; set; } = 0;

        public static PlayerStatTrack operator+ (PlayerStatTrack a, PlayerStatTrack b)
        {
            PlayerStatTrack stats = new PlayerStatTrack();

            stats.DamageDealt = a.DamageDealt + b.DamageDealt;
            stats.SCPDamageDealt = a.SCPDamageDealt + b.SCPDamageDealt;
            stats.TotalKills = a.TotalKills + b.TotalKills;
            stats.DamageHealed = a.DamageHealed + b.DamageHealed;
            stats.SCPItemsUsed = a.SCPItemsUsed + b.SCPItemsUsed;
            stats.DistanceWalkedMaybe = a.DistanceWalkedMaybe + b.DistanceWalkedMaybe;
            stats.TimesRespawned = a.TimesRespawned + b.TimesRespawned;
            stats.EscapeTime = a.EscapeTime > b.EscapeTime ? b.EscapeTime : a.EscapeTime;
            stats.DoorInteracts = a.DoorInteracts + b.DoorInteracts;
            stats.ShotsFired = a.ShotsFired + b.ShotsFired;
            stats.HighestKeycardHeld = a.HighestKeycardHeld > b.HighestKeycardHeld ? a.HighestKeycardHeld : b.HighestKeycardHeld;
            stats.GeneratorsActivated = a.GeneratorsActivated + b.GeneratorsActivated;
            stats.GeneratorsStopped = a.GeneratorsStopped + b.GeneratorsStopped;
            stats.CoinFlips = a.CoinFlips + b.CoinFlips;

            return stats;
        }
    }
    public class DebugStatTrack
    {

    }
}
