using UnityEngine;

namespace SindishTech.WaterTankSimulator
{
    /// <summary>
    /// Controls the water tank visualization with 2 materials:
    /// - Material 0 (Metal): Shown in Editor mode
    /// - Material 1 (Water): Shown in Play mode, fills based on water level
    /// 
    /// The water material uses a shader that clips pixels above the water level.
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("SindishTech/Water Level Controller")]
    public class WaterLevelController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the WaterTankSimulation component")]
        public WaterTankSimulation simulation;

        [Tooltip("The renderer with the tank materials")]
        public Renderer tankRenderer;

        [Header("Material Indices")]
        [Tooltip("Index of the Metal material (shown in Editor)")]
        public int metalMaterialIndex = 0;

        [Tooltip("Index of the Water material (shown in Play mode)")]
        public int waterMaterialIndex = 1;

        [Header("Tank Dimensions (World Space)")]
        [Tooltip("Y position of the tank bottom in world space")]
        public float tankBottomY = 0f;

        [Tooltip("Y position of the tank top in world space")]
        public float tankTopY = 2f;

        [Header("Debug")]
        [SerializeField] private float currentWaterWorldY = 0f;
        [SerializeField] private bool isPlayMode = false;

        // Shader property IDs
        private static readonly int WaterLevelYID = Shader.PropertyToID("_WaterLevelY");
        private static readonly int TemperatureID = Shader.PropertyToID("_Temperature");
        private static readonly int TankBottomYID = Shader.PropertyToID("_TankBottomY");
        private static readonly int TankTopYID = Shader.PropertyToID("_TankTopY");

        private Material waterMaterial;
        private Material[] originalMaterials;
        private bool materialsInitialized = false;

        private void OnEnable()
        {
            if (tankRenderer == null)
                tankRenderer = GetComponent<Renderer>();

            if (tankRenderer != null)
            {
                originalMaterials = tankRenderer.sharedMaterials;
            }

            InitializeMaterials();
        }

        private void OnDisable()
        {
            // Restore original materials when disabled
            RestoreEditorMaterials();
        }

        private void InitializeMaterials()
        {
            if (tankRenderer == null || tankRenderer.sharedMaterials.Length <= waterMaterialIndex)
                return;

            if (Application.isPlaying)
            {
                // Get instance of water material for runtime modification
                Material[] mats = tankRenderer.materials;
                if (mats.Length > waterMaterialIndex)
                {
                    waterMaterial = mats[waterMaterialIndex];
                }
            }
            else
            {
                waterMaterial = tankRenderer.sharedMaterials[waterMaterialIndex];
            }

            materialsInitialized = true;
        }

        private void Update()
        {
            isPlayMode = Application.isPlaying;

            if (!materialsInitialized)
            {
                InitializeMaterials();
            }

            if (simulation == null) return;

            UpdateWaterLevel();
        }

        private void UpdateWaterLevel()
        {
            if (waterMaterial == null) return;

            // Calculate water Y position in world space based on fill percentage
            float fillPercentage = simulation.WaterLevelPercentage / 100f;
            float tankHeight = tankTopY - tankBottomY;
            currentWaterWorldY = tankBottomY + (fillPercentage * tankHeight);

            // Update shader properties
            waterMaterial.SetFloat(WaterLevelYID, currentWaterWorldY);
            waterMaterial.SetFloat(TemperatureID, simulation.CurrentTemperature);
            waterMaterial.SetFloat(TankBottomYID, tankBottomY);
            waterMaterial.SetFloat(TankTopYID, tankTopY);
        }

        private void RestoreEditorMaterials()
        {
            if (!Application.isPlaying && tankRenderer != null && originalMaterials != null)
            {
                tankRenderer.sharedMaterials = originalMaterials;
            }
        }

        /// <summary>
        /// Auto-detect tank dimensions from the renderer bounds.
        /// </summary>
        [ContextMenu("Auto Detect Tank Dimensions")]
        public void AutoDetectDimensions()
        {
            if (tankRenderer == null)
            {
                Debug.LogWarning("No renderer assigned!");
                return;
            }

            Bounds bounds = tankRenderer.bounds;
            tankBottomY = bounds.min.y;
            tankTopY = bounds.max.y;

            Debug.Log($"Tank dimensions detected: Bottom Y = {tankBottomY}, Top Y = {tankTopY}, Height = {tankTopY - tankBottomY}");
        }

        /// <summary>
        /// Test water fill at specific percentage.
        /// </summary>
        [ContextMenu("Test Fill 50%")]
        public void TestFill50()
        {
            if (simulation != null)
            {
                simulation.SetWaterLevel(simulation.tankCapacity * 0.5f);
            }
        }

        [ContextMenu("Test Fill 100%")]
        public void TestFill100()
        {
            if (simulation != null)
            {
                simulation.SetWaterLevel(simulation.tankCapacity);
            }
        }

        [ContextMenu("Test Fill 0%")]
        public void TestFill0()
        {
            if (simulation != null)
            {
                simulation.SetWaterLevel(0);
            }
        }
    }
}
