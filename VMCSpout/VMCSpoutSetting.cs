using System.Collections.Generic;
using Newtonsoft.Json;

namespace VMCSpout
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class VMCSpoutSetting
    {
        [JsonProperty]
        public string SpoutName { get; set; } = "VMC Spout 1";
        [JsonProperty]
        public int OutputWidth { get; set; } = 1920;
        [JsonProperty]
        public int OutputHeight { get; set; } = 1080;
    }
}
