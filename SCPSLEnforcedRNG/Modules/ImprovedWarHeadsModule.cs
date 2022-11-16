using MEC;
using Synapse.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLEnforcedRNG.Modules
{
    public class ImprovedWarHeads : BaseModule
    {
        //-------------------------------------------------------------------------------
        //Override
        public override string ModuleName { get { return "ImprovedWarHeads"; } }
        public override void Activate()
        {
            SynapseController.Server.Events.Map.WarheadDetonationEvent += StartOmegaWarHead;
        }
        public override void SetUpRound()
        {
            omegaWarhead = false;
            Timing.KillCoroutines(_omegaTimer);
        }

        //-------------------------------------------------------------------------------
        //Main
        public static bool omegaWarhead;
        private static CoroutineHandle _omegaTimer;

        public static IEnumerator<float> OmegaWarheadTimer()
        {
            for (; ; )
            {
                omegaWarhead = true;
                Map.Get.Cassie("ALPHA WARHEAD DETONATION FAILED TO .g6 NEUTRALIZE ALL THE THREATS pitch_.4 .g4 .g4 pitch_1 . OMEGA WARHEAD DETONATION SEQUENCE ENGAGED . TOP SIDE OF THE FACILITY WILL BE DETONATED IN T MINUS 300 SECONDS");
                Map.Get.SendBroadcast(10, "Omega Warhead Detonation in 300 seconds.");

                yield return Timing.WaitForSeconds(150f); //150s left
                Map.Get.Cassie("pitch.4 .g4 .g4 pitch_1 OMEGA WARHEAD DETONATION IN T MINUS 150 SECONDS", true, false);
                Map.Get.SendBroadcast(10, "Omega Warhead Detonation in 150 seconds.");

                yield return Timing.WaitForSeconds(90f); //60s left
                Map.Get.Cassie("pitch.4 .g4 .g4 pitch_1 OMEGA WARHEAD DETONATION IN T MINUS 60 SECONDS", true, false);
                Map.Get.SendBroadcast(10, "Omega Warhead Detonation in 60 seconds.");

                yield return Timing.WaitForSeconds(40f); //20s left
                Map.Get.Cassie("pitch.4 .g4 .g4 pitch_1 OMEGA WARHEAD DETONATION IN T MINUS 20 SECONDS pitch_.2 .g4 . .g4 . .g4 . .g4 . .g4 . .g4", true, false);
                Map.Get.SendBroadcast(10, "Omega Warhead Detonation in 20 seconds.");

                yield return Timing.WaitForSeconds(20f);
                Map.Get.Nuke.Detonate();
                foreach (var player in PlayerInfo.playerList)
                {
                    player.PlayerPtr.Kill("Omega Warhead Detonation.");
                }
                yield return Timing.WaitForSeconds(50f);
            }
        }

        //-------------------------------------------------------------------------------
        //Events
        public static void StartOmegaWarHead()
        {
            if (omegaWarhead) return;
            _omegaTimer = Timing.RunCoroutine(OmegaWarheadTimer());
        }

    }
}
