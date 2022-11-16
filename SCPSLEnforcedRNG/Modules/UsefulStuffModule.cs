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
            SynapseController.Server.Events.Map.DoorInteractEvent += CuffInteract;
        }

        //-------------------------------------------------------------------------------
        //Events
        public static void OnRadio(PlayerRadioInteractEventArgs args)
        {
            args.Radio.Durabillity = 100;
        }
        public void CuffInteract(DoorInteractEventArgs args)
        {
            if (args.Player.IsCuffed && !args.Door.IsDestroyed && !args.Door.Locked) args.Door.Open = !args.Door.Open;
        }
    }
}
