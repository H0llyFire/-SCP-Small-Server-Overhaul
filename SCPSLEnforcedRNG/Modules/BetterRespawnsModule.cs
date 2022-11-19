using MEC;
using Synapse.Api;
using Synapse.Api.Events.SynapseEventArguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
        public override void SetUpRound()
        {
            Timing.KillCoroutines(_respawnTimerRoutine);
            _respawnTimerRoutine = Timing.RunCoroutine(RoundRespawnTimer());
        }
        //-------------------------------------------------------------------------------
        //Main
        public static float respawnTimer = 0f;
        private static CoroutineHandle _respawnTimerRoutine;

        public static IEnumerator<float> RoundRespawnTimer()
        {
            for (; ; )
            {
                float time = UnityEngine.Random.Range(MainModule.ServerConfigs.respawnTime - MainModule.ServerConfigs.respawnTimeRange, MainModule.ServerConfigs.respawnTime + MainModule.ServerConfigs.respawnTimeRange);
                respawnTimer = Timing.LocalTime + time;

                bool isChaos = UnityEngine.Random.Range(0f, 1f) + MainModule.ServerConfigs.chaosChance > 1f;

                yield return Timing.WaitForSeconds(time - 20f);
                if (!(Map.Get.Nuke.Detonated || Map.Get.Nuke.Active))
                    TurnSpectatorsToTutorial();

                yield return Timing.WaitForSeconds(isChaos?10f:14f);
                if ((!Map.Get.Nuke.Detonated) && MainModule.GetRoleAmount(14) > 0)
                    Round.Get.SpawnVehicle(isChaos);
                else TurnTutorialToSpectators();

                yield return Timing.WaitForSeconds(10f);
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
                    player.PlayerPtr.ItemInHand = new(ItemType.Coin);
                }
        }
        public static void TurnTutorialToSpectators()
        {
            foreach (var player in PlayerInfo.playerList)
                if (player.PlayerPtr.RoleID == 14)
                    player.PlayerPtr.RoleID = 2;
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
    }
}
