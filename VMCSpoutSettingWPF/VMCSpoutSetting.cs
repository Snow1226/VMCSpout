using Newtonsoft.Json;

namespace VMCSpoutSettingWPF
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class VMCSpoutSetting
    {
        [JsonProperty]
        public string MainCamSpoutName { get; set; } = "VMC Spout Main";
        [JsonProperty]
        public int MainCamOutputWidth { get; set; } = 1920;
        [JsonProperty]
        public int MainCamOutputHeight { get; set; } = 1080;

        [JsonProperty]
        public bool UseMirror { get; set; } = true;
        [JsonProperty]
        public int MirrorResolution { get; set; } = 1024;

        [JsonProperty]
        public float MirrorWidth { get; set; } = 3;
        [JsonProperty]
        public float MirrorHeight { get; set; } = 2;
        [JsonProperty]
        public bool FollowMirrorPosition { get; set; } = false;
        [JsonProperty]
        public float MirrorPositionX { get; set; } = 0;
        [JsonProperty]
        public float MirrorPositionY { get; set; } = 0;
        [JsonProperty]
        public float MirrorPositionZ { get; set; } = 0;
        [JsonProperty]
        public float MirrorRotationY { get; set; } = 0;

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
