using Synapse.Api;
using Synapse.Api.Events.SynapseEventArguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLEnforcedRNG.Modules
{
    public class TrollStuff : BaseModule
    {
        //-------------------------------------------------------------------------------
        //Override
        public override string ModuleName { get { return "TrollStuff"; } }
        public override void Activate()
        {
            //SynapseController.Server.Events.Player.PlayerChangeItemEvent += RollLeftHandiness;

        }

        //-------------------------------------------------------------------------------
        //Events
        public void RollLeftHandiness(PlayerChangeItemEventArgs args)
        {
            if (args.Player.Scale.x < 0) args.Player.Scale = new(args.Player.Scale.x * -1, args.Player.Scale.y, args.Player.Scale.z);

            if (args.NewItem.ItemCategory == ItemCategory.Keycard)
            {
                int pick = UnityEngine.Random.Range(1, 10000);
                //DebugTranslator.Console("Pick: " + pick);

                if (pick % 10 == 0 && args.Player.Scale.x >= 0) args.Player.Scale = new(args.Player.Scale.x * -1, args.Player.Scale.y, args.Player.Scale.z);
                
            }

        }
        
    }
}
