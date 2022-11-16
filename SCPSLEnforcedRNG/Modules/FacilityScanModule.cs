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
    public class FacilityScan : BaseModule
    {
        //-------------------------------------------------------------------------------
        //Overrides
        public override string ModuleName { get { return "FacilityScan"; } }
        public override void Activate()
        {
            SynapseController.Server.Events.Player.PlayerEscapesEvent += CheckLastD;
            SynapseController.Server.Events.Player.PlayerDeathEvent += CheckLastD;
        }
        public override void SetUpRound()
        {
            Timing.KillCoroutines(_scan);
            LastScanTime = MainModule.RoundStartTime;
            _scan = Timing.RunCoroutine(ScanTimer());


            Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_A).Open = false;
            Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_A).Locked = false;
            Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_B).Open = false;
            Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_B).Locked = false;
        }

        //-------------------------------------------------------------------------------
        //Main
        public static float LastScanTime { get; set; } = 0f;
        private static CoroutineHandle _scan;

        private static void AnnounceNoDs()
        {
            Timing.CallDelayed(20f, () =>
            {
                Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_A).Open = true;
                Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_A).Locked = true;
                Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_B).Open = true;
                Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Gate_B).Locked = true;

                Map.Get.Cassie("ATTENTION . NO .g1 CLASS D PERSONNEL DETECTED INSIDE .g2 THE FACILITY . ALL .g3 FACILITY GATES HAVE BEEN OPENED . SCIENCE .g4 PERSONNEL SHOULD EVACUATE .g1 IMMEDIATELY");
                Map.Get.SendBroadcast(10, "All Gates Have Been Opened.");
            });
        }
        public static IEnumerator<float> ScanTimer()
        { // 16 => 8 => 4 min if no nuke if ppl in facility
            for (; ; )
            {
                yield return Timing.WaitForSeconds(5f);
                if (Map.Get.Nuke.Detonated) continue;
                int generators = MoreGeneratorFunctions.CheckGeneratorsOvercharge();
                float scanTime = Timing.LocalTime - LastScanTime;

                if ((generators == 1 && scanTime >= 960f) || (generators == 2 && scanTime >= 480f) || (generators == 3 && scanTime >= 240f))
                {
                    Map.Get.Cassie("jam_010_3 WARNING . FACILITY WIDE CAMERA SCAN IN T MINUS .g1 15 SECONDS");
                    yield return Timing.WaitForSeconds(15f);
                    ScanFacility();
                    LastScanTime = Timing.LocalTime;
                }
            }
        }
        public static void ScanFacility()
        {
            int lczCount = 0;
            int hczCount = 0;
            int ezCount = 0;
            int lczScpCount = 0;
            int hczScpCount = 0;
            int ezScpCount = 0;
            foreach (var player in PlayerInfo.playerList)
            {
                var playerRoom = player.PlayerPtr.Room.Zone;
                if (player.PlayerPtr.Team == Team.SCP)
                {
                    if (playerRoom == Synapse.Api.Enum.ZoneType.LCZ) lczScpCount++;
                    else if (playerRoom == Synapse.Api.Enum.ZoneType.HCZ) hczScpCount++;
                    else if (playerRoom == Synapse.Api.Enum.ZoneType.Entrance) ezScpCount++;
                }
                else
                {
                    if (playerRoom == Synapse.Api.Enum.ZoneType.LCZ) lczCount++;
                    else if (playerRoom == Synapse.Api.Enum.ZoneType.HCZ) hczCount++;
                    else if (playerRoom == Synapse.Api.Enum.ZoneType.Entrance) ezCount++;
                }
            }

            string cassieOut = "ATTENTION . CAMERA SCAN COMPLETE . ";

            if (lczCount > 0) cassieOut += lczCount + " PERSONNEL . ";
            if (lczScpCount > 0) cassieOut += lczScpCount + " SCP . ";
            if (lczCount + lczScpCount > 0) cassieOut += "DETECTED IN LIGHT CONTAINMENT ZONE . ";

            if (hczCount > 0) cassieOut += hczCount + " PERSONNEL . ";
            if (hczScpCount > 0) cassieOut += hczScpCount + " SCP . ";
            if (hczCount + hczScpCount > 0) cassieOut += "DETECTED IN HEAVY CONTAINMENT ZONE . ";

            if (ezCount > 0) cassieOut += ezCount + " PERSONNEL . ";
            if (ezScpCount > 0) cassieOut += ezScpCount + " SCP . ";
            if (ezCount + ezScpCount > 0) cassieOut += "DETECTED IN ENTERANCE ZONE . ";
            if (cassieOut.Length < 40) cassieOut += "NO BODY DETECTED INSIDE THE FACILITY";

            Map.Get.Cassie(cassieOut);
        }
        //-------------------------------------------------------------------------------
        //EVENTS
        public static void CheckLastD(PlayerEscapeEventArgs args)
        {
            if (args.IsClassD && MainModule.GetRoleAmount(1) == 1)
            {
                DebugTranslator.Console("Last Dclass Escaped.");
                AnnounceNoDs();
            }
        }
        public static void CheckLastD(PlayerDeathEventArgs args)
        {
            if (args.Victim.RoleType == RoleType.ClassD && MainModule.GetRoleAmount(1) == 1)
            {
                DebugTranslator.Console("Last Dclass Died.");
                AnnounceNoDs();
            }
        }
        
    }
}
