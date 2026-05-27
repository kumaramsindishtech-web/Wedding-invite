using UnityEngine;

namespace SindishTech.WaterTankSimulator
{
    /// <summary>
    /// Visual indicator for water flow in pipes.
    /// Uses particle system or material UV scrolling to show water movement.
    /// Attach to Inlet or Outlet pipe objects.
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("SindishTech/Pipe Flow Visual")]
    public class PipeFlowVisual : MonoBehaviour
    {
        public enum PipeType
        {
            Inlet,
            Outlet
        }

        public enum FlowVisualMode
        {
            UVScroll,
            ParticleSystem,
            Both
        }

        [Header("References")]
        [Tooltip("Reference to the WaterTankSimulation component")]
        public WaterTankSimulation simulation;

        [Header("Pipe Configuration")]
        [Tooltip("Whether this is an inlet or outlet pipe")]
        public PipeType pipeType = PipeType.Inlet;

        [Tooltip("Visual mode for flow indication")]
        public FlowVisualMode visualMode = FlowVisualMode.UVScroll;

        [Header("UV Scroll Settings")]
        [Tooltip("Renderer for UV scroll effect")]
        public Renderer pipeRenderer;

        [Tooltip("Material index on the renderer")]
        public int materialIndex = 0;

        [Tooltip("UV scroll direction")]
        public Vector2 scrollDirection = new Vector2(1f, 0f);

        [Tooltip("UV scroll speed multiplier")]
        [Range(0.1f, 10f)]
        public float scrollSpeed = 2f;

        [Tooltip("Flow color tint when active")]
        public Color flowColor = new Color(0.3f, 0.6f, 0.9f, 1f);

        [Tooltip("Idle color when no flow")]
        public Color idleColor = new Color(0.5f, 0.5f, 0.5f, 1f);

        [Tooltip("Enable color change based on flow state")]
        public bool enableFlowColor = true;

        [Header("Particle System Settings")]
        [Tooltip("Particle system for flow visualization")]
        public ParticleSystem flowParticles;

        [Tooltip("Particle emission rate when flowing")]
        [Range(1f, 500f)]
        public float particleEmissionRate = 50f;

        [Header("Temperature Visual")]
        [Tooltip("Tint particles/material based on water temperature")]
        public bool showTemperatureColor = true;

        [Tooltip("Cold water color")]
        public Color coldColor = new Color(0.2f, 0.5f, 0.9f, 1f);

        [Tooltip("Hot water color")]
        public Color hotColor = new Color(0.9f, 0.3f, 0.1f, 1f);

        [Tooltip("Temperature at which color is fully hot")]
        public float hotTemperature = 200f;

        // Internal
        private Material pipeMaterial;
        private Vector2 currentOffset = Vector2.zero;
        private bool isFlowing = false;
        private ParticleSystem.EmissionModule emissionModule;
        private static readonly int MainTexID = Shader.PropertyToID("_MainTex_ST");
        private static readonly int OffsetID = Shader.PropertyToID("_MainTex");
        private static readonly int ColorID = Shader.PropertyToID("_Color");
        private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

        private void OnEnable()
        {
            SetupMaterial();
            SetupParticles();
        }

        private void SetupMaterial()
        {
            if (pipeRenderer == null)
                pipeRenderer = GetComponent<Renderer>();

            if (pipeRenderer != null)
            {
                if (Application.isPlaying)
                {
                    if (pipeRenderer.materials.Length > materialIndex)
                        pipeMaterial = pipeRenderer.materials[materialIndex];
                }
                else
                {
                    if (pipeRenderer.sharedMaterials.Length > materialIndex)
                        pipeMaterial = pipeRenderer.sharedMaterials[materialIndex];
                }
            }
        }

        private void SetupParticles()
        {
            if (flowParticles == null)
                flowParticles = GetComponentInChildren<ParticleSystem>();

            if (flowParticles != null)
            {
                emissionModule = flowParticles.emission;
            }
        }

        private void Update()
        {
            if (simulation == null) return;

            // Determine if water should be flowing in this pipe
            bool shouldFlow = pipeType == PipeType.Inlet
                ? simulation.inletValveOpen
                : (simulation.outletValveOpen && simulation.CurrentWaterLevel > 0f);

            isFlowing = shouldFlow;

            float deltaTime = GetDeltaTime();

            // UV Scroll
            if (visualMode == FlowVisualMode.UVScroll || visualMode == FlowVisualMode.Both)
            {
                UpdateUVScroll(deltaTime);
            }

            // Particle System
            if (visualMode == FlowVisualMode.ParticleSystem || visualMode == FlowVisualMode.Both)
            {
                UpdateParticles();
            }

            // Color feedback
            UpdateFlowColor(deltaTime);
        }

        private void UpdateUVScroll(float deltaTime)
        {
            if (pipeMaterial == null) return;

            if (isFlowing)
            {
                // Scroll UV to simulate flow
                float flowDirection = pipeType == PipeType.Inlet ? 1f : -1f;
                currentOffset += scrollDirection * scrollSpeed * flowDirection * deltaTime;

                // Wrap offset to prevent floating point issues
                currentOffset.x = currentOffset.x % 1f;
                currentOffset.y = currentOffset.y % 1f;

                pipeMaterial.SetTextureOffset("_MainTex", currentOffset);
            }
        }

        private void UpdateParticles()
        {
            if (flowParticles == null) return;

            if (isFlowing)
            {
                if (!flowParticles.isPlaying)
                    flowParticles.Play();

                emissionModule.rateOverTime = particleEmissionRate;

                // Temperature-based particle color
                if (showTemperatureColor)
                {
                    var main = flowParticles.main;
                    float tempNormalized = Mathf.Clamp01(simulation.CurrentTemperature / hotTemperature);
                    Color particleColor = Color.Lerp(coldColor, hotColor, tempNormalized);
                    main.startColor = particleColor;
                }
            }
            else
            {
                emissionModule.rateOverTime = 0f;
                if (flowParticles.isPlaying && flowParticles.particleCount == 0)
                    flowParticles.Stop();
            }
        }

        private void UpdateFlowColor(float deltaTime)
        {
            if (!enableFlowColor || pipeMaterial == null) return;

            Color targetColor;

            if (isFlowing)
            {
                if (showTemperatureColor)
                {
                    float tempNormalized = Mathf.Clamp01(simulation.CurrentTemperature / hotTemperature);
                    targetColor = Color.Lerp(coldColor, hotColor, tempNormalized);
                }
                else
                {
                    targetColor = flowColor;
                }
            }
            else
            {
                targetColor = idleColor;
            }

            // Smooth color transition
            Color currentColor;
            if (pipeMaterial.HasProperty(BaseColorID))
            {
                currentColor = pipeMaterial.GetColor(BaseColorID);
                Color newColor = Color.Lerp(currentColor, targetColor, 5f * deltaTime);
                pipeMaterial.SetColor(BaseColorID, newColor);
            }
            else if (pipeMaterial.HasProperty(ColorID))
            {
                currentColor = pipeMaterial.GetColor(ColorID);
                Color newColor = Color.Lerp(currentColor, targetColor, 5f * deltaTime);
                pipeMaterial.SetColor(ColorID, newColor);
            }

            // Emission glow when flowing hot
            if (pipeMaterial.HasProperty(EmissionColorID) && isFlowing && showTemperatureColor)
            {
                float tempNormalized = Mathf.Clamp01(simulation.CurrentTemperature / hotTemperature);
                Color emissionColor = Color.Lerp(Color.black, hotColor * 0.3f, tempNormalized);
                pipeMaterial.SetColor(EmissionColorID, emissionColor);
            }
        }

        private float GetDeltaTime()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return 0.016f;
            }
#endif
            return Time.deltaTime;
        }

        /// <summary>
        /// Check if water is currently flowing through this pipe.
        /// </summary>
        public bool IsFlowing => isFlowing;

        [ContextMenu("Test Flow On")]
        private void TestFlowOn()
        {
            isFlowing = true;
        }

        [ContextMenu("Test Flow Off")]
        private void TestFlowOff()
        {
            isFlowing = false;
        }
    }
}
