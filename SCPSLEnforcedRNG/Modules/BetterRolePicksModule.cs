using MEC;
using Synapse;
using Synapse.Api;
using Synapse.Api.Events.SynapseEventArguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLEnforcedRNG.Modules
{
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

    public class BetterRolePicks : BaseModule
    {
        //-------------------------------------------------------------------------------
        //Overrides
        public override string ModuleName { get { return "BetterRolePicks"; } }
        public override void Activate()
        {
            return;
        }
        public override void SetUpRound()
        {
            ResetRoles();
            Timing.CallDelayed(2f, () => AssignRoles(Server.Get.PlayersAmount));
        }

        //-------------------------------------------------------------------------------
        //Main
        public static int lastSCP = -1;

        public static void ResetRoles()
        {
            foreach (var player in PlayerInfo.playerList)
            {
                player.roundRole = -1;
            }
        }

        public static void AssignRoles(int playerCount)
        {
            string roles = MainModule.ServerConfigs.RolePicks.Substring(0, playerCount);
            
            DebugTranslator.Console("Will roll these roles: " + roles);

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
        private static void AssignSCP()
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
            int[] scpsPlus = { 0, 3, 5, 16, 9, 0, 3, 5, 16, 9, 0, 3, 5, 16, 9, 0, 3, 5, 16, 9 };
            int[] scps = PlayerInfo.playerList.Count >= 20 ? scpsPlus : scpsMinus;

            int tempIndexScp;
            int count = 0;
            do
            {
                tempIndexScp = UnityEngine.Random.Range(0, scps.Length);
                DebugTranslator.Console("Random Pick: " + tempIndexScp + "\nCorresponding Pick: " + scps[tempIndexScp]);
                count++;
            } while (scps[tempIndexScp] == lastSCP && count < 2);
            lastSCP = scps[tempIndexScp];

            selectedPlayer.PlayerPtr.RoleID = scps[tempIndexScp];
            selectedPlayer.AddUpCounts();

            if (scps[tempIndexScp] == 16) AntiCamp.doggoPtr = selectedPlayer; AntiCamp.doggoAlive = Timing.RunCoroutine(AntiCamp.DoggoCampTimer());

            string tempText = "";
            foreach (var player in tempPlayerList) tempText += player.PlayerPtr.NickName + ",";

            DebugTranslator.Console(
                tempText + " were deemed WORTHY\n" +
                "Player " + selectedPlayer.PlayerPtr.NickName + "\n" +
                "Rolled SCP with " + ((1.0f / tempPlayerList.Count) * 100.0f) + "% Probability");
        }
        private static void AssignPC()
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
            selectedPlayer.PlayerPtr.RoleType = RoleType.Scp079;
            selectedPlayer.AddUpCounts();

            string tempText = "";
            foreach (var player in tempPlayerList) tempText += player.PlayerPtr.NickName + ",";

            DebugTranslator.Console(
                tempText + " were deemed WORTHY\n" +
                "Player " + selectedPlayer.PlayerPtr.NickName + "\n" +
                "Rolled PC with " + ((1.0f / tempPlayerList.Count) * 100.0f) + "% Probability");
        }
        private static void AssignGuard()
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
        private static void AssignDBoi()
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
        private static void AssignScientist()
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

        private static void AssignSpawnRole(RoleType role, PlayerInfo? forcedPlayer = null)
        { //Change rolls to be more rng instead of forcing a queue
            uint tempVal = 0;
            List<PlayerInfo> tempPlayerList = new();
            foreach (var player in PlayerInfo.playerList)
            {
                if (player.roundRole != -1) continue;
                for(uint x = player.GetRoleCount(role);x>0;x--)
                {
                    tempPlayerList.Add(player);
                }
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

            string roleName = RoleType.None.ToString();

            DebugTranslator.Console(
                tempText + " were deemed WORTHY\n" +
                "Player " + selectedPlayer.PlayerPtr.NickName + "\n" +
                "Rolled " + roleName + " with " + ((1.0f / tempPlayerList.Count) * 100.0f) + "% Probability");
        }
    }
}
