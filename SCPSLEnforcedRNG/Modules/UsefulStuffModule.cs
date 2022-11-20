using MEC;
using Synapse.Api;
using Synapse.Api.Events.SynapseEventArguments;
using UnityEngine;

namespace SCPSLEnforcedRNG.Modules
{
    public class UsefulStuff : BaseModule
    {
        //-------------------------------------------------------------------------------
        //Override
        public override string ModuleName { get { return "UsefulRadio"; } }
        public override void Activate()
        {
            SynapseController.Server.Events.Player.PlayerRadioInteractEvent += OnRadio;
            //SynapseController.Server.Events.Map.DoorInteractEvent += CuffInteract;
            SynapseController.Server.Events.Player.PlayerStartWorkstationEvent += InstaStartWorkStation;
        }
        public override void SetUpRound()
        {
            Timing.KillCoroutines(SpectatorRespawnTimer);
            SpectatorRespawnTimer = Timing.RunCoroutine(SpectatorRespawn());
            Map.Get.Scp914.KnobState = Scp914.Scp914KnobSetting.OneToOne;


            Synapse.Api.CustomObjects.SynapseWorkStationObject outsideWorkStation = 
                new(new(148.4816f, 992.7029f, -46.49313f), Quaternion.Euler(0f, 90f, 0f), new(1f, 1f, 1f));
            Map.Get.WorkStations.Add(outsideWorkStation.WorkStation);

            //Map.Get.IntercomText = "";
        }

        //-------------------------------------------------------------------------------
        //Main
        public static CoroutineHandle SpectatorRespawnTimer { get; set; }

        public static IEnumerator<float> SpectatorRespawn()
        {
            for (; ; )
            {
                foreach (var player in PlayerInfo.playerList)
                    if (player.PlayerPtr.RoleType == RoleType.Spectator || player.PlayerPtr.RoleType == RoleType.Tutorial)
                    {
                        player.PlayerPtr.ActiveBroadcasts.Clear();
                        player.PlayerPtr.SendBroadcast(2, "Time until Respawn: " + (int)(BetterRespawns.respawnTimer - Timing.LocalTime));
                    }
                yield return Timing.WaitForSeconds(1f);
            }
        }

        //-------------------------------------------------------------------------------
        //Events
        public static void OnRadio(PlayerRadioInteractEventArgs args)
        {
            args.Radio.Durabillity = 100;
        }
        public void CuffInteract(DoorInteractEventArgs args)
        {
            DebugTranslator.Console("Door interacted");
            if (args.Player.IsCuffed && !args.Door.IsDestroyed && !args.Door.Locked) args.Door.Open = !args.Door.Open;
        }
        public void InstaStartWorkStation(PlayerStartWorkstationEventArgs args)
        {
            args.WorkStation.State = Synapse.Api.Enum.WorkstationState.Online;
        }
    }
}
