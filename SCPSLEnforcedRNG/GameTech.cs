using MapGeneration;
using MEC;
using RemoteAdmin.Communication;
using Synapse;
using Synapse.Api;
using Synapse.Api.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace SCPSLEnforcedRNG
{
    public static class GameTech
    {
        public static PluginConfig ServerConfigs;



        public static float respawnTimer = 0;
        public static List<PlayerInfo> playerList = new();
        public static float roundStartTime;

        public static List<CoroutineHandle> RoundCoroutines = new List<CoroutineHandle>();
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
                if (!Map.Get.Nuke.Detonated)
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



        public static void SetupMapStart()
        {
            LastGeneratorCheck = 0;
            LightsOutMode = ServerConfigs.LightsOutMode;
            roundStartTime = Timing.LocalTime;
            ResetRoles();
            if (RoundCoroutines.Count > 0) foreach (var coroutine in RoundCoroutines) Timing.KillCoroutines(coroutine);

            Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Intercom).Locked = true;
            LightsOut(ServerConfigs.StartingLightsOff);


            Timing.CallDelayed(2f, () => AssignRoles(Server.Get.PlayersAmount));
            RoundCoroutines.Add(Timing.RunCoroutine(RoundRespawnTimer()));
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
                if (generator.Engaged) countEngaged++;
                else if (generator.Active)
                {
                    countActive++;
                    Timing.CallDelayed(generator.Time + 2f, () => CheckGeneratorsOvercharge());
                }
            }
            ModifyMapOnGenerators();

            LastGeneratorCheck = countEngaged;
            DebugTranslator.Console("Engaged Generators: " + countEngaged +
                "\nActive Generators: " + countActive);
            return countEngaged;
        }
        public static void ModifyMapOnGenerators()
        {
            if (LastGeneratorCheck == -1) return;

            int generatorCount = 0;
            foreach (var generator in Map.Get.Generators)
                if (generator.Active) generatorCount++;
            switch (generatorCount)
            {
                case 3:
                    Map.Get.Cassie("ALL FACILITY SECURITY SYSTEMS ARE NOW OPERATIONAL");
                    Map.Get.SendBroadcast(10, "Security Systems ON-LINE");
                    Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Intercom).Locked = false;
                    TurnOnLights(ServerConfigs.StartingLightsOff);
                    LastGeneratorCheck = -1;
                    break;
                case 2:
                    Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Intercom).Locked = false;
                    TurnOnLights(ServerConfigs.GeneratorLightsOn);
                    break;
                case 1:
                    Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Intercom).Locked = false;
                    TurnOnLights(ServerConfigs.GeneratorLightsOn);
                    break;
                default:
                    break;
            }
        }
        public static void TurnSpectatorsToTutorial()
        {
            foreach (var player in playerList)
                if (player.PlayerPtr.RoleID == 2)
                    player.PlayerPtr.RoleID = 14;
        }
        public static void TurnTutorialToSpectators()
        {
            foreach (var player in playerList)
                if (player.PlayerPtr.RoleID == 14)
                    player.PlayerPtr.RoleID = 2;
        }
        public static int GetRoleAmount(int role)
        {
            int count = 0;
            foreach (var player in playerList)
                if (player.PlayerPtr.RoleID == role)
                    count++;
            return count;
        }
        public static void ResetRoles()
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
            foreach (PlayerInfo player in playerList)
            {
                tempOut += player.PlayerPtr.NickName + " | ";
            }
            DebugTranslator.Console(tempOut);

            string roles = ServerConfigs.RolePicks.Substring(0, playerCount);
            char[] characters = roles.ToCharArray();
            Array.Sort(characters);
            roles = new String(characters);
            DebugTranslator.Console(roles);
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

        }
        public static void AssignSCP()
        {
            uint tempVal = 0;
            List<PlayerInfo> tempPlayerList = new();
            foreach (var player in playerList)
            {
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

            int[] scps = { 0, 3, 5, 16, 0, 3, 5, 16, 0, 3, 5, 16, 0, 3, 5, 16, 0, 3, 5, 16, 0, 3, 5, 16 };
            int tempIndexScp = UnityEngine.Random.Range(0, scps.Length);
            selectedPlayer.PlayerPtr.RoleID = scps[tempIndexScp];
            selectedPlayer.AddUpCounts();

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
            foreach (var player in playerList)
            {
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
            foreach (var player in playerList)
            {
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
            foreach (var player in playerList)
            {
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
            foreach (var player in playerList)
            {
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
