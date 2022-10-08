using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using MapGeneration;
using MEC;
using Synapse;
using Synapse.Api;
using Synapse.Api.Events.SynapseEventArguments;
using Synapse.Api.Items;
using Synapse.Api.Plugin;
using static System.Net.WebRequestMethods;

namespace SCPSLEnforcedRNG
{
    //TODO:
    //New class xd Chaos Guard (chance for a guard to be Chaos with the skin of a guard (amogus)
    //Make preffered role system
    //4th respawn Zetta Wave?

    //Refinery updates
    //Heals and damage to players
    //flashlight no recipe
    //
    

    [PluginInformation(
        Name = "EnforcedRNG", //The Name of Your Plugin
        Author = "H0llyFire", // Your Name
        Description = "The RNG Manipulator 9000. Not fit for large or public servers", // A Description of your Plugin
        LoadPriority = 0, //When your Plugin should get loaded (use 0 if you don't know how to use it)
        SynapseMajor = 2, //The Synapse Version for which this Plugin was created for (SynapseMajor.SynapseMinor.SynapsePatch => 2.7.0)
        SynapseMinor = 10,
        SynapsePatch = 1,
        Version = "v.1.1.2" //The Current Version of your Plugin
        )]
    public class PluginClass : AbstractPlugin
    {
        [Config(section = "EnforcedRNG")]
        public static PluginConfig ServerConfigs { get; set; }

        public override void Load()
        {
            EventCalls.SetupEvents(ServerConfigs);

            Timing.CallDelayed(3f, () =>
            {
                //Map.Get.Scp914.
            });

            DebugTranslator.Console("PLUGIN LOADED SUCCESSFULLY", 0, true);
        }

        
    } 
}