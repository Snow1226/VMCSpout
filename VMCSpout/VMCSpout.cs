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
    Version: "0.0.2",
    Author: "snow",
    Description: "Spout2 sender for VMC",
    AuthorURL: "https://twitter.com/snow_mil",
    PluginURL: "https://github.com/Snow1226")]

    public class VMCSpout : MonoBehaviour
    {
        private SpoutSender _spoutSender = null;
        [SerializeField] private SpoutResources _spoutResources = null;

        private RenderTexture _mainCamRenderTexture;
        private Camera _mainCamSpoutCamera;
        private Dictionary<string, Shader> _shaders = new Dictionary<string, Shader>();

        private const int AvatarLayer = 3;

        private VMCSpoutSetting _settings = new VMCSpoutSetting();
        private Camera _currentCamera;

        private GameObject _spoutRoot = null;

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
            AssetBundle assetBundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("VMCSpout.Resources.shader"));
            _shaders = assetBundle.LoadAllAssets<Shader>().ToDictionary(x => x.name);
            assetBundle.Unload(false);

            _spoutResources = new SpoutResources();
            _spoutResources.blitShader = _shaders["Hidden/Klak/Spout/Blit"];

        }

        private void Update()
        {
            if (_currentCamera)
            {
                _mainCamSpoutCamera.fieldOfView = _currentCamera.fieldOfView;
                _mainCamSpoutCamera.transform.position = _currentCamera.transform.position;
                _mainCamSpoutCamera.transform.rotation = _currentCamera.transform.rotation;
            }

        }

        [OnSetting]
        public void OnSetting()
        {
            Debug.Log("VMC Spout Setting Opened");
            var proc = new System.Diagnostics.Process();
            string dllDirectory = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName;

            proc.StartInfo.FileName = Path.Combine(dllDirectory, "VMCSpoutSettingWPF.exe");
            proc.Start();
            proc.WaitForExit();
            SpoutCameraInitialize();
        }

        private void OnModelLoaded(GameObject currentModel)
        {
            Renderer[] renderers = currentModel.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
                renderer.gameObject.layer = AvatarLayer;
        }

        private void OnCameraChanged(Camera currentCamera)
        {
            _currentCamera = currentCamera;
            SpoutCameraInitialize();
        }

        private void SpoutCameraInitialize()
        {
            if(_currentCamera == null)
                return;

            if (_mainCamRenderTexture == null || _mainCamRenderTexture.width != _settings.MainCamOutputWidth || _mainCamRenderTexture.height != _settings.MainCamOutputHeight)
            {
                _mainCamRenderTexture?.Release();
                _mainCamRenderTexture = new RenderTexture(_settings.MainCamOutputWidth, _settings.MainCamOutputHeight, 24)
                {
                    useMipMap = false,
                    anisoLevel = 1,
                    useDynamicScale = false,
                    enableRandomWrite = true
                };
            }

            if(_currentCamera.gameObject.GetComponentsInChildren<SpoutSender>().Length > 0)
            {
                foreach (var spout in _currentCamera.gameObject.GetComponentsInChildren<SpoutSender>())
                {
                    DestroyImmediate(spout.gameObject);
                }
            }

            _mainCamSpoutCamera = Instantiate(_currentCamera);
            DestroyImmediate(_mainCamSpoutCamera.GetComponent("AudioListener"));
            _mainCamSpoutCamera.transform.SetParent(_currentCamera.transform);
            _mainCamSpoutCamera.transform.localPosition = Vector3.zero;
            _mainCamSpoutCamera.transform.localRotation = Quaternion.identity;
            _mainCamSpoutCamera.tag = "Untagged";

            _mainCamSpoutCamera.clearFlags = CameraClearFlags.SolidColor;
            _mainCamSpoutCamera.backgroundColor = new Color(0, 0, 0, 0);
            _mainCamSpoutCamera.cullingMask = 1 << AvatarLayer;
            _mainCamSpoutCamera.stereoTargetEye = StereoTargetEyeMask.None;
            _mainCamSpoutCamera.depth = -1;

            _mainCamSpoutCamera.targetTexture = _mainCamRenderTexture;

            _spoutSender = _mainCamSpoutCamera.gameObject.AddComponent<SpoutSender>();

            _spoutSender.SetResources(_spoutResources);

            _spoutSender.spoutName = _settings.MainCamSpoutName;
            _spoutSender.keepAlpha = true;
            _spoutSender.captureMethod = CaptureMethod.Texture;

            _spoutSender.sourceTexture = _mainCamRenderTexture;

            if(_spoutRoot != null)
                DestroyImmediate(_spoutRoot);

            _spoutRoot = new GameObject("VMCSpoutAdditionalCameras");
            _spoutRoot.transform.position = Vector3.zero;
            _spoutRoot.transform.rotation = Quaternion.identity;

            foreach (CameraSetting cs in _settings.AdditionalCameras)
            {
                var spCamera = Instantiate(_currentCamera);
                DestroyImmediate(spCamera.GetComponent("AudioListener"));
                spCamera.transform.SetParent(_spoutRoot.transform);
                spCamera.transform.position = _currentCamera.transform.position;
                spCamera.transform.rotation = _currentCamera.transform.rotation;
                spCamera.tag = "Untagged";

                spCamera.clearFlags = CameraClearFlags.SolidColor;
                spCamera.backgroundColor = new Color(0, 0, 0, 0);
                spCamera.cullingMask = 1 << AvatarLayer;
                spCamera.stereoTargetEye = StereoTargetEyeMask.None;
                spCamera.depth = -1;

                var additionalCam = spCamera.gameObject.AddComponent<AdditionalCamera>();
                additionalCam.Initialize(cs, _spoutResources);
            }

        }
    }
}
