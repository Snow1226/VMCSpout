using UnityEngine;
using System.Collections.Generic;

namespace VMCSpout
{
    public class MirrorPlane : MonoBehaviour
    {
        public LayerMask reflectLayers = 1 <<VMCSpoutStatic.AvatarLayer; // 反射に映すレイヤー
        public int textureSize = 512;        // 反射テクスチャの解像度
        public float clipPlaneOffset = 0.07f;

        private Dictionary<Camera, RenderTexture> _reflectionTextures = new Dictionary<Camera, RenderTexture>();
        private Camera _reflectionCamera;
        private static bool _insideRendering = false;
        public float reflectivity = 0.5f; //反射強度（0.0～1.0）

        // カメラがこのオブジェクトを描画する直前に呼ばれる（カメラごとに実行される）
        public void OnWillRenderObject()
        {
            var rend = GetComponent<Renderer>();
            if (!enabled || !rend || !rend.sharedMaterial || !rend.enabled)
                return;

            Camera cam = Camera.current;
            if (!cam || _insideRendering)
                return;

            RemoveInvalidReflectionTextures();

            _insideRendering = true;
            try
            {
                RenderTexture reflectionTexture = GetReflectionTexture(cam);
                CreateReflectionCamera(cam);

                Vector3 pos = transform.position;
                Vector3 normal = transform.up;

                UpdateCameraModes(cam, _reflectionCamera);

                float d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
                Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

                Matrix4x4 reflection = Matrix4x4.zero;
                CalculateReflectionMatrix(ref reflection, reflectionPlane);

                _reflectionCamera.worldToCameraMatrix = cam.worldToCameraMatrix * reflection;

                Vector4 clipPlane = CameraSpacePlane(_reflectionCamera, pos, normal, 1.0f);
                _reflectionCamera.projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);

                _reflectionCamera.cullingMask = 1 << VMCSpoutStatic.AvatarLayer; // 反射対象のレイヤーのみ描画
                _reflectionCamera.targetTexture = reflectionTexture;

                // 鏡像反転によりカリング（表裏）が逆になるため一時的に反転
                GL.invertCulling = true;
                _reflectionCamera.Render();
                GL.invertCulling = false;

                // マテリアルに現在のカメラ用の反射テクスチャをセット
                rend.sharedMaterial.SetFloat("_Reflectivity", reflectivity);
                rend.sharedMaterial.SetTexture("_ReflectionTex", reflectionTexture);
            }
            finally
            {
                GL.invertCulling = false;
                _insideRendering = false;
            }
        }

        private RenderTexture GetReflectionTexture(Camera cam)
        {
            if (!_reflectionTextures.TryGetValue(cam, out RenderTexture rt) || rt == null || rt.width != textureSize || rt.height != textureSize)
            {
                if (rt != null)
                {
                    if (RenderTexture.active == rt)
                        RenderTexture.active = null;

                    rt.Release();
                    SafeDestroy(rt);
                }

                rt = new RenderTexture(textureSize, textureSize, 16);
                rt.name = "__MirrorReflection_" + cam.GetInstanceID();
                rt.hideFlags = HideFlags.DontSave;
                _reflectionTextures[cam] = rt;
            }
            return rt;
        }

        private void RemoveInvalidReflectionTextures()
        {
            if (_reflectionTextures.Count == 0)
                return;

            var removeTargets = new List<Camera>();
            foreach (var pair in _reflectionTextures)
            {
                if (pair.Key == null || pair.Value == null)
                {
                    if (pair.Value != null)
                    {
                        if (RenderTexture.active == pair.Value)
                            RenderTexture.active = null;

                        pair.Value.Release();
                        SafeDestroy(pair.Value);
                    }
                    removeTargets.Add(pair.Key);
                }
            }

            foreach (var cam in removeTargets)
                _reflectionTextures.Remove(cam);
        }

        private void CreateReflectionCamera(Camera src)
        {
            if (_reflectionCamera == null)
            {
                GameObject go = new GameObject("Mirror Reflection Camera", typeof(Camera));
                _reflectionCamera = go.GetComponent<Camera>();
                _reflectionCamera.enabled = false;
                go.hideFlags = HideFlags.HideAndDontSave;
            }
        }

        private void UpdateCameraModes(Camera src, Camera dest)
        {
            if (dest == null) return;
            // 反射対象以外は透過させるため、反射カメラ背景は常に透明でクリア
            dest.clearFlags = CameraClearFlags.SolidColor;
            dest.backgroundColor = new Color(0f, 0f, 0f, 0f);
            dest.farClipPlane = src.farClipPlane;
            dest.nearClipPlane = src.nearClipPlane;
            dest.orthographic = src.orthographic;
            dest.fieldOfView = src.fieldOfView;
            dest.aspect = src.aspect;
            dest.orthographicSize = src.orthographicSize;
        }

        private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            Vector3 offsetPos = pos + normal * clipPlaneOffset;
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cpos = m.MultiplyPoint(offsetPos);
            Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
        }

        private static void CalculateReflectionMatrix(ref Matrix4x4 reflectionMat, Vector4 plane)
        {
            reflectionMat.m00 = (1F - 2F * plane[0] * plane[0]);
            reflectionMat.m01 = (-2F * plane[0] * plane[1]);
            reflectionMat.m02 = (-2F * plane[0] * plane[2]);
            reflectionMat.m03 = (-2F * plane[3] * plane[0]);

            reflectionMat.m10 = (-2F * plane[1] * plane[0]);
            reflectionMat.m11 = (1F - 2F * plane[1] * plane[1]);
            reflectionMat.m12 = (-2F * plane[1] * plane[2]);
            reflectionMat.m13 = (-2F * plane[3] * plane[1]);

            reflectionMat.m20 = (-2F * plane[2] * plane[0]);
            reflectionMat.m21 = (-2F * plane[2] * plane[1]);
            reflectionMat.m22 = (1F - 2F * plane[2] * plane[2]);
            reflectionMat.m23 = (-2F * plane[3] * plane[2]);

            reflectionMat.m30 = 0F;
            reflectionMat.m31 = 0F;
            reflectionMat.m32 = 0F;
            reflectionMat.m33 = 1F;
        }

        private void ReleaseResources()
        {
            foreach (var rt in _reflectionTextures.Values)
            {
                if (rt != null)
                {
                    if (RenderTexture.active == rt)
                        RenderTexture.active = null;

                    rt.Release();
                    SafeDestroy(rt);
                }
            }
            _reflectionTextures.Clear();

            if (_reflectionCamera != null)
            {
                SafeDestroy(_reflectionCamera.gameObject);
                _reflectionCamera = null;
            }
        }

        private static void SafeDestroy(Object obj)
        {
            if (obj == null)
                return;

            if (Application.isPlaying)
                Destroy(obj);
            else
                DestroyImmediate(obj);
        }

        private void OnDisable()
        {
            ReleaseResources();
        }

        private void OnDestroy()
        {
            ReleaseResources();
        }
    }
}
