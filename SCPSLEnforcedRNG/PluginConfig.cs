using Synapse.Config;
using System.ComponentModel;

namespace SCPSLEnforcedRNG 
{ 
    public class PluginConfig : AbstractConfigSection
    {
        public bool ShowDebugInConsole { get; set; } = true;
        //-----------------------------------------------------------------------
        public bool MainModuleActive { get; set; } = true;
        public bool BetterRolePicksModuleActive { get; set; } = true;
        public string RolePicks { get; set; } = "303242334312303432";
        //-----------------------------------------------------------------------
        public bool BetterRespawnModuleActive { get; set; } = true;
        public float chaosChance { get; set; } = 0.25f;
        public int respawnTime { get; set; } = 420;
        public int respawnTimeRange { get; set; } = 30;
        //-----------------------------------------------------------------------
        public bool AntiCampModuleActive { get; set; } = true;
        //-----------------------------------------------------------------------
        public bool MoreGeneratorFunctionsModuleActive { get; set; } = true;
        public bool LightsOutMode { get; set; } = false;
        public int StartingLightsOff { get; set; } = 12;
        public int GeneratorLightsOn { get; set; } = 6;
        public bool ChaosAnnounce { get; set; } = true;
        //-----------------------------------------------------------------------
        public bool StatsModuleActive { get; set; } = true;
        public bool EnableAfterRoundStats { get; set; } = true;
        //-----------------------------------------------------------------------
        public bool FacilityScanModuleActive { get; set; } = true;
        //-----------------------------------------------------------------------
        public bool ImprovedWarHeadsModuleActive { get; set; } = true;
        public bool EnableOmegaWarhead { get; set; } = true;
        //-----------------------------------------------------------------------
        public bool UsefulRadioModuleActive { get; set; } = true;
        //-----------------------------------------------------------------------
        //-----------------------------------------------------------------------
        //-----------------------------------------------------------------------
        //-----------------------------------------------------------------------
        //-----------------------------------------------------------------------
        //-----------------------------------------------------------------------

    }
}
