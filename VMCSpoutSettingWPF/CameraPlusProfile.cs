using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VMCSpoutSettingWPF
{
    public class CameraPlusProfile
    {
        public bool IsEnabled { get; set; }
        public string Name { get; set; }
        public Dictionary<string, CameraPlusSetting> CameraPlusSettings { get; set; }
    }

    public class CameraPlusSetting
    {
        public bool IsEnabled { get; set; }
        public string CameraName { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class SpoutCameraElements
    {
        [JsonProperty("SpoutReceiverName")]
        public string reciverName = string.Empty;
        [JsonProperty("AutoConnect")]
        public bool reciverAutoConnect = false;
        [JsonProperty("SpoutSenderName")]
        public string senderName = string.Empty;
        [JsonProperty("SpoutSenderWidth")]
        public int senderWidth = 1920;
        [JsonProperty("SpoutSenderHeight")]
        public int senderHeight = 1080;
    }

    public class vmcProtocolElements
    {
        [JsonConverter(typeof(StringEnumConverter)), JsonProperty("Mode")]
        internal VMCProtocolMode mode = VMCProtocolMode.Disable;
        [JsonProperty("Address")]
        public string address = "127.0.0.1";
        [JsonProperty("Sender Port")]
        public int port = 39540;
        [JsonProperty("Receiver Port")]
        public int receiverPort = 39540;
    }
    internal enum VMCProtocolMode
    {
        Disable,
        Sender,
        Receiver
    }
}
