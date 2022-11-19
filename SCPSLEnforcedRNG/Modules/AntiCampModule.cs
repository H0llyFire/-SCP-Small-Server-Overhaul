using MapGeneration;
using MEC;
using Synapse;
using Synapse.Api;
using Synapse.Api.Events.SynapseEventArguments;

namespace SCPSLEnforcedRNG.Modules
{
    //Check if a doggo exists every 20 seconds instead, check in coroutines maybe for every doggo
    public class AntiCamp : BaseModule
    {
        //-------------------------------------------------------------------------------
        //Overrides
        public override string ModuleName { get { return "AntiCamp"; } }
        public override void Activate()
        {
            SynapseController.Server.Events.Player.PlayerDeathEvent += CheckDoggoLiving;
        }
        public override void SetUpRound()
        {
            Timing.KillCoroutines(doggoAlive);
            Timing.KillCoroutines(doggoLightsFlash);
        }

        //-------------------------------------------------------------------------------
        //Main
        private static Room doggoRoom;
        private static int doggoCounter;
        public static PlayerInfo doggoPtr;
        public static CoroutineHandle doggoLightsFlash;
        public static CoroutineHandle doggoAlive;

        public static IEnumerator<float> DoggoCampTimer()
        { //10s; 2s check
            for (; ; )
            {
                yield return Timing.WaitForSeconds(2f);
                if (doggoRoom != null && doggoRoom == doggoPtr.PlayerPtr.Room)
                {
                    //DebugTranslator.Console(doggoRoom.RoomName+"\n"+doggoCounter, 1);

                    if (doggoCounter == 3)
                    {
                        //FuckUp Lights
                        doggoLightsFlash = Timing.RunCoroutine(FlashingLightsTimer());
                        doggoCounter++;
                    }
                    else
                    {
                        doggoCounter++;
                    }
                }
                else
                {
                    //UnFuckUp Lights
                    if (doggoRoom != null)
                    {
                        Timing.KillCoroutines(doggoLightsFlash);
                    }
                    doggoCounter = 0;
                    doggoRoom = doggoPtr.PlayerPtr.Room;
                    //DebugTranslator.Console(doggoRoom.RoomName, 1);
                }
            }
        }
        public static IEnumerator<float> FlashingLightsTimer()
        {
            for (; ; )
            {
                if (doggoRoom.RoomType == RoomName.Outside)
                    yield return Timing.WaitForSeconds(20f);
                if ((!MoreGeneratorFunctions.OfflineRooms.Contains(doggoRoom)))
                    doggoRoom.LightsOut(0.2f);
                //DebugTranslator.Console("FLASH");
                yield return Timing.WaitForSeconds(5f);
            }
        }

        //-------------------------------------------------------------------------------
        //Events
        public static void CheckDoggoLiving(PlayerDeathEventArgs args)
        {
            if (args.Victim.RoleType == RoleType.Scp93953)
            {
                Timing.KillCoroutines(doggoAlive);
                Timing.KillCoroutines(doggoLightsFlash);
            }
        }
    }
}
