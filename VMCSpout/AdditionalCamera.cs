using Klak.Spout;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Text;
using UnityEngine;

namespace VMCSpout
{
    public class AdditionalCamera : MonoBehaviour
    {
        private const string MemoryMapNamePrefix = "VMCSpout.Camera.";
        private const float ReopenIntervalSeconds = 0.5f;

        public Camera addCamera { get; set; }
        public SpoutSender spoutSender { get; set; }
        public RenderTexture renderTexture { get; set; }
        public CameraSetting setting { get; set; }

        private SpoutCameraData _cameraData;
        private Canvas _mirrorCanvas;
        private GameObject _cubeObject;
        private Vector3 pos = Vector3.zero;
        private Quaternion rot = Quaternion.identity;

        private MemoryMappedFile cameraDataMemoryMappedFile;
        private MemoryMappedViewAccessor cameraDataAccessor;
        private byte[] cameraDataBuffer;
        private string memoryMapName;
        private float nextOpenAttemptTime;

        public void Initialize(CameraSetting camSetting,SpoutResources spoutResources)
        {
            this.gameObject.SetActive(false);
            setting = camSetting;
            addCamera = this.gameObject.GetComponent<Camera>();
            memoryMapName = MemoryMapNamePrefix + setting.SpoutName;

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

            _mirrorCanvas = this.gameObject.GetComponentInChildren<Canvas>(true);
            _cubeObject = this.transform.GetComponentInChildren<MeshRenderer>(true).gameObject;

            addCamera.enabled = false;
            _mirrorCanvas.gameObject.SetActive(false);
            _cubeObject.SetActive(false);
            this.gameObject.SetActive(true);
        }

        private void LateUpdate()
        {
            if (!this.isActiveAndEnabled || addCamera == null)
                return;

            if (!TryReadCameraData(out _cameraData))
            {
                addCamera.enabled = false;
                _mirrorCanvas.gameObject.SetActive(false);
                _cubeObject.SetActive(false);
                return;
            }

            if (_cameraData.Position == null || _cameraData.Position.Length < 3 || _cameraData.Rotation == null || _cameraData.Rotation.Length < 4) 
            {
                addCamera.enabled = false;
                _mirrorCanvas.gameObject.SetActive(false);
                _cubeObject.SetActive(false);
                return;
            }

            pos.x = _cameraData.Position[0];
            pos.y = _cameraData.Position[1];
            pos.z = _cameraData.Position[2];
            rot.x = _cameraData.Rotation[0];
            rot.y = _cameraData.Rotation[1];
            rot.z = _cameraData.Rotation[2];
            rot.w = _cameraData.Rotation[3];

            if (!IsValidFloat(pos.x) || !IsValidFloat(pos.y) || !IsValidFloat(pos.z)
                || !IsValidFloat(rot.x) || !IsValidFloat(rot.y) || !IsValidFloat(rot.z) || !IsValidFloat(rot.w)
                || !IsValidFloat(_cameraData.Fov))
                return;

            addCamera.enabled = _cameraData.CameraEnabled;
            _mirrorCanvas.gameObject.SetActive(_cameraData.CameraEnabled);
            _cubeObject.SetActive(_cameraData.CameraEnabled);

            this.gameObject.transform.localPosition = pos;
            this.gameObject.transform.localRotation = rot;
            this.addCamera.fieldOfView = _cameraData.Fov;
        }

        private bool TryReadCameraData(out SpoutCameraData cameraData)
        {
            cameraData = default(SpoutCameraData);

            if (cameraDataAccessor == null && !TryOpenMemoryMap())
            {
                return false;
            }

            try
            {
                EnsureBufferSize();
                cameraDataAccessor.ReadArray(0, cameraDataBuffer, 0, cameraDataBuffer.Length);

                var json = ReadJsonPayload();
                if (string.IsNullOrWhiteSpace(json))
                {
                    return false;
                }

                cameraData = JsonConvert.DeserializeObject<SpoutCameraData>(json);
                return true;
            }
            catch (IOException)
            {
                ReleaseMemoryMap();
                return false;
            }
            catch (ObjectDisposedException)
            {
                ReleaseMemoryMap();
                return false;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private bool TryOpenMemoryMap()
        {
            if (Time.unscaledTime < nextOpenAttemptTime)
            {
                return false;
            }

            ReleaseMemoryMap();

            try
            {
                cameraDataMemoryMappedFile = MemoryMappedFile.OpenExisting(memoryMapName, MemoryMappedFileRights.Read);
                cameraDataAccessor = cameraDataMemoryMappedFile.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
                cameraDataBuffer = null;
                return true;
            }
            catch (FileNotFoundException)
            {
                nextOpenAttemptTime = Time.unscaledTime + ReopenIntervalSeconds;
                return false;
            }
        }

        private void EnsureBufferSize()
        {
            var capacity = checked((int)cameraDataAccessor.Capacity);
            if (cameraDataBuffer == null || cameraDataBuffer.Length != capacity)
            {
                cameraDataBuffer = new byte[capacity];
            }
        }

        private string ReadJsonPayload()
        {
            if (cameraDataBuffer == null || cameraDataBuffer.Length == 0)
            {
                return string.Empty;
            }

            if (cameraDataBuffer.Length > sizeof(int))
            {
                var length = BitConverter.ToInt32(cameraDataBuffer, 0);
                if (length > 0 && length <= cameraDataBuffer.Length - sizeof(int))
                {
                    return Encoding.UTF8.GetString(cameraDataBuffer, sizeof(int), length);
                }
            }

            var endIndex = Array.IndexOf(cameraDataBuffer, (byte)0);
            if (endIndex < 0)
            {
                endIndex = cameraDataBuffer.Length;
            }

            if (endIndex == 0)
            {
                return string.Empty;
            }

            return Encoding.UTF8.GetString(cameraDataBuffer, 0, endIndex).Trim();
        }

        private static bool IsValidFloat(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private void ReleaseMemoryMap()
        {
            if (cameraDataBuffer != null)
            {
                Array.Clear(cameraDataBuffer, 0, cameraDataBuffer.Length);
                cameraDataBuffer = null;
            }

            if (cameraDataAccessor != null)
            {
                cameraDataAccessor.Dispose();
                cameraDataAccessor = null;
            }

            if (cameraDataMemoryMappedFile != null)
            {
                cameraDataMemoryMappedFile.Dispose();
                cameraDataMemoryMappedFile = null;
            }
        }

        private void OnDestroy()
        {
            ReleaseMemoryMap();

            nextOpenAttemptTime = 0f;
            memoryMapName = null;
            _cameraData = default(SpoutCameraData);

            if(spoutSender != null)
            {
                spoutSender.enabled = false;
                spoutSender.sourceTexture = null;
                Destroy(spoutSender);
                spoutSender = null;
            }

            if (addCamera != null)
            {
                addCamera.targetTexture = null;
                addCamera = null;
            }

            if(renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
                renderTexture = null;
            }

            _mirrorCanvas = null;
            _cubeObject = null;
        }
    }

    public struct SpoutCameraData
    {
        public bool CameraEnabled;
        public string Name;
        public float Fov;
        public float[] Position;
        public float[] Rotation;
        public SpoutCameraData(bool cameraEnabled, string name, Vector3 pos, Quaternion rot, float fov)
        {
            this.CameraEnabled = cameraEnabled;
            this.Name = name;
            this.Position = new float[] { pos.x, pos.y, pos.z };
            this.Rotation = new float[] { rot.x, rot.y, rot.z, rot.w };
            this.Fov = fov;
        }
    }
}
