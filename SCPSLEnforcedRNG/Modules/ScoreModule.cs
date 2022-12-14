using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLEnforcedRNG.Modules
{
    public class Score : BaseModule
    {
        /* Score Calcs?
         * Base Score 100pts
         
         * 100% of total Score if win as the same team you spawn as
         
         * [CUTS]
         * cut of 25% if you respawn as the same team you spawned as and win (halves after each trigger 25 => 12.5 => 6.25 =>...)
         * cut of 40% if you respawn as a different team and win (halves after each trigger)
         * cut of 50% if you die, but your team wins (last team you lived as)
         * cut of 75% if you lose but still live (Chaos/SCP)
         * cut of 100% if you die and your team loses (last team you lived as)
         * cut of 25% if stalemate 
         
         * [ADDITIONAL POINTS]
         * + 20 pts for killing an armed enemy
         * + 1 pts for killing Kritik
         * + 50 pts for killing SCP
         * + 25 pts for arming a nuke that explodes
         * + 20 pts for escaping
         * + % of damage dealt to scp (max 10 per SCP => 100% dealt)
         * + 5 pts for actually activating a generator as MTF-Friendly role
         * + 5 pts for deactivating a generator as Chaos-Friendly role
         * + 10 pts for executing SCP 079
         * ...
         
         * [SUBTRACTED POINTS]
         * - 10 pts for each minute you stay in the same fucking room (leaving for a few secs and going back is enough for a timer reset)
         * - 1 pts for killing H0lly lulw
         * - 40 pts to all non-SCP-Friendly roles if SCP 106 is terminated by Femur Breaker
         * ...
         */

        //-------------------------------------------------------------------------------
        //Override
        public override string ModuleName { get { return "Score"; } }

        public override void Activate()
        {
            SynapseController.Server.Events.Round.RoundEndEvent += CountScore;
        }
        public override void SetUpRound()
        {
        }

        //-------------------------------------------------------------------------------
        //Main

        //-------------------------------------------------------------------------------
        //Events
        private static void CountScore()
        {

        }
    }
}
