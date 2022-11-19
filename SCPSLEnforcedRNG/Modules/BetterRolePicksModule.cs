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
        public static RoleType lastSCP = RoleType.None;

        public static void ResetRoles()
        {
            foreach (var player in PlayerInfo.playerList)
            {
                player.roundRole = RoleType.None;
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
                        AssignSpawnRole(RoleType.Scp173);
                        break;
                    case '1':
                        AssignSpawnRole(RoleType.Scp079);
                        break;
                    case '2':
                        AssignSpawnRole(RoleType.FacilityGuard);
                        break;
                    case '3':
                        AssignSpawnRole(RoleType.ClassD);
                        break;
                    case '4':
                        AssignSpawnRole(RoleType.Scientist);
                        break;
                    default:
                        break;
                }
                roles = roles.Remove(0, 1);
            }
            Round.Get.RoundLock = false;

        }
        private static void AssignSpawnRole(RoleType role, PlayerInfo? forcedPlayer = null)
        {
            List<PlayerInfo> tempPlayerList = new();
            foreach (var player in PlayerInfo.playerList)
            {
                if (player.roundRole != RoleType.None) continue;
                for (uint x = player.GetRoleCount(role) + 1; x > 0; x--)
                {
                    tempPlayerList.Add(player);
                }
            }

            int tempIndex = MainModule.RandomTimeSeededPos(0, tempPlayerList.Count-1);

            //DebugTranslator.Console(tempIndex.ToString() + " " + tempPlayerList.Count + " " + role.ToString());
            foreach (var player in tempPlayerList) DebugTranslator.Console(player.Name);
            DebugTranslator.Console("1");
            var selectedPlayer = tempPlayerList[tempIndex];
            DebugTranslator.Console("2");

            RoleType[] scps = { RoleType.Scp173, RoleType.Scp106, RoleType.Scp049, RoleType.Scp93953 };
            RoleType scpRole = scps[MainModule.RandomTimeSeededPos(scps.Length-1)];
            DebugTranslator.Console("3");
            if (scpRole == lastSCP) scpRole = scps[MainModule.RandomTimeSeededPos(scps.Length-1)];
            lastSCP = scpRole;
            DebugTranslator.Console("4");

            if (role == RoleType.Scp173 && scpRole == RoleType.Scp93953)
                { AntiCamp.doggoPtr = selectedPlayer; AntiCamp.doggoAlive = Timing.RunCoroutine(AntiCamp.DoggoCampTimer()); }
            DebugTranslator.Console("5");

            selectedPlayer.roundRole = role;
            DebugTranslator.Console("6");
            selectedPlayer.PlayerPtr.RoleType = role == RoleType.Scp173 ? scpRole : role;
            DebugTranslator.Console("7");
            selectedPlayer.AddUpCounts();
            DebugTranslator.Console("8");

            if (role == RoleType.ClassD || role == RoleType.Scientist) 
                selectedPlayer.PlayerPtr.Inventory.AddItem(ItemType.Coin);
            if (role == RoleType.ClassD || role == RoleType.Scientist || role == RoleType.FacilityGuard) 
                selectedPlayer.PlayerPtr.Inventory.AddItem(ItemType.Flashlight);
            if (role == RoleType.FacilityGuard)
                selectedPlayer.PlayerPtr.Inventory.AddItem(ItemType.Ammo9x19);
            if (role == RoleType.FacilityGuard)
                selectedPlayer.PlayerPtr.Inventory.AddItem(ItemType.Adrenaline);
            DebugTranslator.Console("9");

            string roleName = role.ToString();
            int probabilityCount = 0;
            DebugTranslator.Console("10");
            foreach (var player in tempPlayerList)
                if (player == selectedPlayer) probabilityCount++;
            DebugTranslator.Console("11");
            DebugTranslator.Console(
                "Player " + selectedPlayer.PlayerPtr.NickName + "\n" +
                "Rolled " + roleName + " with " + (((float)probabilityCount) / tempPlayerList.Count * 100.0f) + "% Probability");
        }
    }
}
