using MapGeneration;
using MEC;
using RemoteAdmin.Communication;
using Synapse;
using Synapse.Api;

namespace SCPSLEnforcedRNG
{
    public static class GameTech
    {
        public static PluginConfig ServerConfigs { get; set; }
        public static RoundStatTrack RoundStats { get; set; }
        public static PlayerStatTrack BestUserStats { get; set; }

        public static List<CoroutineHandle> RoundCoroutines = new List<CoroutineHandle>();

        private static PlayerInfo doggoPtr;
        private static Room doggoRoom;
        private static int doggoCounter;
        public  static CoroutineHandle doggoLightsFlash;
        public  static CoroutineHandle doggoAlive;


        public static int lastSCP = 0;
        public static bool omegaWarhead;

        public static float respawnTimer = 0f;
        public static float lastScanTime = 0f;
        public static float roundStartTime;

        public static int LastGeneratorCheck = 0;
        public static bool LightsOutMode;
        public static List<Room> OfflineRooms = new List<Room>();

        public static IEnumerator<float> RoundRespawnTimer()
        {
            for (; ; )
            {
                float time = UnityEngine.Random.Range(ServerConfigs.respawnTime - ServerConfigs.respawnTimeRange, ServerConfigs.respawnTime + ServerConfigs.respawnTimeRange);
                respawnTimer = Timing.LocalTime + time;

                bool isChaos = UnityEngine.Random.Range(0f, 1f) + ServerConfigs.chaosChance > 1f;

                yield return Timing.WaitForSeconds(time - 20f);
                if (!(Map.Get.Nuke.Detonated||Map.Get.Nuke.Active))
                    TurnSpectatorsToTutorial();

                yield return Timing.WaitForSeconds(10f);
                if ((!Map.Get.Nuke.Detonated) && GetRoleAmount(14) > 0)
                    Round.Get.SpawnVehicle(isChaos);
                else TurnTutorialToSpectators();

                yield return Timing.WaitForSeconds(10f);
                if (!Map.Get.Nuke.Detonated)
                    Round.Get.MtfRespawn(isChaos);
                else TurnTutorialToSpectators();
            }
        }
        public static IEnumerator<float> ScanTimer()
        { // 16 => 8 => 4 min if no nuke if ppl in facility
            //bool commencingScan = false;

            for (; ; )
            {
                yield return Timing.WaitForSeconds(5f);

                int generators = CheckGeneratorsOvercharge();
                float scanTime = Timing.LocalTime - lastScanTime;

                if ((generators == 1 && scanTime >= 960f) || (generators == 2 && scanTime >= 480f) || (generators == 3 && scanTime >= 240f))
                {
                    Map.Get.Cassie("jam_010_3 WARNING . FACILITY WIDE CAMERA SCAN IN T MINUS .g1 15 SECONDS");
                    yield return Timing.WaitForSeconds(15f);
                    ScanFacility();
                    lastScanTime = Timing.LocalTime;
                }
            }
        }
        public static IEnumerator<float> GeneratorCheckTimer()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(3f);
                CheckGeneratorsOvercharge();
            }
        }
        public static IEnumerator<float> DoggoCampTimer()
        { //10s; 2s check
            for(; ; )
            {
                yield return Timing.WaitForSeconds(2f);
                if (doggoRoom != null && doggoRoom == doggoPtr.PlayerPtr.Room)
                {
                    //DebugTranslator.Console(doggoRoom.RoomName+"\n"+doggoCounter, 1);

                    if (doggoCounter == 3)
                    {
                        //FuckUp Lights
                        doggoLightsFlash = Timing.RunCoroutine(FlashingLightsTimer());
                        doggoCounter++;
                    }
                    else
                    {
                        doggoCounter++;
                    }
                }
                else
                {
                    //UnFuckUp Lights
                    if(doggoRoom!=null)
                    {
                        Timing.KillCoroutines(doggoLightsFlash);
                    }
                    doggoCounter = 0;
                    doggoRoom = doggoPtr.PlayerPtr.Room;
                    //DebugTranslator.Console(doggoRoom.RoomName, 1);
                }
            }
        }
        public static IEnumerator<float> FlashingLightsTimer()
        {
            for(; ; )
            {
                if (doggoRoom.RoomType != RoomName.Outside)
                    yield return Timing.WaitForSeconds(20f);
                if ((!OfflineRooms.Contains(doggoRoom)))
                    doggoRoom.LightsOut(0.2f);
                //DebugTranslator.Console("FLASH");
                yield return Timing.WaitForSeconds(5f);
            }
        }
        public static IEnumerator<float> OmegaWarheadTimer()
        {
            for(; ; )
            {
                omegaWarhead = true;
                Map.Get.Cassie("ALPHA WARHEAD DETONATION FAILED TO .g6 NEUTRALIZE ALL THE THREATS pitch_.4 .g4 .g4 pitch_1 . OMEGA WARHEAD DETONATION SEQUENCE ENGAGED . TOP SIDE OF THE FACILITY WILL BE DETONATED IN T MINUS 300 SECONDS");
                Map.Get.SendBroadcast(10, "Omega Warhead Detonation in 300 seconds.");

                yield return Timing.WaitForSeconds(150f); //150s left
                Map.Get.Cassie("pitch.4 .g4 .g4 pitch_1 OMEGA WARHEAD DETONATION IN T MINUS 150 SECONDS", true, false);
                Map.Get.SendBroadcast(10, "Omega Warhead Detonation in 150 seconds.");

                yield return Timing.WaitForSeconds(90f); //60s left
                Map.Get.Cassie("pitch.4 .g4 .g4 pitch_1 OMEGA WARHEAD DETONATION IN T MINUS 60 SECONDS", true, false);
                Map.Get.SendBroadcast(10, "Omega Warhead Detonation in 60 seconds.");

                yield return Timing.WaitForSeconds(40f); //20s left
                Map.Get.Cassie("pitch.4 .g4 .g4 pitch_1 OMEGA WARHEAD DETONATION IN T MINUS 20 SECONDS pitch_.2 .g4 . .g4 . .g4 . .g4 . .g4 . .g4", true, false);
                Map.Get.SendBroadcast(10, "Omega Warhead Detonation in 20 seconds.");

                yield return Timing.WaitForSeconds(20f);
                Map.Get.Nuke.Detonate();
                foreach(var player in PlayerInfo.playerList)
                {
                    player.PlayerPtr.Kill("Omega Warhead Detonation.");
                }
                yield return Timing.WaitForSeconds(50f);
            }
        }



        public static void SetupMapStart()
        {
            RoundStats = new();
            LightsOutMode = ServerConfigs.LightsOutMode;
            roundStartTime = Timing.LocalTime;
            lastScanTime = roundStartTime;
            LastGeneratorCheck = 0;
            ResetRoles();

            if (RoundCoroutines.Count > 0) foreach (var coroutine in RoundCoroutines) Timing.KillCoroutines(coroutine);
            Timing.KillCoroutines(doggoAlive);
            Timing.KillCoroutines(doggoLightsFlash);

            Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Intercom).Locked = true;
            Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_A).Open = false;
            Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_A).Locked = false;
            Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_B).Open = false;
            Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_B).Locked = false;
            LightsOut(ServerConfigs.StartingLightsOff);


            Timing.CallDelayed(2f, () => AssignRoles(Server.Get.PlayersAmount));
            RoundCoroutines.Add(Timing.RunCoroutine(RoundRespawnTimer()));
            RoundCoroutines.Add(Timing.RunCoroutine(GeneratorCheckTimer()));
            
        }


        public static void ScanFacility()
        {
            int lczCount = 0;
            int hczCount = 0;
            int ezCount  = 0;
            int lczScpCount = 0;
            int hczScpCount = 0;
            int ezScpCount = 0;
            foreach (var player in PlayerInfo.playerList)
            {
                var playerRoom = player.PlayerPtr.Room.Zone;
                if (player.PlayerPtr.Team==Team.SCP)
                {
                    if (playerRoom == Synapse.Api.Enum.ZoneType.LCZ) lczScpCount++;
                    else if (playerRoom == Synapse.Api.Enum.ZoneType.HCZ) hczScpCount++;
                    else if (playerRoom == Synapse.Api.Enum.ZoneType.Entrance) ezScpCount++;
                }
                else
                {
                    if (playerRoom == Synapse.Api.Enum.ZoneType.LCZ) lczCount++;
                    else if (playerRoom == Synapse.Api.Enum.ZoneType.HCZ) hczCount++;
                    else if (playerRoom == Synapse.Api.Enum.ZoneType.Entrance) ezCount++;
                }
            }

            string cassieOut = "ATTENTION . CAMERA SCAN COMPLETE . ";

            if (lczCount > 0) cassieOut += lczCount + " PERSONNEL . ";
            if (lczScpCount > 0) cassieOut += lczScpCount + " SCP . ";
            if (lczCount+lczScpCount>0) cassieOut += "DETECTED IN LIGHT CONTAINMENT ZONE . ";

            if (hczCount > 0) cassieOut += hczCount + " PERSONNEL . ";
            if (hczScpCount > 0) cassieOut += hczScpCount + " SCP . ";
            if (hczCount + hczScpCount > 0) cassieOut += "DETECTED IN HEAVY CONTAINMENT ZONE . ";

            if (ezCount > 0) cassieOut += ezCount + " PERSONNEL . ";
            if (ezScpCount > 0) cassieOut += ezScpCount + " SCP . ";
            if (ezCount + ezScpCount > 0) cassieOut += "DETECTED IN ENTERANCE ZONE . ";
            if (cassieOut.Length < 40) cassieOut += "NO BODY DETECTED INSIDE THE FACILITY";

            Map.Get.Cassie(cassieOut);

        }
        public static int LightsOut(int amount)
        {
            OfflineRooms.Clear();
            string output = "\n";
            if (LightsOutMode)
            {
                foreach (var room in Map.Get.Rooms)
                {
                    //output += "[EnforcedRNG]: " + room.Identifier + "\n";
                    room.LightController.NetworkLightsEnabled = false;
                    OfflineRooms.Add(room);
                }
                DebugTranslator.Console(OfflineRooms.Count + " Lights turned Off");
                return Map.Get.Rooms.Count;
            }
            else
            {
                var RoomList = Map.Get.Rooms;
                var FilteredRoomList = new List<Room>();
                foreach (var room in RoomList)
                {
                    //output += "[EnforcedRNG]: " + room.RoomType + "\n";

                    if (room.RoomType == RoomName.Unnamed
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

                for (int i = 0; i < (amount < FilteredRoomList.Count ? amount : FilteredRoomList.Count); i++)
                {
                    int x = UnityEngine.Random.Range(0, FilteredRoomList.Count);
                    FilteredRoomList[x].LightController.NetworkLightsEnabled = false;
                    OfflineRooms.Add(FilteredRoomList[x]);
                    FilteredRoomList.RemoveAt(x);
                }
                DebugTranslator.Console(OfflineRooms.Count + " Lights turned Off");
                return amount;
            }
        }
        public static void TurnOnLights(int amount)
        {
            if (LightsOutMode) return;
            for (int i = 0; i < (amount < OfflineRooms.Count ? amount : OfflineRooms.Count); i++)
            {
                int x = UnityEngine.Random.Range(0, OfflineRooms.Count - 1);
                OfflineRooms[x].LightController.NetworkLightsEnabled = true;
                OfflineRooms.RemoveAt(x);
            }
            DebugTranslator.Console(amount + " Lights turned On");
        }

        public static int CheckGeneratorsOvercharge()
        {
            if (LastGeneratorCheck == -1) return 3;
            int countEngaged = 0;
            int countActive = 0;
            foreach (var generator in Map.Get.Generators)
            {
                if (generator.Engaged) 
                    countEngaged++;
                else if (generator.Active)
                    countActive++;
            }
            ModifyMapOnGenerators();

            /*DebugTranslator.Console("Engaged Generators: " + countEngaged +
                "\nActive Generators: " + countActive);*/
            return countEngaged;
        }
        public static void ModifyMapOnGenerators()
        {
            if (LastGeneratorCheck == 3) return;

            int generatorCount = 0;
            foreach (var generator in Map.Get.Generators)
                if (generator.Engaged) generatorCount++;

            if (generatorCount == 3 && LastGeneratorCheck != 3)
            {
                Map.Get.Cassie("ALL FACILITY SECURITY SYSTEMS ARE NOW OPERATIONAL");
                Map.Get.SendBroadcast(10, "Security Systems ON-LINE");
                Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Intercom).Locked = false;
                TurnOnLights(ServerConfigs.StartingLightsOff);
                LastGeneratorCheck = 3;
            }
            else if (generatorCount == 2 && LastGeneratorCheck < 2)
            {
                Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Intercom).Locked = false;
                TurnOnLights(ServerConfigs.GeneratorLightsOn);
                LastGeneratorCheck = 2;
            }
            else if (generatorCount == 1 && LastGeneratorCheck < 1)
            {
                Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Intercom).Locked = false;
                TurnOnLights(ServerConfigs.GeneratorLightsOn);
                LastGeneratorCheck = 1;
            }
        }
        public static void TurnSpectatorsToTutorial()
        {
            foreach (var player in PlayerInfo.playerList)
                if (player.PlayerPtr.RoleID == 2)
                {
                    player.PlayerPtr.RoleID = 14;
                    player.PlayerPtr.Inventory.AddItem(ItemType.Coin);
                }
        }
        public static void TurnTutorialToSpectators()
        {
            foreach (var player in PlayerInfo.playerList)
                if (player.PlayerPtr.RoleID == 14)
                    player.PlayerPtr.RoleID = 2;
        }
        public static int GetRoleAmount(int role)
        {
            int count = 0;
            foreach (var player in PlayerInfo.playerList)
                if (player.PlayerPtr.RoleID == role)
                    count++;
            return count;
        }
        public static void ResetRoles()
        {
            foreach (var player in PlayerInfo.playerList)
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
         * 18 Chaos Insurgency  - Rifleman
         * 19 Chaos Insurgency  - Repressor
         * 20 Chaos Insurgency  - Marauder
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

        public static void AssignRoles(int playerCount)
        {
            string tempOut = "kys:\n";
            //DebugTranslator.Console(tempOut);
            //DebugTranslator.Console(playerCount.ToString());
            //DebugTranslator.Console(playerList.Count.ToString());
            foreach (PlayerInfo player in PlayerInfo.playerList)
            {
                tempOut += player.PlayerId + " | ";
            }
            DebugTranslator.Console(tempOut);

            string roles = ServerConfigs.RolePicks.Substring(0, playerCount);
            //char[] characters = roles.ToCharArray();
            //Array.Sort(characters);
            //roles = new String(characters);
            DebugTranslator.Console(roles);

            Round.Get.RoundLock = true;
            for (int i = 0; i < playerCount; i++)
            {
                switch (roles[0])
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
            Round.Get.RoundLock = false;

        }
        public static void AssignSCP()
        {
            uint tempVal = 0;
            List<PlayerInfo> tempPlayerList = new();
            foreach (var player in PlayerInfo.playerList)
            {
                if (player.PlayerId == PlayerInfo.AlexID) continue;
                if (tempVal < player.NotSCP)
                {
                    tempVal = player.NotSCP;
                    tempPlayerList.Clear();
                }
                if (player.roundRole == -1 && tempVal == player.NotSCP) tempPlayerList.Add(player);
            }

            int tempIndex = UnityEngine.Random.Range(0, tempPlayerList.Count);
            var selectedPlayer = tempPlayerList[tempIndex];
            selectedPlayer.roundRole = 0;
            
            int[] scpsMinus = { 0, 3, 5, 16, 0, 3, 5, 16, 0, 3, 5, 16, 0, 3, 5, 16 };
            int[] scpsPlus  = { 0, 3, 5, 16, 9, 0, 3, 5, 16, 9, 0, 3, 5, 16, 9, 0, 3, 5, 16, 9 };
            int[] scps = PlayerInfo.playerList.Count >= 10 ? scpsPlus : scpsMinus;

            int tempIndexScp;
            int count = 0;
            do
            {
                tempIndexScp = UnityEngine.Random.Range(0, scps.Length);
                DebugTranslator.Console("Random Pick: " + tempIndexScp + "\nCorresponding Pick: " + scps[tempIndexScp]);
                count++;
            } while (scps[tempIndexScp] == lastSCP && count < 3);
            lastSCP = scps[tempIndexScp];

            selectedPlayer.PlayerPtr.RoleID = scps[tempIndexScp];
            selectedPlayer.AddUpCounts();

            if (scps[tempIndexScp % scps.Length] == 16) 
            {
                DebugTranslator.Console("Doggo SCP", 1);
                doggoPtr = selectedPlayer;
                doggoAlive = Timing.RunCoroutine(DoggoCampTimer()); 
            }

            string tempText = "";
            foreach (var player in tempPlayerList) tempText += player.PlayerPtr.NickName + ",";

            DebugTranslator.Console(
                tempText + " were deemed WORTHY\n" +
                "Player " + selectedPlayer.PlayerPtr.NickName + "\n" +
                "Rolled SCP with " + ((1.0f / tempPlayerList.Count) * 100.0f) + "% Probability");
        }
        public static void AssignPC()
        {
            uint tempVal = 0;
            List<PlayerInfo> tempPlayerList = new();
            foreach (var player in PlayerInfo.playerList)
            {
                if (player.PlayerId == PlayerInfo.AlexID) continue;
                if (player.roundRole == -1 && tempVal < player.NotPC)
                {
                    tempVal = player.NotPC;
                    tempPlayerList.Clear();
                }
                if (player.roundRole == -1 && tempVal == player.NotPC) tempPlayerList.Add(player);
            }
            int tempIndex = UnityEngine.Random.Range(0, tempPlayerList.Count);
            var selectedPlayer = tempPlayerList[tempIndex];

            selectedPlayer.roundRole = 1;
            selectedPlayer.PlayerPtr.RoleID = 7;
            selectedPlayer.AddUpCounts();

            string tempText = "";
            foreach (var player in tempPlayerList) tempText += player.PlayerPtr.NickName + ",";

            DebugTranslator.Console(
                tempText + " were deemed WORTHY\n" +
                "Player " + selectedPlayer.PlayerPtr.NickName + "\n" +
                "Rolled PC with " + ((1.0f / tempPlayerList.Count) * 100.0f) + "% Probability");
        }
        public static void AssignGuard()
        {
            uint tempVal = 0;
            List<PlayerInfo> tempPlayerList = new();
            foreach (var player in PlayerInfo.playerList)
            {
                if (player.PlayerId == PlayerInfo.AlexID) continue;
                if (player.roundRole == -1 && tempVal < player.NotGuard)
                {
                    tempVal = player.NotGuard;
                    tempPlayerList.Clear();
                }
                if (player.roundRole == -1 && tempVal == player.NotGuard) tempPlayerList.Add(player);
            }
            int tempIndex = UnityEngine.Random.Range(0, tempPlayerList.Count);
            var selectedPlayer = tempPlayerList[tempIndex];

            selectedPlayer.roundRole = 2;
            selectedPlayer.PlayerPtr.RoleID = 15;
            selectedPlayer.AddUpCounts();

            selectedPlayer.PlayerPtr.Inventory.AddItem(ItemType.Flashlight);
            selectedPlayer.PlayerPtr.Inventory.AddItem(ItemType.Ammo9x19);
            selectedPlayer.PlayerPtr.Inventory.AddItem(ItemType.Adrenaline);

            string tempText = "";
            foreach (var player in tempPlayerList) tempText += player.PlayerPtr.NickName + ",";

            DebugTranslator.Console(
                tempText + " were deemed WORTHY\n" +
                "Player " + selectedPlayer.PlayerPtr.NickName + "\n" +
                "Rolled Guard with " + ((1.0f / tempPlayerList.Count) * 100.0f) + "% Probability");
        }
        public static void AssignDBoi()
        {
            uint tempVal = 0;
            List<PlayerInfo> tempPlayerList = new();
            foreach (var player in PlayerInfo.playerList)
            {
                if (player.PlayerId == PlayerInfo.AlexID && player.roundRole == -1)
                { 
                    tempPlayerList.Clear(); 
                    tempPlayerList.Add(player); 
                    break; 
                }
                //DebugTranslator.Console("Check " + player.Index);
                if (player.roundRole == -1 && tempVal < player.NotDboi)
                {
                    tempVal = player.NotDboi;
                    tempPlayerList.Clear();
                }
                if (player.roundRole == -1 && tempVal == player.NotDboi) tempPlayerList.Add(player);
            }
            int tempIndex = UnityEngine.Random.Range(0, tempPlayerList.Count);
            var selectedPlayer = tempPlayerList[tempIndex];

            selectedPlayer.roundRole = 3;
            selectedPlayer.PlayerPtr.RoleID = 1;
            selectedPlayer.AddUpCounts();
            selectedPlayer.PlayerPtr.Inventory.AddItem(15);
            selectedPlayer.PlayerPtr.Inventory.AddItem(35);

            string tempText = "";
            foreach (var player in tempPlayerList) tempText += player.PlayerPtr.NickName + ",";

            DebugTranslator.Console(
                tempText + " were deemed WORTHY\n" +
                "Player " + selectedPlayer.PlayerPtr.NickName + "\n" +
                "Rolled DBoi with " + ((1.0f / tempPlayerList.Count) * 100.0f) + "% Probability");
        }
        public static void AssignScientist()
        {
            uint tempVal = 0;
            List<PlayerInfo> tempPlayerList = new();
            foreach (var player in PlayerInfo.playerList)
            {
                if (player.PlayerId == PlayerInfo.AlexID) continue;
                if (player.roundRole == -1 && tempVal < player.NotScientist)
                {
                    tempVal = player.NotScientist;
                    tempPlayerList.Clear();
                }
                if (player.roundRole == -1 && tempVal == player.NotScientist) tempPlayerList.Add(player);
            }
            int tempIndex = UnityEngine.Random.Range(0, tempPlayerList.Count);
            var selectedPlayer = tempPlayerList[tempIndex];

            selectedPlayer.roundRole = 4;
            selectedPlayer.PlayerPtr.RoleID = 6;
            selectedPlayer.AddUpCounts();
            selectedPlayer.PlayerPtr.Inventory.AddItem(15);
            selectedPlayer.PlayerPtr.Inventory.AddItem(35);

            string tempText = "";
            foreach (var player in tempPlayerList) tempText += player.PlayerPtr.NickName + ",";

            DebugTranslator.Console(
                tempText + " were deemed WORTHY\n" +
                "Player " + selectedPlayer.PlayerPtr.NickName + "\n" +
                "Rolled Scientist with " + ((1.0f / tempPlayerList.Count) * 100.0f) + "% Probability");
        }
    }
}
