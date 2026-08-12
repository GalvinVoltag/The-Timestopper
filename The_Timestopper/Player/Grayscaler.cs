using System.Collections.Generic;
using UnityEngine;

namespace The_Timestopper.Player
{
    public class Grayscaler : MonoBehaviour
    {
        private static readonly int MyCustomDepth = Shader.PropertyToID("_MyCustomDepth");
        private GameObject GrayscaleCube;
        public static Grayscaler _instance;

        public static Grayscaler Instance
        {
            get
            {
                if (_instance) return _instance;
                if (GameObject.Find("Player") == null) return null;
                if (GameObject.Find("Player").transform.Find("Main Camera") == null) return null;
                _instance = GameObject.Find("Player").transform.Find("Main Camera").GetOrAddComponent<Grayscaler>();
                return _instance;
            }
        }
        public RenderTexture depth;
        public Camera depthCamera;
        public Camera mainCamera;
        public Material grayscaleMaterial;
        public float grayscaleBubbleExpansion = 0;
        public float intensityControl = 0;

        public static void SetGrayscale(bool activation)
        {
            if (!Instance) return;
            Instance.GrayscaleCube.SetActive(activation);
        }

        public static bool dirty = false;
        private static readonly int DoExpansion = Shader.PropertyToID("_DoExpansion");
        private static readonly int Intensity = Shader.PropertyToID("_Intensity");
        private static readonly int Smoothness = Shader.PropertyToID("_Smoothness");
        private static readonly int SmoothnessInvert = Shader.PropertyToID("_SmoothnessInvert");
        private static readonly int NoDepthDistance = Shader.PropertyToID("_NoDepthDistance");
        private static readonly int Progression = Shader.PropertyToID("_Progression");
        private static readonly int ColorSpace1 = Shader.PropertyToID("_ColorSpace");
        private static readonly int AllIntensity = Shader.PropertyToID("_AllIntensity");
        private static readonly int Distance = Shader.PropertyToID("_Distance");

        public static void UpdateShaderSettings()
        {
            dirty = true;
            if (!TimestopperProgress.HasArm || !Instance) return;
            if (!Instance.GrayscaleCube) Instance.CreateGrayscaleCube();
            Instance.GrayscaleCube?.SetActive(Timestopper.grayscale.value);
            Instance.enabled = Timestopper.grayscale.value;
            Instance.grayscaleMaterial.SetFloat(DoExpansion, Timestopper.bubbleEffect.value? 1 : 0);
            Instance.grayscaleMaterial.SetFloat(Intensity, Timestopper.grayscaleIntensity.value);
            Instance.grayscaleMaterial.SetFloat(Smoothness, Timestopper.bubbleSmoothness.value);
            Instance.grayscaleMaterial.SetFloat(SmoothnessInvert, Timestopper.colorInversionArea.value);
            Instance.grayscaleMaterial.SetFloat(NoDepthDistance, Timestopper.skyTransitionTreshold.value);
            Instance.grayscaleMaterial.SetFloat(Progression, Timestopper.bubbleProgression.value);
            Instance.grayscaleMaterial.SetVector(ColorSpace1, new Vector4(
                Timestopper.grayscaleColorSpace.value.r,
                Timestopper.grayscaleColorSpace.value.g,
                Timestopper.grayscaleColorSpace.value.b,
                Timestopper.grayscaleColorSpaceIntensity.value));
        }

        public void CreateGrayscaleCube()
        {
            GrayscaleCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(GrayscaleCube.GetComponent<BoxCollider>());
            GrayscaleCube.name = "Grayscale Cube";
            GrayscaleCube.transform.SetParent(transform);
            GrayscaleCube.transform.localRotation = Quaternion.Euler(0, 0, 0);
            if (Camera.main) GrayscaleCube.transform.localPosition = Vector3.forward * (Camera.main.nearClipPlane + 0.001f);
            GrayscaleCube.transform.localScale = new Vector3(50, 50, 0);
            GrayscaleCube.GetComponent<MeshRenderer>().SetMaterials(new List<Material>(){grayscaleMaterial});
            GrayscaleCube.SetActive(Timestopper.grayscale.value);
        }
        public void Awake()
        {
            if (Instance == null || Timestopper.grayscaleShader == null) return;
            mainCamera = GetComponent<Camera>();
            depth = new RenderTexture(mainCamera.pixelWidth, mainCamera.pixelHeight, 24, RenderTextureFormat.RFloat);
            depth.Create();
            
            GameObject depthCamObj =  new GameObject("Depth Camera");
            depthCamObj.transform.parent = transform;
            depthCamObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            depthCamera = depthCamObj.AddComponent<Camera>();

            depthCamera.CopyFrom(mainCamera);
            depthCamera.enabled = false;
            depthCamera.targetTexture = depth;
            depthCamera.depthTextureMode = DepthTextureMode.Depth;
            depthCamera.aspect = mainCamera.aspect;
            depthCamera.SetReplacementShader(Timestopper.depthShader, "RenderType");

            grayscaleMaterial = new Material(Timestopper.grayscaleShader);
            
            CreateGrayscaleCube();
        }

        public void LateUpdate()
        {
            if (dirty && Instance)
            {
                UpdateShaderSettings();
                dirty = false;
            }
            // if (!GrayscaleCube.activeInHierarchy) return;
            if (Timestopper.bubbleEffect.value) {
                depthCamera.fieldOfView = mainCamera.fieldOfView;
                depthCamera.aspect = mainCamera.aspect;
                if (depth.width != mainCamera.pixelWidth || depth.height != mainCamera.pixelHeight)
                {
                    depth.Release();
                    depth = new RenderTexture(mainCamera.pixelWidth, mainCamera.pixelHeight, 24,
                        RenderTextureFormat.RFloat);
                    depth.Create();
                    depthCamera.targetTexture = depth;
                }

                depthCamera?.Render();
                Shader.SetGlobalTexture(MyCustomDepth, depth);
            }
            // bubbleEffect;
            // overallEffectIntensity;
            // grayscaleIntensity;
            // bubbleSmoothness;
            // colorInversionArea;
            // skyTransitionTreshold;
            // bubbleDistance;
            // bubbleProgression;
            // grayscaleColorSpace;
            // grayscaleColorSpaceIntensity;
            if (Timestopper.TimeStop)
            {
                if (intensityControl < 1) intensityControl += Timestopper.playerDeltaTime * Timestopper.stopSpeed.value;
                if (intensityControl > 1) intensityControl = 1;
            }
            else
            {
                if (intensityControl > 0) intensityControl -= Timestopper.playerDeltaTime * Timestopper.stopSpeed.value;
                if (intensityControl < 0) intensityControl = 0;
            }
            if (grayscaleBubbleExpansion < 20) grayscaleBubbleExpansion += Timestopper.playerDeltaTime * Timestopper.bubbleDistance.value;
            grayscaleMaterial.SetFloat(AllIntensity, Timestopper.overallEffectIntensity.value * intensityControl);
            grayscaleMaterial.SetFloat(Distance, grayscaleBubbleExpansion);
        }
    }
}