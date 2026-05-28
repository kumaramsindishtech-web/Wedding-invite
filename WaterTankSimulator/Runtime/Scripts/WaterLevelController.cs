using UnityEngine;

namespace SindishTech.WaterTankSimulator
{
    /// <summary>
    /// Controls the water tank fill visualization.
    /// Uses a simple 0-1 fill amount that the shader uses with UV coordinates.
    /// 
    /// Setup:
    /// - Material 0 (Metal): Your tank shell material
    /// - Material 1 (Water): Use SindishTech/WaterFill shader
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

        [Header("Material Index")]
        [Tooltip("Index of the Water material in the renderer's material array")]
        public int waterMaterialIndex = 1;

        [Header("Debug Info")]
        [SerializeField] private float currentFillAmount = 0f;
        [SerializeField] private bool materialFound = false;

        // Shader property IDs
        private static readonly int FillAmountID = Shader.PropertyToID("_FillAmount");
        private static readonly int TemperatureID = Shader.PropertyToID("_Temperature");

        private Material waterMaterial;

        private void OnEnable()
        {
            if (tankRenderer == null)
                tankRenderer = GetComponent<Renderer>();

            GetWaterMaterial();
            
            // Initialize to empty
            if (waterMaterial != null)
            {
                waterMaterial.SetFloat(FillAmountID, 0f);
            }
        }

        private void GetWaterMaterial()
        {
            if (tankRenderer == null)
            {
                materialFound = false;
                Debug.LogWarning("[WaterLevelController] No Tank Renderer assigned!");
                return;
            }

            Material[] mats = Application.isPlaying ? tankRenderer.materials : tankRenderer.sharedMaterials;
            
            if (mats != null && mats.Length > waterMaterialIndex)
            {
                waterMaterial = mats[waterMaterialIndex];
                materialFound = waterMaterial != null;
                
                if (materialFound)
                {
                    Debug.Log($"[WaterLevelController] Water material found: {waterMaterial.name}");
                }
            }
            else
            {
                materialFound = false;
                Debug.LogWarning($"[WaterLevelController] No material at index {waterMaterialIndex}");
            }
        }

        private void Update()
        {
            if (waterMaterial == null)
            {
                GetWaterMaterial();
                if (waterMaterial == null) return;
            }

            if (simulation == null) return;

            UpdateWaterShader();
        }

        private void UpdateWaterShader()
        {
            // Get fill percentage from simulation (0-100) and convert to 0-1
            currentFillAmount = simulation.WaterLevelPercentage / 100f;
            
            // Send to shader
            waterMaterial.SetFloat(FillAmountID, currentFillAmount);
            waterMaterial.SetFloat(TemperatureID, simulation.CurrentTemperature);
            
            // Debug log every 60 frames
            if (Time.frameCount % 60 == 0)
            {
                Debug.Log($"[WaterLevelController] Fill: {currentFillAmount:F2} ({simulation.WaterLevelPercentage:F1}%), Temp: {simulation.CurrentTemperature:F1}°C");
            }
        }

        /// <summary>
        /// Manually set fill level for testing (0-1)
        /// </summary>
        public void SetFillDirect(float fill01)
        {
            if (waterMaterial == null) return;
            
            currentFillAmount = Mathf.Clamp01(fill01);
            waterMaterial.SetFloat(FillAmountID, currentFillAmount);
            Debug.Log($"[WaterLevelController] Direct fill set to: {currentFillAmount:F2}");
        }

        [ContextMenu("Test: Fill 0%")]
        public void TestFill0() => SetFillDirect(0f);

        [ContextMenu("Test: Fill 25%")]
        public void TestFill25() => SetFillDirect(0.25f);

        [ContextMenu("Test: Fill 50%")]
        public void TestFill50() => SetFillDirect(0.5f);

        [ContextMenu("Test: Fill 75%")]
        public void TestFill75() => SetFillDirect(0.75f);

        [ContextMenu("Test: Fill 100%")]
        public void TestFill100() => SetFillDirect(1f);
    }
}
