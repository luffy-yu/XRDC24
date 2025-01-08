using UnityEngine;

namespace Fragilem17.MirrorsAndPortals
{
    public class PortalSurface : MonoBehaviour
    {
        [Tooltip("The source material, disable and re-enable this component if you make changes to the material")]
        public Material Material;

        [Tooltip("When the camera is further from this distance, the surface stops updating its texture.")]
        [Min(0)]
        public float maxRenderingDistance = 5f;

        [Tooltip("Defines whether a certain amount of color should be blended in with the reflection depending on distance.")]
        public bool useColorBlending = true;
        public Color BlendColor = Color.black;

        [Tooltip("Defines the curve for color blending over distance.")]
        public AnimationCurve colorBlendingCurve = AnimationCurve.Linear(0, 0, 1, 1);

        private Material _material;
        private bool _wasTooFar = false;
        private float _cachedFadeColorBlend = -1f; // 缓存的颜色混合值
        private float _cachedAlbedoAlpha = -1f;   // 缓存的 Alpha 值
        private float _cachedRefraction = -1f;   // 缓存的折射值

        private void OnEnable()
        {
            if (Material)
            {
                _material = new Material(Material);
                _material.name += " (Instance)";
                GetComponent<MeshRenderer>().material = _material;
            }
        }

        private void OnDisable()
        {
            if (_material)
            {
                DestroyImmediate(_material);
            }
        }

        private void Update()
        {
            Camera mainCamera = Camera.main;
            if (!mainCamera) return;

            float distance = Vector3.Distance(mainCamera.transform.position, transform.position);
            bool isTooFar = distance > maxRenderingDistance;

            if (isTooFar != _wasTooFar)
            {
                _wasTooFar = isTooFar;
                UpdateMaterialProperties(distance);
            }

            // 每帧仅更新平面计算，用于其他逻辑
            Plane plane = new Plane(-transform.forward, transform.position);
        }

        private void UpdateMaterialProperties(float distance)
        {
            if (!_material) return;

            float distPercent = Mathf.Clamp01(distance / maxRenderingDistance);

            // 更新颜色混合
            if (useColorBlending && _material.HasProperty("_FadeColorBlend"))
            {
                float newFadeColorBlend = colorBlendingCurve.Evaluate(distPercent);
                if (Mathf.Abs(newFadeColorBlend - _cachedFadeColorBlend) > 0.01f)
                {
                    _material.SetFloat("_FadeColorBlend", newFadeColorBlend);
                    _cachedFadeColorBlend = newFadeColorBlend;
                }
            }

            // 更新 Alpha 淡入淡出
            if (_material.HasProperty("_AlbedoAlpha"))
            {
                float newAlbedoAlpha = colorBlendingCurve.Evaluate(distPercent);
                if (Mathf.Abs(newAlbedoAlpha - _cachedAlbedoAlpha) > 0.01f)
                {
                    _material.SetFloat("_AlbedoAlpha", newAlbedoAlpha);
                    _cachedAlbedoAlpha = newAlbedoAlpha;
                }
            }

            // 更新折射
            if (_material.HasProperty("_refraction"))
            {
                float newRefraction = colorBlendingCurve.Evaluate(distPercent);
                if (Mathf.Abs(newRefraction - _cachedRefraction) > 0.01f)
                {
                    _material.SetFloat("_refraction", newRefraction);
                    _cachedRefraction = newRefraction;
                }
            }
        }
    }
}
