using UnityEngine;

namespace SindishTech.WaterTankSimulator
{
    /// <summary>
    /// Controls the water shader material properties based on simulation state.
    /// Attach this to the Tank mesh.
    /// 
    /// BEHAVIOR:
    /// - In Editor Mode (not playing): Shows metallic tank material
    /// - In Play Mode with Simulation Running: Shows water filling shader
    /// - Water level and temperature are driven by the simulation
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("SindishTech/Water Level Controller")]
    public class WaterLevelController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the WaterTankSimulation component")]
        public WaterTankSimulation simulation;

        [Tooltip("The renderer with the water material")]
        public Renderer tankRenderer;

        [Header("Tank Dimensions")]
        [Tooltip("Height of the tank in world units")]
        public float tankHeight = 2f;

        [Tooltip("Y position of the tank bottom in world space")]
        public float tankBottomY = -1f;

        [Header("Material Settings")]
        [Tooltip("Material instance index (if multiple materials on renderer)")]
        public int materialIndex = 0;

        [Header("Mode Control")]
        [Tooltip("When true, simulation mode runs even in Editor (for testing)")]
        public bool forceSimulationModeInEditor = false;

        private Material tankMaterial;
        private static readonly int WaterLevelID = Shader.PropertyToID("_WaterLevel");
        private static readonly int TankHeightID = Shader.PropertyToID("_TankHeight");
        private static readonly int TankBottomYID = Shader.PropertyToID("_TankBottomY");
        private static readonly int TemperatureID = Shader.PropertyToID("_Temperature");
        private static readonly int SimulationModeID = Shader.PropertyToID("_SimulationMode");

        private void OnEnable()
        {
            if (tankRenderer == null)
                tankRenderer = GetComponent<Renderer>();

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
            if (tankMaterial == null)
            {
                UpdateMaterialReference();
                if (tankMaterial == null) return;
            }

            // Determine simulation mode
            // Mode 0 = Metallic (editor mode, no water)
            // Mode 1 = Water simulation active
            bool simulationActive = false;
            
            if (Application.isPlaying)
            {
                // In Play mode: show water when simulation is running
                simulationActive = simulation != null && simulation.IsSimulationRunning;
            }
            else
            {
                // In Editor mode: show metallic unless forced
                simulationActive = forceSimulationModeInEditor && simulation != null && simulation.IsSimulationRunning;
            }

            tankMaterial.SetFloat(SimulationModeID, simulationActive ? 1f : 0f);

            if (simulation == null) return;

            // Update water level
            float level = simulation.WaterLevelPercentage / 100f;
            tankMaterial.SetFloat(WaterLevelID, level);
            tankMaterial.SetFloat(TankHeightID, tankHeight);
            tankMaterial.SetFloat(TankBottomYID, tankBottomY);
            tankMaterial.SetFloat(TemperatureID, simulation.CurrentTemperature);
        }

        private void UpdateMaterialReference()
        {
            if (tankRenderer == null) return;

            // Always use material instance in play mode, shared in editor
            if (Application.isPlaying)
            {
                if (tankRenderer.materials.Length > materialIndex)
                    tankMaterial = tankRenderer.materials[materialIndex];
            }
            else
            {
                if (tankRenderer.sharedMaterials.Length > materialIndex)
                    tankMaterial = tankRenderer.sharedMaterials[materialIndex];
            }
        }

        private void OnWaterLevelChanged(float level)
        {
            if (tankMaterial == null) return;
            float normalizedLevel = level / simulation.tankCapacity;
            tankMaterial.SetFloat(WaterLevelID, normalizedLevel);
        }

        private void OnTemperatureChanged(float temperature)
        {
            if (tankMaterial == null) return;
            tankMaterial.SetFloat(TemperatureID, temperature);
        }

        /// <summary>
        /// Auto-detect tank dimensions from the renderer bounds.
        /// </summary>
        [ContextMenu("Auto Detect Tank Dimensions")]
        public void AutoDetectDimensions()
        {
            if (tankRenderer == null) return;

            Bounds bounds = tankRenderer.bounds;
            tankHeight = bounds.size.y;
            tankBottomY = bounds.min.y;
            
            Debug.Log($"Tank dimensions detected: Height = {tankHeight}, Bottom Y = {tankBottomY}");
        }

        /// <summary>
        /// Force refresh material reference.
        /// </summary>
        [ContextMenu("Refresh Material")]
        public void RefreshMaterial()
        {
            tankMaterial = null;
            UpdateMaterialReference();
        }
    }
}
