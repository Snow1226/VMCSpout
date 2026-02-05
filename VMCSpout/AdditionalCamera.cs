using UnityEngine;
using uOSC;
using Klak.Spout;
using System.Runtime;

namespace VMCSpout
{
    public class AdditionalCamera : MonoBehaviour
    {
        public Camera addCamera { get; set; }
        public SpoutSender spoutSender { get; set; }
        public RenderTexture renderTexture { get; set; }
        public CameraSetting setting { get; set; }

        private uOscServer oscServer;
        private Vector3 pos = Vector3.zero;
        private Quaternion rot = Quaternion.identity;

        public void Initialize(CameraSetting camSetting,SpoutResources spoutResources)
        {
            this.gameObject.SetActive(false);
            setting = camSetting;
            addCamera = this.gameObject.GetComponent<Camera>();

            renderTexture = new RenderTexture(setting.OutputWidth, setting.OutputHeight, 24)
            {
                useMipMap = false,
                anisoLevel = 1,
                useDynamicScale = false,
                enableRandomWrite = true
            };
            addCamera.targetTexture = renderTexture;

            spoutSender = this.gameObject.AddComponent<SpoutSender>();

            spoutSender.SetResources(spoutResources);

            spoutSender.spoutName = setting.SpoutName;
            spoutSender.keepAlpha = true;
            spoutSender.captureMethod = CaptureMethod.Texture;

            spoutSender.sourceTexture = renderTexture;

            oscServer = this.gameObject.AddComponent<uOscServer>();
            oscServer.port = setting.Port;
            oscServer.onDataReceived.AddListener(OnDataReceived);

            this.gameObject.SetActive(true);
        }

        private void OnDataReceived(uOSC.Message message)
        {
            if (this.isActiveAndEnabled)
            {
                if (message.address == "/VMC/Ext/Cam"
                    && (message.values[0] is string)
                    && (message.values[1] is float)
                    && (message.values[2] is float)
                    && (message.values[3] is float)
                    && (message.values[4] is float)
                    && (message.values[5] is float)
                    && (message.values[6] is float)
                    && (message.values[7] is float)
                    && (message.values[8] is float)
                )
                {
                    pos.x = (float)message.values[1];
                    pos.y = (float)message.values[2];
                    pos.z = (float)message.values[3];
                    rot.x = (float)message.values[4];
                    rot.y = (float)message.values[5];
                    rot.z = (float)message.values[6];
                    rot.w = (float)message.values[7];
                    float fov = (float)message.values[8];

                    this.gameObject.transform.localPosition = pos;
                    this.gameObject.transform.localRotation = rot;
                    this.addCamera.fieldOfView = fov;
                }
            }
        }
        private void OnDestroy()
        {
            if(renderTexture != null)
            {
                renderTexture.Release();
                renderTexture = null;
            }
            if (oscServer != null)
            {
                oscServer.StopServer();
                oscServer = null;
            }
        }
    }
}
