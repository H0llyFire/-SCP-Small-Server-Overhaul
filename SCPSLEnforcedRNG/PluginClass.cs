using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using MapGeneration;
using MEC;
using SCPSLEnforcedRNG.Modules;
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
    //Game stats, advanced debug system

    //SCP Blackout? Open and Lockdown of doors + darkness

    //Refinery updates
    //Heals and damage to players
    //flashlight no recipe
    //
    

    [PluginInformation(
        Name = "SmallServerOverhaul", 
        Author = "H0llyFire",
        Description = 
            "An overhaul for small private servers. Plugin will have undefined behaviour on servers with more than 20 people, or servers with high traffic. Optimized for 7-14 players.",
        LoadPriority = 0,
        SynapseMajor = 2,
        SynapseMinor = 10,
        SynapsePatch = 1,
        Version = "v.1.1.7a"
        )]
    public class PluginClass : AbstractPlugin
    {
        [Config(section = "EnforcedRNG")]
        public static PluginConfig ServerConfigs { get; set; }

        public override void Load()
        {
            StatTrack.SetUpCurrentSession();
            new MainModule().Activate();
            DebugTranslator.Console("PLUGIN LOADED SUCCESSFULLY", 0, true);
        }

        
    } 
}