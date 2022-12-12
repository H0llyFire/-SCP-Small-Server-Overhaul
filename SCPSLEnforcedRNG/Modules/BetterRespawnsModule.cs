using CommandSystem.Commands;
using CommandSystem.Commands.RemoteAdmin;
using Interactables.Interobjects.DoorUtils;
using MEC;
using Respawning;
using Synapse;
using Synapse.Api;
using Synapse.Api.Events.SynapseEventArguments;
using Synapse.Api.Items;

namespace SCPSLEnforcedRNG.Modules
{
    public class BetterRespawns : BaseModule
    {
        //-------------------------------------------------------------------------------
        //Overrides
        public override string ModuleName { get { return "BetterRespawns"; } }
        public override void Activate()
        {
            SynapseController.Server.Events.Round.TeamRespawnEvent += RespawnTutorials;
            SynapseController.Server.Events.Player.PlayerDeathEvent += CheckSCPKill;
            SynapseController.Server.Events.Round.TeamRespawnEvent += CommitAlphaWave;

            Server.Get.RoleManager.RegisterCustomRole<AlphaMTFCaptain>();
            Server.Get.RoleManager.RegisterCustomRole<AlphaMTFSpecialist>();
        }
        public override void SetUpRound()
        {
            Timing.KillCoroutines(_respawnTimerRoutine);
            _respawnTimerRoutine = Timing.RunCoroutine(RoundRespawnTimer());
            _hasScientistKilledSCP = false;
            _hasDclassKilledSCP = false;
        }
        //-------------------------------------------------------------------------------
        //Main
        public static float respawnTimer = 0f;
        private static CoroutineHandle _respawnTimerRoutine;
        private static bool _hasScientistKilledSCP = false;
        private static bool _hasDclassKilledSCP = false;

        public static IEnumerator<float> RoundRespawnTimer()
        {
            for (; ; )
            {
                float time = UnityEngine.Random.Range(MainModule.ServerConfigs.respawnTime - MainModule.ServerConfigs.respawnTimeRange, MainModule.ServerConfigs.respawnTime + MainModule.ServerConfigs.respawnTimeRange);
                respawnTimer = Timing.LocalTime + time;

                //bool isChaos = (UnityEngine.Random.Range(0f, 1f) + MainModule.ServerConfigs.chaosChance) > 1f;
                bool isChaos = (MainModule.RandomTimeSeededPos(0,100) + (int)(MainModule.ServerConfigs.chaosChance*100)) > 100;

                yield return Timing.WaitForSeconds(time - 20f);
                if (!(Map.Get.Nuke.Detonated || Map.Get.Nuke.Active))
                    TurnSpectatorsToTutorial();

                if (_hasDclassKilledSCP && (!_hasScientistKilledSCP)) isChaos = true;
                if (_hasScientistKilledSCP && (!_hasDclassKilledSCP)) isChaos = false;

                yield return Timing.WaitForSeconds(isChaos?10f:13f);
                if ((!Map.Get.Nuke.Detonated) && MainModule.GetRoleAmount(14) > 0)
                    Round.Get.SpawnVehicle(isChaos);
                else TurnTutorialToSpectators();

                yield return Timing.WaitForSeconds(isChaos ? 10f : 7f);

                if (!Map.Get.Nuke.Detonated)
                    Round.Get.MtfRespawn(isChaos);
                else TurnTutorialToSpectators();
            }
        }
        public static void TurnSpectatorsToTutorial()
        {
            foreach (var player in PlayerInfo.playerList)
                if (player.PlayerPtr.RoleID == 2)
                {
                    player.PlayerPtr.RoleID = 14;
                    SynapseItem item = new(ItemType.Coin);
                    player.PlayerPtr.Inventory.AddItem(item);
                    player.PlayerPtr.ItemInHand = item;
                }
        }
        public static void TurnTutorialToSpectators()
        {
            foreach (var player in PlayerInfo.playerList)
                if (player.PlayerPtr.RoleID == 14)
                    player.PlayerPtr.RoleID = 2;
        }
        private static void SetUpAlphaGear(List<Player> players)
        {
            bool captain = true;
            foreach(var player in players)
            {
                player.RoleID = captain?100:101;
                captain = false;
            }
        }

        //-------------------------------------------------------------------------------
        //EVENTS
        public static void RespawnTutorials(TeamRespawnEventArgs args)
        {
            args.Players.Clear();
            foreach (var player in PlayerInfo.playerList)
            {
                if (player.PlayerPtr.RoleID == 14)
                {
                    args.Players.Add(player.PlayerPtr);
                    player.StatTrackRound.TimesRespawned++;
                }
            }
            if (args.Players.Count == 0) { DebugTranslator.Console("0 Players Waiting for respawn"); return; }

            DebugTranslator.Console(
                "Spawned team: " + args.TeamID + "\n" +
                "Spawned player amount: " + args.Players.Count);
        }
        public static void CheckSCPKill(PlayerDeathEventArgs args)
        {
            if (args.Victim.Team == Team.SCP)
            {
                if (args.Killer.RoleType == RoleType.ClassD) _hasDclassKilledSCP = true;
                if (args.Killer.RoleType == RoleType.Scientist) _hasScientistKilledSCP = true;
            } 
        }
        public static void CommitAlphaWave(TeamRespawnEventArgs args)
        {
            //return; // Temp closed, didn't work yet anyways

            if (args.Team == Respawning.SpawnableTeamType.ChaosInsurgency) return;
            int threatLevel = 0;
            int safetyLevel = args.Players.Count*2;
            int divident = 25; //*2.5

            foreach(var player in PlayerInfo.playerList)
            {
                if (player.PlayerPtr.RoleType == RoleType.ClassD) threatLevel += 10;
                else if (player.PlayerPtr.Team == Team.CHI) threatLevel += 20;
                else if (player.PlayerPtr.RoleType == RoleType.Scp106 || player.PlayerPtr.RoleType == RoleType.Scp049 || player.PlayerPtr.RoleType == RoleType.Scp173 || player.PlayerPtr.RoleType == RoleType.Scp93953) threatLevel += 60;
                else if (player.PlayerPtr.RoleType == RoleType.Scp0492) threatLevel += 10;
                else if (player.PlayerPtr.RoleType == RoleType.Scientist) safetyLevel += 1;
                else if (player.PlayerPtr.RoleType == RoleType.FacilityGuard) safetyLevel += 1;
            }

            DebugTranslator.Console("Safety Level: " + safetyLevel*10 + "\nThreat Level: " + threatLevel);
            
            
            if(safetyLevel*divident<threatLevel)
            {
                Timing.CallDelayed(0.3f, () => SetUpAlphaGear(args.Players));
            }

        }

        //-------------------------------------------------------------------------------
        //Roles
        public class AlphaMTFCaptain : Synapse.Api.Roles.Role
        {
            public override int GetRoleID() => 100;
            public override string GetRoleName() => "Alpha-1 Captain";
            public override int GetTeamID() => (int)Team.MTF;
            public override List<int> GetFriendsID() => new List<int> { (int)Team.RSC, (int)Team.RIP };
            public override List<int> GetEnemiesID() => new List<int> { (int)Team.CHI, (int)Team.SCP, (int)Team.CDP };
            public override void Spawn()
            {
                RespawnEffectsController.ClearQueue();
                bool isScps = false;
                foreach (var player in PlayerInfo.playerList)
                    if (player.PlayerPtr.Team == Team.SCP)
                        isScps = true;
                if (isScps)
                    Map.Get.Cassie("MTFUNIT NATO_A .g5 1 DESIGNATED RED RIGHT JAM_010_2 HAND .g3 HASENTERED . ALL REMAINING SCPSUBJECTS .g1 WILL NOW BE jam_010_3 TERMINATED");
                else
                    Map.Get.Cassie("MTFUNIT NATO_A .g5 1 DESIGNATED RED RIGHT JAM_010_2 HAND .g3 HASENTERED . ALL CLASS D .g5 PERSONNEL ARE ADVISED TO jam_010_3 SURRENDER . AT THE NEAREST SECURITY CHECKPOINT . FAILURE TO COOPERATE WILL RESULT IN .g4 IMMEDIATE jam_010_4 TERMINATION");

                Player.RoleType = RoleType.NtfCaptain;
                Player.MaxHealth = 220;
                Player.Health = 220;
                Player.ArtificialHealth = 40;

                Player.Inventory.Clear();
                Player.AmmoBox[Synapse.Api.Enum.AmmoType.Ammo556x45] = 0;
                Player.AmmoBox[Synapse.Api.Enum.AmmoType.Ammo762x39] = 0;
                Player.AmmoBox[Synapse.Api.Enum.AmmoType.Ammo9x19] = 0;
                Player.AmmoBox[Synapse.Api.Enum.AmmoType.Ammo44cal] = 0;
                Player.AmmoBox[Synapse.Api.Enum.AmmoType.Ammo12gauge] = 0;


                SynapseItem item = new(ItemType.MicroHID);
                item.Durabillity = 100;
                Player.Inventory.AddItem(item);
                Player.Inventory.AddItem(ItemType.GunShotgun);
                Player.Inventory.AddItem(ItemType.KeycardNTFCommander);
                Player.Inventory.AddItem(ItemType.ArmorCombat);
                Player.Inventory.AddItem(ItemType.Adrenaline);
                Player.Inventory.AddItem(ItemType.Adrenaline);
                Player.Inventory.AddItem(ItemType.SCP500);

                Player.AmmoBox[Synapse.Api.Enum.AmmoType.Ammo556x45] = 60;
                Player.AmmoBox[Synapse.Api.Enum.AmmoType.Ammo12gauge] = 54;
                Player.AmmoBox[Synapse.Api.Enum.AmmoType.Ammo9x19] = 80;
            }
            
        }
        public class AlphaMTFSpecialist : Synapse.Api.Roles.Role
        {
            public override int GetRoleID() => 101;
            public override string GetRoleName() => "Alpha-1 Specialist";
            public override int GetTeamID() => (int)Team.MTF;
            public override List<int> GetFriendsID() => new List<int> { (int)Team.RSC, (int)Team.RIP };
            public override List<int> GetEnemiesID() => new List<int> { (int)Team.CHI, (int)Team.SCP, (int)Team.CDP };
            public override void Spawn()
            {
                Player.RoleType = RoleType.NtfSpecialist;
                Player.MaxHealth = 160;
                Player.Health = 160;
                Player.ArtificialHealth = 40;

                Player.Inventory.Clear();
                Player.AmmoBox[Synapse.Api.Enum.AmmoType.Ammo556x45] = 0;
                Player.AmmoBox[Synapse.Api.Enum.AmmoType.Ammo762x39] = 0;
                Player.AmmoBox[Synapse.Api.Enum.AmmoType.Ammo9x19] = 0;
                Player.AmmoBox[Synapse.Api.Enum.AmmoType.Ammo44cal] = 0;
                Player.AmmoBox[Synapse.Api.Enum.AmmoType.Ammo12gauge] = 0;

                SynapseItem item = new(ItemType.ParticleDisruptor);
                item.Durabillity = 5f;
                Player.Inventory.AddItem(item);
                Player.Inventory.AddItem(ItemType.KeycardNTFCommander);
                Player.Inventory.AddItem(ItemType.GunCrossvec);
                Player.Inventory.AddItem(ItemType.ArmorHeavy);
                Player.Inventory.AddItem(ItemType.Adrenaline);
                Player.Inventory.AddItem(ItemType.Adrenaline);
                Player.Inventory.AddItem(ItemType.Medkit);

                Player.AmmoBox[Synapse.Api.Enum.AmmoType.Ammo556x45] = 60;
                Player.AmmoBox[Synapse.Api.Enum.AmmoType.Ammo9x19] = 200;
                Player.AmmoBox[Synapse.Api.Enum.AmmoType.Ammo12gauge] = 24;

            }
        }
    }
}
