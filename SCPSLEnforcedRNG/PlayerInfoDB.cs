using Synapse.Api;
using Synapse.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class PlayerInfo
    {
        public static PlayerRepository repository = new();
        public static int playerCount = 0;

        public PlayerInfoDB playerInfo;
        //private Player playerPtr;
        public Player PlayerPtr { get; set; }
        public PlayerStatTrack StatTrackSession { get ; set; }
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
        public short roundRole;


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
            roundRole = -1;
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
            if (roundRole == -1) return;
            NotSCP =        (roundRole == 0 ? 0 : NotSCP + 1);
            NotPC =         (roundRole == 1 ? 0 : NotPC + 1);
            NotGuard =      (roundRole == 2 ? 0 : NotGuard + 1);
            NotDboi =       (roundRole == 3 ? 0 : NotDboi + 1);
            NotScientist =  (roundRole == 4 ? 0 : NotScientist + 1);

            DebugTranslator.Console("Player role: " + roundRole);
            SavePlayerToDB();
        }
        public void SavePlayerToDB()
        {
            repository.Save(playerInfo);
        }
    }
}
