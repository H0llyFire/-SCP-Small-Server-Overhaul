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
            Map.Get.Scp914.KnobState = Scp914.Scp914KnobSetting.OneToOne;
            //Map.Get.IntercomText = "";
        }

        //-------------------------------------------------------------------------------
        //Main
        public static CoroutineHandle SpectatorRespawnTimer { get; set; }

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
