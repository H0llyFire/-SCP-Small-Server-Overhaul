using Synapse.Api;
using Synapse.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCPSLEnforcedRNG
{
    public class PlayerInfoDB : IDatabaseEntity
    {
        public int Id { get; set; }
        public string GameIdentifier { get; set; }
        public string Name { get; set; }

        public uint NotSCP { get; set; } = 0;
        public uint NotGuard { get; set; } = 0;
        public uint NotDboi { get; set; } = 0;
        public uint NotScientist { get; set; } = 0;
        public uint NotPC { get; set; } = 0;

        public ushort PrefferedRole { get; set; }

        public int GetId() => Id;
    }

    public class PlayerRepository : Repository<PlayerInfoDB>
    {
        public PlayerInfoDB FindByPlayerID(string id)
        {
            return Get(LiteDB.Query.EQ("GameIdentifier", id));
        }
        public PlayerInfoDB FindByNickname(string nick)
        {
            return Get(LiteDB.Query.EQ("Name", nick));
        }
    }

    //Make A Component and rewrite a lot of shit to be easier to access :))))))
    public class PlayerInfo// : MonoBehaviour
    {
        public static PlayerRepository repository = new();
        public static List<PlayerInfo> playerList = new();
        public static int playerCount = 0;
        public const string AlexID = "76561198366313280@steam";
        public const string RonID = "76561198234561648@steam";

        public PlayerInfoDB playerInfo;
        public Player PlayerPtr { get; set; }
        public PlayerStatTrack StatTrackSession { get; set; } = new();
        public PlayerStatTrack StatTrackRound { get; set; }
        private int index;
        public int Index { get; }

        public string PlayerId
        {
            get { return playerInfo.GameIdentifier; }
            set { playerInfo.GameIdentifier = value; repository.Save(playerInfo); }
        }
        public string Name
        {
            get { return playerInfo.Name; }
            set { playerInfo.Name = value; repository.Save(playerInfo); }
        }
        public uint NotSCP 
        { 
            get { return playerInfo.NotSCP; }
            set { playerInfo.NotSCP = value; repository.Save(playerInfo); }
        }
        public uint NotGuard
        {
            get { return playerInfo.NotGuard; }
            set { playerInfo.NotGuard = value; repository.Save(playerInfo); }
        }
        public uint NotDboi
        {
            get { return playerInfo.NotDboi; }
            set { playerInfo.NotDboi = value; repository.Save(playerInfo); }
        }
        public uint NotScientist
        {
            get { return playerInfo.NotScientist; }
            set { playerInfo.NotScientist = value; repository.Save(playerInfo); }
        }
        public uint NotPC
        {
            get { return playerInfo.NotPC; }
            set { playerInfo.NotPC = value; repository.Save(playerInfo); }
        }

        //Internal Role ID
        //0 - SCP
        //1 - PC
        //2 - Guard
        //3 - D-Class
        //4 - Scientist
        public ushort PrefferedRole
        {
            get { return playerInfo.PrefferedRole; }
            set { playerInfo.PrefferedRole = value; }
        }
        public RoleType roundRole;


        public PlayerInfo(Player playerPtr, string playerId)
        {
            playerInfo = repository.FindByPlayerID(playerId);
            string outText;
            if(playerInfo == null)
            {
                playerInfo = repository.Insert(new PlayerInfoDB() 
                {
                    GameIdentifier = playerId,
                    Name = playerPtr.NickName
                });
                //repository.Save(playerInfo);
                DebugTranslator.Console("Player not found in Database", 1);
            }
            else
                DebugTranslator.Console("Player found in Database");

            PlayerPtr = playerPtr;

            index = playerCount;
            playerCount++;
            roundRole = RoleType.None;
            DebugTranslator.Console("Player Registered on Index " + index);
            DebugTranslator.Console(Index.ToString() + " | " + playerInfo.GameIdentifier + " | " + playerInfo.Name);
            PrintInfo();
        }
        /*~PlayerInfo()
        {
            _playerCount--;
        }*/
        public string PrintInfo()
        {
            string info =
                "Player " + Name + " wasn't\n" +
                "SCP:       " + NotSCP       + " times\n" +
                "D-Boi:     " + NotDboi      + " times\n" +
                "Scientist: " + NotScientist + " times\n" +
                "Guard:     " + NotGuard     + " times\n" +
                "PC:        " + NotPC        + " times";

            DebugTranslator.Console(info);
            return info;
        }
        public void AddUpCounts()
        {
            if (roundRole == RoleType.None) return;
            NotSCP =        (roundRole == RoleType.Scp173 ? 0 : NotSCP + 1);
            NotPC =         (roundRole == RoleType.Scp079 ? 0 : NotPC + 1);
            NotGuard =      (roundRole == RoleType.FacilityGuard ? 0 : NotGuard + 1);
            NotDboi =       (roundRole == RoleType.ClassD ? 0 : NotDboi + 1);
            NotScientist =  (roundRole == RoleType.Scientist ? 0 : NotScientist + 1);

            DebugTranslator.Console("Player role: " + roundRole);
            SavePlayerToDB();
        }

        public void SavePlayerToDB()
        {
            repository.Save(playerInfo);
        }
        public void AddUpStats()
        {
            StatTrackSession += StatTrackRound;
        }

        public uint GetRoleCount(RoleType role)
        {
            switch(role)
            {
                case RoleType.ClassD:
                    return NotDboi;
                case RoleType.Scientist:
                    return NotScientist;
                case RoleType.FacilityGuard:
                    return NotGuard;
                case RoleType.Scp079:
                    return NotPC;
                case RoleType.Scp173:
                    return NotSCP;
                default:
                    return 0;
            }
        }

        public static PlayerInfo GetPlayerByID(string id)
        {
            foreach(var player in playerList)
            {
                if (player.PlayerId == id)
                    return player;
            }
            return null;
        }
        public static PlayerInfo GetPlayerByNick(string nick)
        {
            foreach (var player in playerList)
            {
                if (player.Name == nick)
                    return player;
            }
            return null;
        }
    }
}
