using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using VMC;
using VMCMod;
using Klak.Spout;
using System.Reflection;
using Newtonsoft.Json;

namespace VMCSpout
{
    [VMCPlugin(
    Name: "VMC Spout",
    Version: "0.0.1",
    Author: "snow",
    Description: "Spout2 sender for VMC",
    AuthorURL: "https://twitter.com/snow_mil",
    PluginURL: "https://github.com/Snow1226")]

    public class VMCSpout : MonoBehaviour
    {
        private SpoutSender _spoutSender = null;
        [SerializeField] private SpoutResources _spoutResources = null;

        private RenderTexture _renderTexture;
        private Camera _spoutCamera;
        private Dictionary<string, Shader> _shaders = new Dictionary<string, Shader>();

        private const int AvatarLayer = 3;

        private VMCSpoutSetting _settings = new VMCSpoutSetting();

        private void Awake()
        {
            VMCEvents.OnModelLoaded += OnModelLoaded;
            VMCEvents.OnCameraChanged += OnCameraChanged;

            string dllDirectory = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName;
            if (File.Exists(Path.Combine(dllDirectory, "VMCSpoutSetting.json")))
                _settings = JsonConvert.DeserializeObject<VMCSpoutSetting>(File.ReadAllText(Path.Combine(dllDirectory, "VMCSpoutSetting.json")));

            else
            {
                _settings = new VMCSpoutSetting();
                File.WriteAllText(Path.Combine(dllDirectory, "VMCSpoutSetting.json"), JsonConvert.SerializeObject(_settings, Formatting.Indented));
            }
        }

        private void Start()
        {
            if (_renderTexture == null)
            {
                CreateRenderTexture();
            }

            AssetBundle assetBundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("VMCSpout.Resources.shader"));
            _shaders = assetBundle.LoadAllAssets<Shader>().ToDictionary(x => x.name);
            assetBundle.Unload(false);

            if (GameObject.Find("VMCSpout") == null)
            {
                CreateSpout();
            }
        }

        private void update()
        {
            _spoutCamera.fieldOfView = Camera.main.fieldOfView;

        }
        private void OnModelLoaded(GameObject currentModel)
        {
            SkinnedMeshRenderer[] skinnedMeshRenderers = currentModel.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var smr in skinnedMeshRenderers)
                smr.gameObject.layer = AvatarLayer;
            MeshRenderer[] meshRenderers = currentModel.GetComponentsInChildren<MeshRenderer>();
            foreach (var mr in meshRenderers)
                mr.gameObject.layer = AvatarLayer;
        }

        private void OnCameraChanged(Camera currentCamera)
        {
            if (_renderTexture == null)
            {
                CreateRenderTexture();
            }

            _spoutCamera = Instantiate(currentCamera);
            DestroyImmediate(_spoutCamera.GetComponent("AudioListener"));
            _spoutCamera.transform.SetParent(currentCamera.transform);
            _spoutCamera.transform.localPosition = Vector3.zero;
            _spoutCamera.transform.localRotation = Quaternion.identity;
            _spoutCamera.tag = "Untagged";

            _spoutCamera.clearFlags = CameraClearFlags.SolidColor;
            _spoutCamera.backgroundColor = new Color(0, 0, 0, 0);
            _spoutCamera.cullingMask = 1 << AvatarLayer;
            _spoutCamera.stereoTargetEye = StereoTargetEyeMask.None;
            _spoutCamera.depth = -1;

            _spoutCamera.targetTexture = _renderTexture;

        }

        private void CreateRenderTexture()
        {
            _renderTexture = new RenderTexture(_settings.OutputWidth, _settings.OutputHeight, 24)
            {
                useMipMap = false,
                anisoLevel = 1,
                useDynamicScale = false,
                enableRandomWrite = true
            };
            Debug.Log("VMCSpout: RenderTexture created.");
        }

        private void CreateSpout()
        {
            var obj = new GameObject("VMCSpout");

            _spoutResources = new SpoutResources();
            _spoutResources.blitShader = _shaders["Hidden/Klak/Spout/Blit"];

            _spoutSender = obj.AddComponent<SpoutSender>();

            _spoutSender.SetResources(_spoutResources);

            _spoutSender.spoutName = _settings.SpoutName;
            _spoutSender.keepAlpha = true;
            _spoutSender.captureMethod = CaptureMethod.Texture;
            
            _spoutSender.sourceTexture = _renderTexture;
            
            Debug.Log("VMCSpout: SpoutObnject Create");
        }
    }
}
