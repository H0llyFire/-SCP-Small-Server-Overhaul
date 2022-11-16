using MapGeneration;
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
    public class MoreGeneratorFunctions : BaseModule
    {
        //-------------------------------------------------------------------------------
        //Override
        public override string ModuleName { get { return "MoreGeneratorFunctions"; } }
        public override void Activate()
        {
            SynapseController.Server.Events.Round.TeamRespawnEvent += AnnounceChaos;
        }

        public override void SetUpRound()
        {
            LightsOutMode = MainModule.ServerConfigs.LightsOutMode;
            LastGeneratorCheck = 0;


            Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Intercom).Locked = true;
            LightsOut(MainModule.ServerConfigs.StartingLightsOff);


            GeneratorCheck = Timing.RunCoroutine(GeneratorCheckTimer());
        }
        //-------------------------------------------------------------------------------
        //Main
        public static int LastGeneratorCheck { get; set; } = 0;
        public static bool LightsOutMode { get; set; }
        public static List<Room> OfflineRooms = new List<Room>();
        public static CoroutineHandle GeneratorCheck { get; set; }

        public static IEnumerator<float> GeneratorCheckTimer()
        {
            for (; ; )
            {
                yield return Timing.WaitForSeconds(3f);
                CheckGeneratorsOvercharge();
            }
        }
        public static int LightsOut(int amount)
        {
            OfflineRooms.Clear();
            //string output = "\n";
            if (LightsOutMode)
            {
                foreach (var room in Map.Get.Rooms)
                {
                    //output += "[EnforcedRNG]: " + room.Identifier + "\n";
                    room.LightController.NetworkLightsEnabled = false;
                    OfflineRooms.Add(room);
                }
                DebugTranslator.Console(OfflineRooms.Count + " Lights turned Off");
                return Map.Get.Rooms.Count;
            }
            else
            {
                var RoomList = Map.Get.Rooms;
                var FilteredRoomList = new List<Room>();
                foreach (var room in RoomList)
                {
                    //output += "[EnforcedRNG]: " + room.RoomType + "\n";
                    if (room.RoomType == RoomName.Unnamed
                        || room.RoomType == RoomName.LczGreenhouse
                        || room.RoomType == RoomName.LczAirlock
                        || room.RoomType == RoomName.HczTesla
                        || room.RoomType == RoomName.EzCollapsedTunnel
                        || room.RoomType == RoomName.EzOfficeStoried
                        || room.RoomType == RoomName.EzOfficeLarge
                        || room.RoomType == RoomName.EzOfficeSmall)
                    {
                        FilteredRoomList.Add(room);
                    }
                }

                for (int i = 0; i < (amount < FilteredRoomList.Count ? amount : FilteredRoomList.Count); i++)
                {
                    int x = UnityEngine.Random.Range(0, FilteredRoomList.Count);
                    FilteredRoomList[x].LightController.NetworkLightsEnabled = false;
                    OfflineRooms.Add(FilteredRoomList[x]);
                    FilteredRoomList.RemoveAt(x);
                }
                DebugTranslator.Console(OfflineRooms.Count + " Lights turned Off");
                return amount;
            }
        }
        public static void TurnOnLights(int amount)
        {
            if (LightsOutMode) return;
            int lightsToTurn = amount < OfflineRooms.Count ? amount : OfflineRooms.Count;
            for (int i = 0; i < lightsToTurn; i++)
            {
                int x = UnityEngine.Random.Range(0, OfflineRooms.Count - 1);
                OfflineRooms[x].LightController.NetworkLightsEnabled = true;
                OfflineRooms.RemoveAt(x);
            }
            DebugTranslator.Console(amount + " Lights turned On");
        }

        public static int CheckGeneratorsOvercharge()
        {
            if (LastGeneratorCheck == -1) return 3;
            int countEngaged = 0;
            int countActive = 0;
            foreach (var generator in Map.Get.Generators)
            {
                if (generator.Engaged)
                    countEngaged++;
                else if (generator.Active)
                    countActive++;
            }
            ModifyMapOnGenerators();

            /*DebugTranslator.Console("Engaged Generators: " + countEngaged +
                "\nActive Generators: " + countActive);*/
            return countEngaged;
        }
        public static void ModifyMapOnGenerators()
        {
            if (LastGeneratorCheck == 3) return;

            int generatorCount = 0;
            foreach (var generator in Map.Get.Generators)
                if (generator.Engaged) generatorCount++;

            if (generatorCount == 3 && LastGeneratorCheck != 3)
            {
                Map.Get.Cassie("ALL FACILITY SECURITY SYSTEMS ARE NOW OPERATIONAL");
                Map.Get.SendBroadcast(10, "Security Systems ON-LINE");
                Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Intercom).Locked = false;
                TurnOnLights(MainModule.ServerConfigs.StartingLightsOff);
                LastGeneratorCheck = 3;
            }
            else if (generatorCount == 2 && LastGeneratorCheck < 2)
            {
                Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Intercom).Locked = false;
                TurnOnLights(MainModule.ServerConfigs.GeneratorLightsOn);
                LastGeneratorCheck = 2;
            }
            else if (generatorCount == 1 && LastGeneratorCheck < 1)
            {
                Map.Get.GetDoor(Synapse.Api.Enum.DoorType.Intercom).Locked = false;
                TurnOnLights(MainModule.ServerConfigs.GeneratorLightsOn);
                LastGeneratorCheck = 1;
            }
        }

        //-------------------------------------------------------------------------------
        //EVENTS
        public static void AnnounceChaos(TeamRespawnEventArgs args)
        {
            if (CheckGeneratorsOvercharge() == 3 && args.Players.Count > 0 && args.TeamID == 2)
            {
                Map.Get.Cassie("ATTENTION . ALL SECURITY PERSONNEL . CHAOS .g3 INSURGENCY HASENTERED . LETHAL FORCE .g4 AUTHORIZED");
                Map.Get.SendBroadcast(7, "Chaos Entered Facility");
            }
        }
    }
}
