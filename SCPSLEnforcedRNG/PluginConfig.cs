using Synapse.Config;

namespace SCPSLEnforcedRNG 
{ 
    public class PluginConfig : AbstractConfigSection
    {
        public bool ShowDebugInConsole { get; set; } = true;
        public string RolePicks { get; set; } = "303242334312303432";

        public float chaosChance { get; set; } = 0.25f;

        public int respawnTime { get; set; } = 420;

        public int respawnTimeRange { get; set; } = 30;

        public bool LightsOutMode { get; set; } = false;

        public int StartingLightsOff { get; set; } = 12;

        public int GeneratorLightsOn { get; set; } = 6;


        public bool EnableCustomRoleAssignment { get; set; } = true;
        public bool EnableOmegaWarhead { get; set; } = true;
        public bool EnableAfterRoundStats { get; set; } = true;
        public bool EnableGeneratorChanges { get; set; } = true;
    }
}
