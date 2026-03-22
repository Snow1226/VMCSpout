using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace VMCSpout
{
    public class Mirror : MonoBehaviour
    {
        public Transform target;
        private GameObject _targetFloorObject;
        public Camera renderCamera;

        public Material mirrorMaterial;
        public RenderTexture mirrorTexture = null;

        private RawImage _rawImage;
        private Vector3 _center = Vector3.zero;
        private Vector3 _mirrorEular = new Vector3(90, 0, 0);
        private float _mirrorRectWidth = 10f;
        private float _mirrorRectHeight = 10f;


        private Camera _mirrorCamera;
        private readonly int _texId = Shader.PropertyToID("_MainTex");

        private void Awake()
        {
            _rawImage = GetComponent<RawImage>();
        }

        private void Start()
        {
        }

        private void OnDestroy()
        {
            if (mirrorTexture != null)
            {
                mirrorTexture.Release();
                Destroy(mirrorTexture);
            }
            if (mirrorMaterial != null)
            {
                Destroy(mirrorMaterial);
            }
        }

        public void Initialize(int textureSize, float mirrorWidth, float mirrorHeight)
        {
            _targetFloorObject = GameObject.Find("AvatarCenterFloor");
            if (_targetFloorObject == null)
                _targetFloorObject = new GameObject("AvatarCenterFloor");

            mirrorTexture = new RenderTexture(textureSize, textureSize, 24, RenderTextureFormat.ARGB32);

            _mirrorRectWidth = mirrorWidth;
            _mirrorRectHeight = mirrorHeight;

            _mirrorCamera = this.gameObject.AddComponent<Camera>();
            _mirrorCamera.enabled = false;
            _mirrorCamera.hideFlags = HideFlags.HideAndDontSave;
            _mirrorCamera.clearFlags = CameraClearFlags.SolidColor;
            _mirrorCamera.backgroundColor = new Color(0, 0, 0, 0);
            _mirrorCamera.cullingMask = 1 << VMCSpoutStatic.AvatarLayer;
            _mirrorCamera.targetTexture = mirrorTexture;

            mirrorMaterial = new Material(VMCSpoutStatic.shaders["MirrorReflection"]);
            mirrorMaterial.SetTexture(_texId, mirrorTexture);

            _rawImage.material = mirrorMaterial;
        }

        private void Update()
        {
            if (target != null && renderCamera != null)
            {
                if(_mirrorCamera != null && mirrorTexture != null)
                {
                    SetReflectionCamera();
                    GL.invertCulling = true;
                    _mirrorCamera.Render();
                    GL.invertCulling = false;
                    mirrorMaterial.SetTexture(_texId, mirrorTexture);
                }
            }
        }

        private void LateUpdate()
        {
            if (target != null && renderCamera != null)
            {
                _center = Vector3.zero;

                var canvasRect = _rawImage.canvas.GetComponent<RectTransform>();
                var rect = _rawImage.gameObject.GetComponent<RectTransform>();

                var screenPos = RectTransformUtility.WorldToScreenPoint(renderCamera, _center);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, renderCamera, out var localPoint);
                rect.localPosition = localPoint;

                rect.localRotation = Quaternion.Inverse(renderCamera.transform.rotation) * Quaternion.Euler(_mirrorEular);

                var leftWorld = new Vector3(-_mirrorRectWidth / 2, 0, 0);
                var rightWorld = new Vector3(_mirrorRectWidth / 2, 0, 0);
                var backWorld = new Vector3(0, 0, -_mirrorRectHeight / 2);
                var forwardWorld = new Vector3(0, 0, _mirrorRectHeight / 2);

                var imageSizePointLeft = RectTransformUtility.WorldToScreenPoint(renderCamera, leftWorld);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, imageSizePointLeft, renderCamera, out var localLeft);

                var imageSizePointRight = RectTransformUtility.WorldToScreenPoint(renderCamera, rightWorld);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, imageSizePointRight, renderCamera, out var localRight);

                var imageSizePointBack = RectTransformUtility.WorldToScreenPoint(renderCamera, backWorld);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, imageSizePointBack, renderCamera, out var localBack);

                var imageSizePointForward = RectTransformUtility.WorldToScreenPoint(renderCamera, forwardWorld);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, imageSizePointForward, renderCamera, out var localForward);

                rect.sizeDelta = new Vector2(Vector2.Distance(localLeft, localRight), Vector2.Distance(localBack, localForward));
            }
        }

        private void SetReflectionCamera()
        {
            _targetFloorObject.transform.position = new Vector3(target.position.x, 0, target.position.z);

            Vector3 planeNormal = _targetFloorObject.transform.up;
            Vector3 planePos = _targetFloorObject.transform.position;
            float d = -Vector3.Dot(planeNormal, planePos);
            Vector4 plane = new Vector4(planeNormal.x, planeNormal.y, planeNormal.z, d);

            Matrix4x4 refMatrix = CalcReflectionMatrix(plane);
            _mirrorCamera.worldToCameraMatrix = renderCamera.worldToCameraMatrix * refMatrix;

            Vector3 cPos = _mirrorCamera.worldToCameraMatrix.MultiplyPoint(planePos);
            Vector3 cNormal = _mirrorCamera.worldToCameraMatrix.MultiplyVector(planeNormal).normalized;
            Vector4 clipPlane = new Vector4(cNormal.x, cNormal.y, cNormal.z, -Vector3.Dot(cPos, cNormal));

            _mirrorCamera.projectionMatrix = renderCamera.CalculateObliqueMatrix(clipPlane);
        }

        private Matrix4x4 CalcReflectionMatrix(Vector4 n)
        {
            var refMatrix = new Matrix4x4
            {
                m00 = 1f - 2f * n.x * n.x,
                m01 = -2f * n.x * n.y,
                m02 = -2f * n.x * n.z,
                m03 = -2f * n.x * n.w,
                m10 = -2f * n.x * n.y,
                m11 = 1f - 2f * n.y * n.y,
                m12 = -2f * n.y * n.z,
                m13 = -2f * n.y * n.w,
                m20 = -2f * n.x * n.z,
                m21 = -2f * n.y * n.z,
                m22 = 1f - 2f * n.z * n.z,
                m23 = -2f * n.z * n.w,
                m30 = 0F,
                m31 = 0F,
                m32 = 0F,
                m33 = 1F
            };

            return refMatrix;
        }
    }
}
