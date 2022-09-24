using MapGeneration;
using MEC;
using Synapse;
using Synapse.Api;
using Synapse.Api.Events.SynapseEventArguments;
using Synapse.Api.Plugin;
using static System.Net.WebRequestMethods;

namespace SCPSLEnforcedRNG
{
    //TODO:
    //New class xd Chaos Guard (chance for a guard to be Chaos with the skin of a guard (amogus)
    //Make preffered role system
    //4th respawn Zetta Wave?

    public class PlayerInfo
    {
        private static int _playerCount = 0;

        public Player playerPtr;
        public string id;
        public int index;

        //Change count to how many times wasn't a class
        public uint SCPcount      = 0;
        public uint GurdCount     = 0;
        public uint DBoiCount     = 0;
        public uint ScientisCount = 0;
        public uint PCCount       = 0;

        //Internal Role ID
        //0 - SCP
        //1 - PC
        //2 - Guard
        //3 - D-Class
        //4 - Scientist
        public ushort prefferedRole = 0;
        public short roundRole = -1;
        public PlayerInfo(Player playerPtr, string playerId)
        {
            this.playerPtr = playerPtr;
            this.id = playerId;
            this.index = _playerCount;
            _playerCount++;

            if (PluginClass.playerList.Count == 0) return;

            foreach (var player in PluginClass.playerList)
            {
                if (player.SCPcount      > SCPcount)      SCPcount      = player.SCPcount;
                if (player.DBoiCount     > DBoiCount)     DBoiCount     = player.DBoiCount;
                if (player.ScientisCount > ScientisCount) ScientisCount = player.ScientisCount;
                if (player.GurdCount     > GurdCount)     GurdCount     = player.GurdCount;
                if (player.PCCount       > PCCount)       PCCount       = player.PCCount;
            }
        }
        public string PrintInfo()
        {
            string info = "\n" +
                "[EnforcedRNG]: Player " + playerPtr.NickName + " was\n" +
                "[EnforcedRNG]: SCP " + SCPcount + " times\n" +
                "[EnforcedRNG]: D-Boi " + DBoiCount + " times\n" +
                "[EnforcedRNG]: Scientist " + ScientisCount + " times\n" +
                "[EnforcedRNG]: Guard " + GurdCount + " times.\n" +
                "[EnforcedRNG]: PC " + PCCount + " times.";

            Logger.Get.Info(info);
            return info;
        }
    }

    [PluginInformation(
        Name = "EnforcedRNG", //The Name of Your Plugin
        Author = "H0llyFire", // Your Name
        Description = "The RNG Manipulator 9000. Not fit for large or public servers", // A Description of your Plugin
        LoadPriority = 0, //When your Plugin should get loaded (use 0 if you don't know how to use it)
        SynapseMajor = 2, //The Synapse Version for which this Plugin was created for (SynapseMajor.SynapseMinor.SynapsePatch => 2.7.0)
        SynapseMinor = 10,
        SynapsePatch = 1,
        Version = "v.1.0.0" //The Current Version of your Plugin
        )]
    public class PluginClass : AbstractPlugin
    {
        [Config(section = "EnforcedRNG")]
        public static PluginConfig ServerConfigs { get; set; }



        public static float respawnTimer = 0;
        public static List<PlayerInfo> playerList = new();
        private List<CoroutineHandle> RoundCoroutines = new List<CoroutineHandle>();
        private int LastGeneratorCheck = 0;
        private bool LightsOutMode;
        private List<Room> OfflineRooms = new List<Room>();

        private IEnumerator<float> RoundRespawnTimer()
        {
            for (; ; )
            {
                float time = UnityEngine.Random.Range(ServerConfigs.respawnTime - ServerConfigs.respawnTimeRange, ServerConfigs.respawnTime + ServerConfigs.respawnTimeRange);
                respawnTimer = Timing.LocalTime + time;

                yield return Timing.WaitForSeconds(time-20f);
                TurnSpectatorsToTutorial();
                yield return Timing.WaitForSeconds(20f);
                ForceRespawns();
            }
        }

        public override void Load()
        {
            SynapseController.Server.Events.Round.RoundStartEvent += OnRoundStart;
            SynapseController.Server.Events.Player.PlayerJoinEvent += OnPlayerJoin;
            SynapseController.Server.Events.Round.TeamRespawnEvent += OnTeamRespawn;
            SynapseController.Server.Events.Player.PlayerGeneratorInteractEvent += OnGeneratorInteract;

            Logger.Get.Info("\n[EnforcedRNG]: PLUGIN LOADED SUCCESSFULLY");
        }

        public void OnRoundStart()
        {
            LastGeneratorCheck = 0;
            LightsOutMode = ServerConfigs.LightsOutMode;
            ResetRoles();
            if (RoundCoroutines.Count > 0) foreach (var coroutine in RoundCoroutines) Timing.KillCoroutines(coroutine);


            SetupMapStart();
            Timing.CallDelayed(2f, () => AssignRoles(Server.Get.PlayersAmount));

            RoundCoroutines.Add(Timing.RunCoroutine(RoundRespawnTimer()));

            Logger.Get.Info("\n[EnforcedRNG]: ROUND STARTED");
        }
        public void OnPlayerJoin(PlayerJoinEventArgs args)
        {
            Logger.Get.Info("\n" +
                "[EnforcedRNG]: Player Joined.\n" +
                "[EnforcedRNG]: PlayerID: " + args.Player.PlayerId + "\n" +
                "[EnforcedRNG]: SteamID: " + args.Player.UserId);

            bool isInList = false;
            int tempIndex = 0;
            foreach (var playerInfo in playerList)
            {
                if (playerInfo.id == args.Player.UserId)
                {
                    isInList = true;
                    break;
                }
                tempIndex++;
            }
            if (isInList) 
                playerList[tempIndex].playerPtr = args.Player;
            else 
                playerList.Add(new PlayerInfo(args.Player, args.Player.UserId));
            playerList[tempIndex].PrintInfo();
        }
        public void OnTeamRespawn(TeamRespawnEventArgs args)
        {
            args.Players.Clear();
            foreach(var player in playerList)
            {
                if(player.playerPtr.RoleID==14)
                {
                    args.Players.Add(player.playerPtr);
                }
            }

            Logger.Get.Info("\n" +
                "[EnforcedRNG]: Spawned team: " + args.TeamID + "\n" +
                "[EnforcedRNG]: Spawned player amount: " + args.Players.Count);

            if (CheckGeneratorsOvercharge() == 3 && args.Players.Count > 0 && args.TeamID == 2)
            {
                Map.Get.Cassie("ATTENTION . ALL SECURITY PERSONNEL . CHAOS .g3 INSURGENCY HASENTERED . LETHAL FORCE .g4 AUTHORIZED");
                Map.Get.SendBroadcast(7, "Chaos Entered Facility");
            }
        }
        public void OnGeneratorInteract(PlayerGeneratorInteractEventArgs args)
        {
            if (args.GeneratorInteraction == Synapse.Api.Enum.GeneratorInteraction.Activated) CheckGeneratorsOvercharge();
        }

        private void SetupMapStart()
        {
            Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Intercom).Locked = true;
            LightsOut(ServerConfigs.StartingLightsOff);
        }

        private int LightsOut(int amount)
        {
            string output = "\n";
            if (LightsOutMode)
            {
                foreach (var room in Map.Get.Rooms)
                {
                    //output += "[EnforcedRNG]: " + room.Identifier + "\n";
                    room.LightController.NetworkLightsEnabled = false;
                    OfflineRooms.Add(room);
                }
                Logger.Get.Info("\n[EnforcedRNG]: " + OfflineRooms.Count + " Lights turned Off");
                return Map.Get.Rooms.Count;
            }
            else
            {
                var RoomList = Map.Get.Rooms;
                var FilteredRoomList = new List<Room>();
                foreach(var room in RoomList)
                {
                    //output += "[EnforcedRNG]: " + room.RoomType + "\n";

                    if (   room.RoomType == RoomName.Unnamed 
                        || room.RoomType == RoomName.LczGreenhouse
                        || room.RoomType == RoomName.LczAirlock
                        || room.RoomType == RoomName.HczTesla
                        || room.RoomType == RoomName.EzCollapsedTunnel
                        || room.RoomType == RoomName.EzOfficeStoried
                        || room.RoomType == RoomName.EzOfficeLarge
                        || room.RoomType == RoomName.EzOfficeSmall)
                    {
                        FilteredRoomList.Add(room);
                    }
                }

                for (int i = 0; i < (amount<FilteredRoomList.Count?amount:FilteredRoomList.Count); i++)
                {
                    int x = UnityEngine.Random.Range(0, FilteredRoomList.Count);
                    FilteredRoomList[x].LightController.NetworkLightsEnabled = false;
                    OfflineRooms.Add(FilteredRoomList[x]);
                    FilteredRoomList.RemoveAt(x);
                }
                Logger.Get.Info("\n[EnforcedRNG]: " + OfflineRooms.Count + " Lights turned Off");
                return amount;
            }
        }
        private void TurnOnLights(int amount)
        {
            for (int i = 0; i < (amount<OfflineRooms.Count?amount:OfflineRooms.Count); i++)
            {
                int x = UnityEngine.Random.Range(0, OfflineRooms.Count);
                OfflineRooms[x].LightController.NetworkLightsEnabled = true;
                OfflineRooms.RemoveAt(x);
            }
            Logger.Get.Info("\n[EnforcedRNG]: " + amount + " Lights turned On");
        }

        private int CheckGeneratorsOvercharge()
        {
            if (LastGeneratorCheck == 3) return 3;
            int countEngaged = 0;
            int countActive = 0;
            foreach(var generator in Map.Get.Generators)
            {
                if (generator.Engaged) countEngaged++;
                else if (generator.Active) 
                { 
                    countActive++;
                    Timing.CallDelayed(generator.Time + 2f, () => CheckGeneratorsOvercharge()); 
                }
            }
            if (countEngaged > LastGeneratorCheck) ModifyMapOnGenerators(countEngaged);
    
            LastGeneratorCheck = countEngaged;
            Logger.Get.Info("\n[EnforcedRNG]: Engaged Generators: " + countEngaged + "\n[EnforcedRNG]: Active Generators: " + countActive);
            return countEngaged;
        }
        private void ModifyMapOnGenerators(int generatorCount)
        {
            switch(generatorCount)
            {
                case 3:
                    Map.Get.Cassie("ALL FACILITY SECURITY SYSTEMS ARE NOW OPERATIONAL");
                    Map.Get.SendBroadcast(10, "Security Systems ON-LINE");
                    Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Intercom).Locked = false;
                    TurnOnLights(ServerConfigs.StartingLightsOff);
                    break;
                case 2:
                    Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Intercom).Locked = false;
                    TurnOnLights(ServerConfigs.StartingLightsOff / 3);
                    break;
                case 1:
                    Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Intercom).Locked = false;
                    TurnOnLights(ServerConfigs.StartingLightsOff/3);
                    break;
                default:
                    break;
            }
        }
        private void TurnSpectatorsToTutorial()
        {
            foreach(var player in playerList)
            {
                if(player.playerPtr.RoleID==2)
                {
                    player.playerPtr.RoleID = 14;
                }
            }
        }
        private void ForceRespawns()
        {
            if (UnityEngine.Random.Range(0f, 1f) + ServerConfigs.chaosChance > 1f)
                Round.Get.MtfRespawn(true);
            else
                Round.Get.MtfRespawn(false);
        }
        private void ResetRoles()
        { 
            foreach (var player in playerList)
            {
                player.roundRole = -1;
            }
        }

        /*Roles List
         * 0  SCP 173           - Peanut
         * 1  Class-D Personel  - D-Boi
         * 2  Dead
         * 3  SCP 106           - Old Man
         * 4  Nine-Tailed Fox   - Specialist
         * 5  SCP 049           - Doctor
         * 6  Scientist
         * 7  SCP 079           - Computer
         * 8  Chaos Insurgency  - Conscript
         * 9  SCP 096           - Crying bitch
         * 10 SCP 049-02        - Zombie
         * 11 Nine-Tailed Fox   - Sergeant
         * 12 Nine-Tailed Fox   - Captain
         * 13 Nine-Tailed Fox   - Private
         * 14 Tutorial
         * 15 Facility Guard
         * 16 SCP 939-53        - Doggo 1
         * 17 SCP 939-89        - Doggo 2
         * 
         * SCPS Only
         *  0  SCP 173           - Peanut
         *  3  SCP 106           - Old Man
         *  5  SCP 049           - Doctor
         * *7  SCP 079           - Computer
         *  9  SCP 096           - Crying bitch
         *  16 SCP 939-53        - Doggo 1
         *  17 SCP 939-89        - Doggo 2
         *  
         * STARTERS Only
         *  1  Class-D Personel  - D-Boi
         *  6  Scientist
         *  15 Facility Guard
         */

        private void AssignRoles(int playerCount)
        {
            string roles = ServerConfigs.RolePicks.Substring(0, playerCount);
            char[] characters = roles.ToCharArray();
            Array.Sort(characters);
            roles = new String(characters);
            for(int i = 0; i < playerCount; i++)
            {
                switch(roles[0])
                {
                    case '0':
                        AssignSCP();
                        break;
                    case '1':
                        AssignPC();
                        break;
                    case '2':
                        AssignGuard();
                        break;
                    case '3':
                        AssignDBoi();
                        break;
                    case '4':
                        AssignScientist();
                        break;
                    default:
                        break;
                }
                roles = roles.Remove(0, 1);
            }

        }
        private void AssignSCP()
        {
            uint tempVal = uint.MaxValue;
            List<int> tempPlayerIndexList = new();
            foreach (var player in playerList)
            {
                if (tempVal > player.SCPcount)
                {
                    tempVal = player.SCPcount;
                    tempPlayerIndexList.Clear();
                }
                if (tempVal == player.SCPcount) tempPlayerIndexList.Add(player.index);
            }

            int tempIndex = UnityEngine.Random.Range(0, tempPlayerIndexList.Count);
            int selectedIndex = tempPlayerIndexList[tempIndex];
            playerList[selectedIndex].roundRole = 0;

            int[] scps = { 0, 3, 5, 16, 17, 0, 3, 5, 16, 17, 0, 3, 5, 16, 17, 0, 3, 5, 16, 17, 0, 3, 5, 16, 17, 0, 3, 5, 16, 17 };
            int tempIndexScp = UnityEngine.Random.Range(0, scps.Length);
            playerList[selectedIndex].playerPtr.RoleID = scps[tempIndexScp];
            playerList[selectedIndex].SCPcount += 1;

            string tempText = "";
            foreach (var playerIndex in tempPlayerIndexList) tempText += playerList[playerIndex].playerPtr.NickName + ",";

            Logger.Get.Info("\n" +
                "[EnforcedRNG]: " + tempText + " were deemed WORTHY\n" +
                "[EnforcedRNG]: Player " + playerList[selectedIndex].playerPtr.NickName + "\n" +
                "[EnforcedRNG]: Rolled SCP with " + ((1.0f / tempPlayerIndexList.Count) * 100.0f) + "% Probability");
        }
        private void AssignPC()
        {
            uint tempVal = uint.MaxValue;
            List<int> tempPlayerIndexList = new();
            foreach (var player in playerList)
            {
                if (tempVal > player.PCCount)
                {
                    tempVal = player.PCCount;
                    tempPlayerIndexList.Clear();
                }
                if (tempVal == player.PCCount) tempPlayerIndexList.Add(player.index);
            }
            int tempIndex = UnityEngine.Random.Range(0, tempPlayerIndexList.Count);
            int selectedIndex = tempPlayerIndexList[tempIndex];

            playerList[selectedIndex].roundRole = 4;
            playerList[selectedIndex].playerPtr.RoleID = 7;
            playerList[selectedIndex].PCCount += 1;

            string tempText = "";
            foreach (var playerIndex in tempPlayerIndexList) tempText += playerList[playerIndex].playerPtr.NickName + ",";

            Logger.Get.Info("\n" +
                "[EnforcedRNG]: " + tempText + " were deemed WORTHY\n" +
                "[EnforcedRNG]: Player " + playerList[selectedIndex].playerPtr.NickName + "\n" +
                "[EnforcedRNG]: Rolled PC with " + ((1.0f / tempPlayerIndexList.Count) * 100.0f) + "% Probability");
        }
        private void AssignGuard()
        {
            uint tempVal = uint.MaxValue;
            List<int> tempPlayerIndexList = new();
            foreach (var player in playerList)
            {
                if (player.roundRole == -1 && tempVal > player.GurdCount)
                {
                    tempVal = player.GurdCount;
                    tempPlayerIndexList.Clear();
                }
                if (player.roundRole==-1 && tempVal==player.GurdCount) tempPlayerIndexList.Add(player.index);
            }
            int tempIndex = UnityEngine.Random.Range(0, tempPlayerIndexList.Count);
            int selectedIndex = tempPlayerIndexList[tempIndex];

            playerList[selectedIndex].roundRole = 1;
            playerList[selectedIndex].playerPtr.RoleID = 15;
            playerList[selectedIndex].GurdCount += 1;

            string tempText = "";
            foreach (var playerIndex in tempPlayerIndexList) tempText += playerList[playerIndex].playerPtr.NickName + ",";

            Logger.Get.Info("\n" +
                "[EnforcedRNG]: " + tempText + " were deemed WORTHY\n" +
                "[EnforcedRNG]: Player " + playerList[selectedIndex].playerPtr.NickName + "\n" +
                "[EnforcedRNG]: Rolled Guard with " + ((1.0f / tempPlayerIndexList.Count) * 100.0f) + "% Probability");
        }
        private void AssignDBoi()
        {
            uint tempVal = uint.MaxValue;
            List<int> tempPlayerIndexList = new();
            foreach (var player in playerList)
            {
                if (player.roundRole == -1 && tempVal > player.DBoiCount)
                {
                    tempVal = player.DBoiCount;
                    tempPlayerIndexList.Clear();
                }
                if (player.roundRole == -1 && tempVal == player.DBoiCount) tempPlayerIndexList.Add(player.index);
            }
            int tempIndex = UnityEngine.Random.Range(0, tempPlayerIndexList.Count);
            int selectedIndex = tempPlayerIndexList[tempIndex];

            playerList[selectedIndex].roundRole = 2;
            playerList[selectedIndex].playerPtr.RoleID = 1;
            playerList[selectedIndex].DBoiCount += 1;

            string tempText = "";
            foreach (var playerIndex in tempPlayerIndexList) tempText += playerList[playerIndex].playerPtr.NickName + ",";

            Logger.Get.Info("\n" +
                "[EnforcedRNG]: " + tempText + " were deemed WORTHY\n" +
                "[EnforcedRNG]: Player " + playerList[selectedIndex].playerPtr.NickName + "\n" +
                "[EnforcedRNG]: Rolled DBoi with " + ((1.0f / tempPlayerIndexList.Count) * 100.0f) + "% Probability");
        }
        private void AssignScientist()
        {
            uint tempVal = uint.MaxValue;
            List<int> tempPlayerIndexList = new();
            foreach (var player in playerList)
            {
                if (player.roundRole == -1 && tempVal > player.ScientisCount)
                {
                    tempVal = player.ScientisCount;
                    tempPlayerIndexList.Clear();
                }
                if (player.roundRole == -1 && tempVal == player.ScientisCount) tempPlayerIndexList.Add(player.index);
            }
            int tempIndex = UnityEngine.Random.Range(0, tempPlayerIndexList.Count);
            int selectedIndex = tempPlayerIndexList[tempIndex];

            playerList[selectedIndex].roundRole = 3;
            playerList[selectedIndex].playerPtr.RoleID = 6;
            playerList[selectedIndex].ScientisCount += 1;

            string tempText = "";
            foreach (var playerIndex in tempPlayerIndexList) tempText += playerList[playerIndex].playerPtr.NickName + ",";

            Logger.Get.Info("\n" +
                "[EnforcedRNG]: " + tempText + " were deemed WORTHY\n" +
                "[EnforcedRNG]: Player " + playerList[selectedIndex].playerPtr.NickName + "\n" +
                "[EnforcedRNG]: Rolled Scientist with " + ((1.0f / tempPlayerIndexList.Count) * 100.0f) + "% Probability");
        }
    } 
}