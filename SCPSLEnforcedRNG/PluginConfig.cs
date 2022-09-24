using Synapse.Config;

namespace SCPSLEnforcedRNG 
{ 
    public class PluginConfig : AbstractConfigSection
    {
        public string RolePicks { get; set; } = "303242334312303432";

        public float chaosChance { get; set; } = 0.25f;

        public int respawnTime { get; set; } = 420;

        public int respawnTimeRange { get; set; } = 30;

        public bool LightsOutMode { get; set; } = false;

        public int StartingLightsOff { get; set; } = 12;
    }
}
