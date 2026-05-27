using UnityEngine;

namespace SindishTech.WaterTankSimulator
{
    /// <summary>
    /// Controls the water shader material properties based on simulation state.
    /// Attach this to the Tank mesh or a child water plane object.
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("SindishTech/Water Level Controller")]
    public class WaterLevelController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the WaterTankSimulation component")]
        public WaterTankSimulation simulation;

        [Tooltip("The renderer with the water material")]
        public Renderer waterRenderer;

        [Header("Tank Dimensions")]
        [Tooltip("Height of the tank in world units")]
        public float tankHeight = 2f;

        [Tooltip("Y position of the tank bottom in world space")]
        public float tankBottomY = -1f;

        [Header("Material Settings")]
        [Tooltip("Material instance index (if multiple materials on renderer)")]
        public int materialIndex = 0;

        private Material waterMaterial;
        private static readonly int WaterLevelID = Shader.PropertyToID("_WaterLevel");
        private static readonly int TankHeightID = Shader.PropertyToID("_TankHeight");
        private static readonly int TankBottomYID = Shader.PropertyToID("_TankBottomY");
        private static readonly int TemperatureID = Shader.PropertyToID("_Temperature");

        private void OnEnable()
        {
            if (waterRenderer == null)
                waterRenderer = GetComponent<Renderer>();

            UpdateMaterialReference();

            if (simulation != null)
            {
                simulation.OnWaterLevelChanged += OnWaterLevelChanged;
                simulation.OnTemperatureChanged += OnTemperatureChanged;
            }
        }

        private void OnDisable()
        {
            if (simulation != null)
            {
                simulation.OnWaterLevelChanged -= OnWaterLevelChanged;
                simulation.OnTemperatureChanged -= OnTemperatureChanged;
            }
        }

        private void Update()
        {
            if (simulation == null || waterMaterial == null) return;

            // Continuous update for edit mode
            float level = simulation.WaterLevelPercentage / 100f;
            waterMaterial.SetFloat(WaterLevelID, level);
            waterMaterial.SetFloat(TankHeightID, tankHeight);
            waterMaterial.SetFloat(TankBottomYID, tankBottomY);
            waterMaterial.SetFloat(TemperatureID, simulation.CurrentTemperature);
        }

        private void UpdateMaterialReference()
        {
            if (waterRenderer == null) return;

            if (Application.isPlaying)
            {
                if (waterRenderer.materials.Length > materialIndex)
                    waterMaterial = waterRenderer.materials[materialIndex];
            }
            else
            {
                if (waterRenderer.sharedMaterials.Length > materialIndex)
                    waterMaterial = waterRenderer.sharedMaterials[materialIndex];
            }
        }

        private void OnWaterLevelChanged(float level)
        {
            if (waterMaterial == null) return;
            float normalizedLevel = level / simulation.tankCapacity;
            waterMaterial.SetFloat(WaterLevelID, normalizedLevel);
        }

        private void OnTemperatureChanged(float temperature)
        {
            if (waterMaterial == null) return;
            waterMaterial.SetFloat(TemperatureID, temperature);
        }

        /// <summary>
        /// Auto-detect tank dimensions from the renderer bounds.
        /// </summary>
        [ContextMenu("Auto Detect Tank Dimensions")]
        public void AutoDetectDimensions()
        {
            if (waterRenderer == null) return;

            Bounds bounds = waterRenderer.bounds;
            tankHeight = bounds.size.y;
            tankBottomY = bounds.min.y;
        }
    }
}
