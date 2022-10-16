using Synapse.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLEnforcedRNG
{
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
            var dbs = this.Query(queryable => queryable.ToList());
            
            return dbs.Last();
        }
    }

    public class RoundStatTrack
    {
        int TimesRonDiedToDoggoInCP { get; set; }
        int EscapedDClass { get; set; }
        int EscapedScientist { get; set; }
        int TotalDamageDealt { get; set; }
        int TotalMTFSpawns { get; set; }
        int TotalCISpawns { get; set; }
        int DoorInteracts { get; set; }
        int TeslaGateKills { get; set; }
        int CandiesGrabbed { get; set; }
        int HIDUsage { get; set; }

    }
    public class PlayerStatTrack
    {
        string PlayerID { get; set; }
        string PlayerName { get; set; }

        int DamageDealt { get; set; }
        int SCPDamageDealt { get; set; }
        int TotalKills { get; set; }
        int DamageHealed { get; set; }
        int SCPItemsUsed { get; set; }
        int DistanceWalkedMaybe { get; set; }
        int TimesRespawned { get; set; }
        int EscapeTime { get; set; }
        int DoorInteracts { get; set; }
        int ShotsFired { get; set; }
        ItemType HighestKeycardHeld { get; set; }
        int GeneratorsActivated { get; set; }
        int GeneratorsStopped { get; set; }
        int CoinFlips { get; set; }
    }
}
