using Synapse.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLEnforcedRNG
{
    public class ServerDB : IDatabaseEntity
    {
        public int Id { get; set; }
        public int GetId() => Id;

        public string? Port { get; set; }
    }
    public class ServerRepo : Repository<ServerDB>
    {
        public ServerDB FindById(int id) => Get(LiteDB.Query.EQ("Id", id));
        public ServerDB FindByPort(string port) => Get(LiteDB.Query.EQ("Port", port));
    }

    public class PlayerDB : IDatabaseEntity
    {
        public int Id { get; set; }
        public int ServerId { get; set; }
        public int GetId() => Id;

        public string? Name { get; set; }
        public string? SteamId { get; set; }
        public uint NotSCP          { get; set; } = 0;
        public uint NotGuard        { get; set; } = 0;
        public uint NotDboi         { get; set; } = 0;
        public uint NotScientist    { get; set; } = 0;
        public uint NotPC           { get; set; } = 0;
        public ushort PrefferedRole { get; set; }
    }
    public class PlayerRepo : Repository<PlayerDB>
    {
        public PlayerDB FindById(int id) => Get(LiteDB.Query.EQ("Id", id));
        public PlayerDB FindBySteamId(string id) => Get(LiteDB.Query.EQ("SteamId", id));
        public PlayerDB FindByName(string name) => Get(LiteDB.Query.EQ("Name", name));

    }

    public class RoundDB : IDatabaseEntity
    {
        public int Id { get; set; }
        public int ServerId { get; set; }
        public int GetId() => Id;

        public int Session { get; set; }
        public int Round { get; set; }

        public int TimesRonDiedToDoggoInCP { get; set; } = 0;
        public int EscapedDClass { get; set; } = 0;
        public int EscapedScientist { get; set; } = 0;
        public float TotalDamageDealt { get; set; } = 0;
        public int TotalMTFSpawns { get; set; } = 0;
        public int TotalCISpawns { get; set; } = 0;
        public int DoorInteracts { get; set; } = 0;
        public int TeslaGateKills { get; set; } = 0;
        public float HIDUsage { get; set; } = 0;

    }
    public class RoundRepo : Repository<RoundDB>
    {
        public RoundDB FindById(int id) => Get(LiteDB.Query.EQ("Id", id));
        public RoundDB FindByRound(int round, int session)
            => Get(LiteDB.Query.And("$.Round = " + round, "$.Session = " + session));
        public RoundDB FindLastRound()
        {
            try { return Query(queryable => queryable.ToList()).Last(); }
            catch { return null; }
        }
    }

    public class PlayerRoundDB : IDatabaseEntity
    {
        public int Id { get; set; }
        public int RoundId { get; set; }
        public int PlayerId { get; set; }
        public int GetId() => Id;

        public float DamageDealt { get; set; } = 0f;
        public float SCPDamageDealt { get; set; } = 0f;
        public int TotalKills { get; set; } = 0; 
        public float DamageHealed { get; set; } = 0; 
        public int SCPItemsUsed { get; set; } = 0;
        public int TimesRespawned { get; set; } = 0; 
        public float EscapeTime { get; set; } = 0f;
        public int DoorInteracts { get; set; } = 0; 
        public int ShotsFired { get; set; } = 0; 
        public ItemType HighestKeycardHeld { get; set; } = ItemType.None; 
        public int GeneratorsActivated { get; set; } = 0; 
        public int GeneratorsStopped { get; set; } = 0; 
        public int CoinFlips { get; set; } = 0; 

    }
    public class PlayerRoundRepo : Repository<PlayerRoundDB>
    {
        public PlayerRoundDB FindById(int id) => Get(LiteDB.Query.EQ("Id", id));
        public PlayerRoundDB FindByRoundPlayer(int roundId, int playerId)
            => Get(LiteDB.Query.And("RoundId = " + roundId, "PlayerId = " + playerId));

    }
}
