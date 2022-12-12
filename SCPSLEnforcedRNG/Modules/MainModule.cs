using MEC;
using Synapse.Api.Events.SynapseEventArguments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLEnforcedRNG.Modules
{
    //Officer Card == Private Card
    //Lieutenant Card == Sergant Card
    //Commander Card == Captain Card


    public class MainModule : BaseModule
    {
        //-------------------------------------------------------------------------------
        //Overrides
        public override string ModuleName { get { return "MainModule"; } }
        public override void Activate()
        {
            ServerConfigs = PluginClass.ServerConfigs;
            SynapseController.Server.Events.Round.RoundStartEvent += StartRound;
            SynapseController.Server.Events.Player.PlayerJoinEvent += CommitPlayer;
            SynapseController.Server.Events.Player.PlayerLeaveEvent += RemovePlayer;
            SynapseController.Server.Events.Round.RoundEndEvent += ClearPlayers;
            SynapseController.Server.Events.Round.RoundRestartEvent += ClearPlayers;
            
            ReadyModules.Add(this);
            if (ServerConfigs.BetterRolePicksModuleActive) ReadyModules.Add(new BetterRolePicks());
            if (ServerConfigs.BetterRespawnModuleActive) ReadyModules.Add(new BetterRespawns());
            if (ServerConfigs.MoreGeneratorFunctionsModuleActive) ReadyModules.Add(new MoreGeneratorFunctions());
            if (ServerConfigs.AntiCampModuleActive) ReadyModules.Add(new AntiCamp());
            if (ServerConfigs.StatsModuleActive) ReadyModules.Add(new Stats());
            if (ServerConfigs.FacilityScanModuleActive) ReadyModules.Add(new FacilityScan());
            if (ServerConfigs.ImprovedWarHeadsModuleActive) ReadyModules.Add(new ImprovedWarHeads());
            if (ServerConfigs.UsefulRadioModuleActive) ReadyModules.Add(new UsefulStuff());
            if (ServerConfigs.SchematicsModuleActive) ReadyModules.Add(new SchematicsModule());
            if (true) ReadyModules.Add(new GuardEscapeModule());
            if (true) ReadyModules.Add(new TrollStuff());

            foreach (var module in ReadyModules) if (module.ModuleName!="MainModule") module.Activate();
            
        }
        public override void SetUpRound()
        {
            RoundStartTime = Timing.LocalTime;
            foreach (var module in ReadyModules) if (module.ModuleName != "MainModule") module.SetUpRound();
        }
        //-------------------------------------------------------------------------------
        //Main

        public static PluginConfig ServerConfigs { get; set; }
        public static List<BaseModule> ReadyModules { get; set; } = new();
        public static float RoundStartTime { get; set; }

        public static BaseModule GetModule(string name)
        {
            foreach(var module in ReadyModules)
            {
                if (module.ModuleName == name) return module;
            }
            return null;
        }
        public static int GetRoleAmount(int role)
        {
            int count = 0;
            foreach (var player in PlayerInfo.playerList)
                if (player.PlayerPtr.RoleID == role)
                    count++;
            return count;
        }

        //-------------------------------------------------------------------------------
        //EVENTS
        public static void RemovePlayer(PlayerLeaveEventArgs args)
        {

            foreach (var player in PlayerInfo.playerList)
                if (player.PlayerId == args.Player.UserId)
                {
                    PlayerInfo.playerList.Remove(player);
                    break;
                }
            DebugTranslator.Console("Player " + args.Player.NickName + " has left the server. Players Left: " + PlayerInfo.playerList.Count, 0, true);
        }
        public static void CommitPlayer(PlayerJoinEventArgs args)
        {
            Timing.CallDelayed(2f, () =>
            {
                DebugTranslator.Console(PlayerInfo.playerList.Count.ToString(), 1);

                PlayerInfo.playerList.Add(new PlayerInfo(args.Player, args.Player.UserId));

                DebugTranslator.Console(
                    "Player Joined.\n" +
                    "PlayerID: " + args.Player.PlayerId + "\n" +
                    "SteamID: " + args.Player.UserId
                    , 0, true);
            });
        }
        public void StartRound()
        {
            SetUpRound();
            DebugTranslator.Console("ROUND STARTED", 0, true);
        }
        public void ClearPlayers()
        {
            PlayerInfo.playerCount = 0;
            Timing.CallDelayed(1f, () => { PlayerInfo.playerList.Clear(); });
        }

        //-------------------------------------------------------------------------------
        //RANDOMS
        private static double randomSeed = 0;
        public static int RandomTimeExclusivePos()
        {
            TimeSpan span = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
            double number = span.TotalMilliseconds;
            //while (number > (int.MaxValue - 1)) number -= int.MaxValue;

            number = number % (int.MaxValue-1);
            //int x = Convert.ToInt32(number);
            int x = (int)number;

            return x;
        }
        public static int RandomTimeExclusivePos(int max)
        {
            if (max <= 0) { DebugTranslator.Console("Random Generation Failed: MAX <= 0"); return 0; }

            TimeSpan span = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
            double number = span.TotalMilliseconds;
            number = number % int.MaxValue;
            int x = (int)number;


            return x % (max + 1);
        }
        public static int RandomTimeExclusivePos(int min, int max)
        {
            if (min < 0) { DebugTranslator.Console("Random Generation Failed: MIN < 0"); return 0; }
            if (max <= 0) { DebugTranslator.Console("Random Generation Failed: MAX <= 0"); return 0; }
            if (min > max) {DebugTranslator.Console("Random Generation Failed: MIN > MAX"); return min - 1; }

            TimeSpan span = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
            double number = span.TotalMilliseconds;
            number = number % int.MaxValue;
            int x = (int)number;


            return (x % (max-min + 1)) + min;
        }
        public static int RandomTimeSeededPos()
        {
            TimeSpan span = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
            double number = span.TotalMilliseconds;
            number = number + randomSeed;

            string num = number.ToString();
            if (num.Contains(".")) randomSeed = double.Parse(num.Substring(num.IndexOf('.') - 6, 5));
            else randomSeed = double.Parse(num.Substring(num.Length - 6));
            randomSeed *= randomSeed;

            DebugTranslator.Console(num + " | " + randomSeed);


            number = number % int.MaxValue;
            int x = (int)number;

            return x;
        }
        public static int RandomTimeSeededPos(int max)
        {
            if (max <= 0) { DebugTranslator.Console("Random Generation Failed: MAX <= 0"); return 0; }

            TimeSpan span = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
            double number = span.TotalMilliseconds;
            number = number + randomSeed;

            string num = number.ToString();
            if (num.Contains(".")) randomSeed = double.Parse(num.Substring(num.IndexOf('.') - 6, 5));
            else randomSeed = double.Parse(num.Substring(num.Length - 6));
            randomSeed *= randomSeed;

            number = number % int.MaxValue;
            int x = (int)number;

            return x % (max + 1);
        }
        public static int RandomTimeSeededPos(int min, int max)
        {
            if (min < 0) { DebugTranslator.Console("Random Generation Failed: MIN < 0"); return 0; }
            if (max <= 0) { DebugTranslator.Console("Random Generation Failed: MAX <= 0"); return 0; }
            if (min > max) { DebugTranslator.Console("Random Generation Failed: MIN > MAX"); return min - 1; }

            TimeSpan span = DateTime.Now.Subtract(new DateTime(1970, 1, 1, 0, 0, 0));
            double number = span.TotalMilliseconds;
            number = number + randomSeed;

            string num = number.ToString();
            if (num.Contains(".")) randomSeed = double.Parse(num.Substring(num.IndexOf('.') - 6, 5));
            else randomSeed = double.Parse(num.Substring(num.Length - 6));
            randomSeed *= randomSeed;

            number = number % int.MaxValue;
            int x = (int)number;

            return (x % (max - min + 1)) + min;
        }


        public static void TestRandom(int customMin = 0, int customMax = 0, int tests = 4)
        {
            string output = "";

            output += "Random Time Exclusive: " + RandomTimeExclusivePos() + "\n";
            output += "Random Time Exclusive (MAX 150): " + RandomTimeExclusivePos(150) + "\n";
            output += "Random Time Exclusive (MAX 87 477 231): " + RandomTimeExclusivePos(87477231) + "\n";
            output += "Random Time Exclusive (MAX " + customMax + "): " + RandomTimeExclusivePos(customMax) + "\n";
            output += "Random Time Exclusive (MIN 5) (MAX 100): " + RandomTimeExclusivePos(5, 100) + "\n";
            output += "Random Time Exclusive (MIN 5 871) (MAX 14 568 101): " + RandomTimeExclusivePos(5871, 14568101) + "\n";
            output += "Random Time Exclusive (MIN " + customMin + ") (MAX " + customMax + "): " + RandomTimeExclusivePos(customMin, customMax) + "\n";
            for (int i = 0; i < tests; i++) output += "Random Time Exclusive Test (MAX 20) #" + i + ": " + RandomTimeExclusivePos(20) + "\n";
            output += "-----------------------------------------------\n";

            output += "Random Time Seeded: " + RandomTimeSeededPos() + "\n";
            output += "Random Time Seeded (MAX 150): " + RandomTimeSeededPos(150) + "\n";
            output += "Random Time Seeded (MAX 87 477 231): " + RandomTimeSeededPos(87477231) + "\n";
            output += "Random Time Seeded (MAX " + customMax + "): " + RandomTimeSeededPos(customMax) + "\n";
            output += "Random Time Seeded (MIN 5) (MAX 100): " + RandomTimeSeededPos(5, 100) + "\n";
            output += "Random Time Seeded (MIN 5 871) (MAX 14 568 101): " + RandomTimeSeededPos(5871, 14568101) + "\n";
            output += "Random Time Seeded (MIN " + customMin + ") (MAX " + customMax + "): " + RandomTimeSeededPos(customMin, customMax) + "\n";
            for (int i = 0; i < tests; i++) output += "Random Time Seeded Test (MAX 20) #" + i + ": " + RandomTimeSeededPos(20) + "\n";
            output += "-----------------------------------------------";

            DebugTranslator.Console(output,1,true);
        }
    }
}
