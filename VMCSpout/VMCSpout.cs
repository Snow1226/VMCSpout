using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using VMC;
using VMCMod;
using Klak.Spout;
using System.Reflection;
using Newtonsoft.Json;
using System;

namespace VMCSpout
{
    [VMCPlugin(
    Name: "VMC Spout",
    Version: "0.1.1",
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

        private bool _displayCamCube = false;
        private List<GameObject> _cameraCubes = new List<GameObject>();

        private void Awake()
        {
            VMCEvents.OnModelLoaded += OnModelLoaded;
            VMCEvents.OnCameraChanged += OnCameraChanged;
            LoadSetting();
        }

        private void Start()
        {
            AssetBundle assetBundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("VMCSpout.Resources.shader"));
            _shaders = assetBundle.LoadAllAssets<Shader>().ToDictionary(x => x.name);
            assetBundle.Unload(false);

            _spoutResources = SpoutResources.CreateInstance<SpoutResources>();
            _spoutResources.blitShader = _shaders["Hidden/Klak/Spout/Blit"];

        }

        private void LateUpdate()
        {
            if (_currentCamera)
            {
                _mainCamSpoutCamera.fieldOfView = _currentCamera.fieldOfView;
                _mainCamSpoutCamera.transform.position = _currentCamera.transform.position;
                _mainCamSpoutCamera.transform.rotation = _currentCamera.transform.rotation;
            }

        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                _displayCamCube = !_displayCamCube;
                foreach (var cube in _cameraCubes)
                    cube.GetComponent<MeshRenderer>().enabled = _displayCamCube;
            }

        }

        private void LoadSetting()
        {
            string dllDirectory = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location).FullName;
            if (File.Exists(Path.Combine(dllDirectory, "VMCSpoutSetting.json")))
                _settings = JsonConvert.DeserializeObject<VMCSpoutSetting>(File.ReadAllText(Path.Combine(dllDirectory, "VMCSpoutSetting.json")));
            else
            {
                _settings = new VMCSpoutSetting();
                File.WriteAllText(Path.Combine(dllDirectory, "VMCSpoutSetting.json"), JsonConvert.SerializeObject(_settings, Formatting.Indented));
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
            LoadSetting();
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
            if(_currentCamera == currentCamera)
                return;
            _currentCamera = currentCamera;
            SpoutCameraInitialize();
        }

        private void SpoutCameraInitialize()
        {
            if(_currentCamera == null || _currentCamera.gameObject.GetComponent<Camera>() == null)
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
            if (_spoutRoot != null)
                DestroyImmediate(_spoutRoot);

            _spoutRoot = new GameObject("VMCSpoutAdditionalCameras");
            _spoutRoot.transform.position = Vector3.zero;
            _spoutRoot.transform.rotation = Quaternion.identity;
            var scaleSync = _spoutRoot.gameObject.AddComponent<ScaleSync>();
            scaleSync.TargetTransform = GameObject.Find("HandTrackerRoot").transform;

            _mainCamSpoutCamera = Instantiate(_currentCamera);
            DestroyImmediate(_mainCamSpoutCamera.GetComponent("AudioListener"));
            DestroyImmediate(_mainCamSpoutCamera.GetComponent("CameraFollower"));
            DestroyImmediate(_mainCamSpoutCamera.GetComponent("CameraMirror"));
            DestroyImmediate(_mainCamSpoutCamera.GetComponent("VirtualCamera"));

            _mainCamSpoutCamera.Reset();

            string complist = "Spout Camera Main :";
            Component[] components = _mainCamSpoutCamera.GetComponents<Component>();
            foreach (var component in components)
                complist = complist + ", " + component.GetType().Name;
             Debug.Log(complist);

            _mainCamSpoutCamera.transform.SetParent(_spoutRoot.transform);
            _mainCamSpoutCamera.transform.localPosition = Vector3.zero;
            _mainCamSpoutCamera.transform.localRotation = Quaternion.identity;
            _mainCamSpoutCamera.tag = "Untagged";

            _mainCamSpoutCamera.clearFlags = CameraClearFlags.SolidColor;
            _mainCamSpoutCamera.backgroundColor = new Color(0, 0, 0, 0);
            _mainCamSpoutCamera.cullingMask = 1 << AvatarLayer;
            _mainCamSpoutCamera.stereoTargetEye = StereoTargetEyeMask.None;
            _mainCamSpoutCamera.depth = -1;

            _mainCamSpoutCamera.targetTexture = _mainCamRenderTexture;

            _mainCamSpoutCamera.gameObject.SetActive(true);

            _spoutSender = _mainCamSpoutCamera.gameObject.AddComponent<SpoutSender>();

            _spoutSender.SetResources(_spoutResources);

            _spoutSender.spoutName = _settings.MainCamSpoutName;
            _spoutSender.keepAlpha = true;
            _spoutSender.captureMethod = CaptureMethod.Texture;

            _spoutSender.sourceTexture = _mainCamRenderTexture;

            _cameraCubes.Clear();
            foreach (CameraSetting cs in _settings.AdditionalCameras)
            {

                var spCamera = Instantiate(_currentCamera);
                DestroyImmediate(spCamera.GetComponent("AudioListener"));
                DestroyImmediate(spCamera.GetComponent("CameraFollower"));
                DestroyImmediate(spCamera.GetComponent("CameraMirror"));
                DestroyImmediate(spCamera.GetComponent("VirtualCamera"));

                spCamera.Reset();

                string complist2 = "Spout Camera " + cs.SpoutName + " :";
                Component[] components2 = _mainCamSpoutCamera.GetComponents<Component>();
                foreach (var component in components2)
                    complist2 = complist2 + ", " + component.GetType().Name;
                Debug.Log(complist2);


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

                var _cameraCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _cameraCube.transform.SetParent(spCamera.transform);
                _cameraCube.transform.localPosition = Vector3.zero;
                _cameraCube.transform.localRotation = Quaternion.identity;
                _cameraCube.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
                if(_displayCamCube)
                    _cameraCube.GetComponent<MeshRenderer>().enabled = true;
                else
                    _cameraCube.GetComponent<MeshRenderer>().enabled = false;
                _cameraCubes.Add(_cameraCube);
            }
        }
    }
}
