using Newtonsoft.Json;

namespace VMCSpoutSettingWPF
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class VMCSpoutSetting
    {
        [JsonProperty]
        public bool ScaleSyncWithCamera { get; set; } = true;
        [JsonProperty]
        public string MainCamSpoutName { get; set; } = "VMC Spout Main";
        [JsonProperty]
        public int MainCamOutputWidth { get; set; } = 1920;
        [JsonProperty]
        public int MainCamOutputHeight { get; set; } = 1080;

        [JsonProperty]
        public CameraSetting[] AdditionalCameras { get; set; } = new CameraSetting[0] ;
    }

    public class CameraSetting
    {
        [JsonProperty]
        public string SpoutName { get; set; } = "VMC Spout 1";
        [JsonProperty]
        public int Port { get; set; } = 39640;
        [JsonProperty]
        public int OutputWidth { get; set; } = 1920;
        [JsonProperty]
        public int OutputHeight { get; set; } = 1080;

    }
}
