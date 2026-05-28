using UnityEngine;

namespace SindishTech.WaterTankSimulator
{
    /// <summary>
    /// Controls the water tank visualization with 2 materials:
    /// - Material 0 (Metal): Always visible (tank shell)
    /// - Material 1 (Water): Clips based on water level Y position
    /// 
    /// The water material uses a shader that clips pixels above the water level.
    /// When water level is 0%, all water pixels are clipped (invisible).
    /// As water fills, more pixels become visible from bottom to top.
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("SindishTech/Water Level Controller")]
    public class WaterLevelController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the WaterTankSimulation component")]
        public WaterTankSimulation simulation;

        [Tooltip("The renderer with the tank materials (usually MeshRenderer on Tank)")]
        public Renderer tankRenderer;

        [Header("Material Index")]
        [Tooltip("Index of the Water material in the renderer's material array")]
        public int waterMaterialIndex = 1;

        [Header("Tank Dimensions (World Space Y)")]
        [Tooltip("Y position of the tank BOTTOM in world space - water starts filling from here")]
        public float tankBottomY = 0f;

        [Tooltip("Y position of the tank TOP in world space - water fills up to here at 100%")]
        public float tankTopY = 2f;

        [Header("Debug Info (Read Only)")]
        [SerializeField] private float currentWaterLevelY = 0f;
        [SerializeField] private float currentFillPercentage = 0f;
        [SerializeField] private bool materialFound = false;

        // Shader property IDs (must match shader exactly)
        private static readonly int WaterLevelYID = Shader.PropertyToID("_WaterLevelY");
        private static readonly int TemperatureID = Shader.PropertyToID("_Temperature");
        private static readonly int TankBottomYID = Shader.PropertyToID("_TankBottomY");
        private static readonly int TankTopYID = Shader.PropertyToID("_TankTopY");

        private Material waterMaterial;

        private void OnEnable()
        {
            if (tankRenderer == null)
                tankRenderer = GetComponent<Renderer>();

            GetWaterMaterial();
            
            // Initialize water level to bottom (0% fill)
            if (waterMaterial != null)
            {
                waterMaterial.SetFloat(WaterLevelYID, tankBottomY - 1f); // Start below tank
            }
        }

        private void GetWaterMaterial()
        {
            if (tankRenderer == null)
            {
                materialFound = false;
                return;
            }

            Material[] mats = Application.isPlaying ? tankRenderer.materials : tankRenderer.sharedMaterials;
            
            if (mats != null && mats.Length > waterMaterialIndex)
            {
                waterMaterial = mats[waterMaterialIndex];
                materialFound = waterMaterial != null;
                
                if (materialFound)
                {
                    Debug.Log($"[WaterLevelController] Water material found: {waterMaterial.name}, Shader: {waterMaterial.shader.name}");
                }
            }
            else
            {
                materialFound = false;
                Debug.LogWarning($"[WaterLevelController] Water material not found at index {waterMaterialIndex}. Material count: {mats?.Length ?? 0}");
            }
        }

        private void Update()
        {
            if (waterMaterial == null)
            {
                GetWaterMaterial();
                if (waterMaterial == null) return;
            }

            if (simulation == null)
            {
                Debug.LogWarning("[WaterLevelController] No simulation assigned!");
                return;
            }

            UpdateWaterShader();
        }

        private void UpdateWaterShader()
        {
            // Get fill percentage from simulation (0-100)
            currentFillPercentage = simulation.WaterLevelPercentage;
            
            // Convert to 0-1 range
            float fillNormalized = currentFillPercentage / 100f;
            
            // Calculate water surface Y in world space
            float tankHeight = tankTopY - tankBottomY;
            currentWaterLevelY = tankBottomY + (fillNormalized * tankHeight);

            // Send to shader
            waterMaterial.SetFloat(WaterLevelYID, currentWaterLevelY);
            waterMaterial.SetFloat(TemperatureID, simulation.CurrentTemperature);
            waterMaterial.SetFloat(TankBottomYID, tankBottomY);
            waterMaterial.SetFloat(TankTopYID, tankTopY);
        }

        /// <summary>
        /// Auto-detect tank dimensions from the renderer bounds.
        /// Call this after positioning your tank in the scene.
        /// </summary>
        [ContextMenu("Auto Detect Tank Dimensions")]
        public void AutoDetectDimensions()
        {
            if (tankRenderer == null)
            {
                tankRenderer = GetComponent<Renderer>();
                if (tankRenderer == null)
                {
                    Debug.LogError("[WaterLevelController] No Renderer found!");
                    return;
                }
            }

            Bounds bounds = tankRenderer.bounds;
            tankBottomY = bounds.min.y;
            tankTopY = bounds.max.y;

            Debug.Log($"[WaterLevelController] Tank dimensions auto-detected:\n" +
                      $"  Bottom Y: {tankBottomY}\n" +
                      $"  Top Y: {tankTopY}\n" +
                      $"  Height: {tankTopY - tankBottomY}");
            
            // Also update shader immediately
            if (waterMaterial != null)
            {
                waterMaterial.SetFloat(TankBottomYID, tankBottomY);
                waterMaterial.SetFloat(TankTopYID, tankTopY);
            }
        }

        /// <summary>
        /// Manually set water level for testing (bypasses simulation)
        /// </summary>
        public void SetWaterLevelDirect(float percentage)
        {
            if (waterMaterial == null) return;
            
            float fillNormalized = Mathf.Clamp01(percentage / 100f);
            float tankHeight = tankTopY - tankBottomY;
            float waterY = tankBottomY + (fillNormalized * tankHeight);
            
            waterMaterial.SetFloat(WaterLevelYID, waterY);
            currentWaterLevelY = waterY;
            currentFillPercentage = percentage;
        }

        [ContextMenu("Test: Fill 0%")]
        public void TestFill0() => SetWaterLevelDirect(0);

        [ContextMenu("Test: Fill 25%")]
        public void TestFill25() => SetWaterLevelDirect(25);

        [ContextMenu("Test: Fill 50%")]
        public void TestFill50() => SetWaterLevelDirect(50);

        [ContextMenu("Test: Fill 75%")]
        public void TestFill75() => SetWaterLevelDirect(75);

        [ContextMenu("Test: Fill 100%")]
        public void TestFill100() => SetWaterLevelDirect(100);
    }
}
