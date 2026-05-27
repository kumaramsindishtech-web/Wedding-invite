using UnityEngine;

namespace SindishTech.WaterTankSimulator
{
    /// <summary>
    /// Controls valve open/close animation on the 3D model.
    /// Attach to "Valve_Inlet" or "Valve_Outlet" objects from the Blender model.
    /// Rotates the valve handle between open and closed positions (binary).
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("SindishTech/Valve Controller")]
    public class ValveController : MonoBehaviour
    {
        public enum ValveType
        {
            Inlet,
            Outlet
        }

        [Header("References")]
        [Tooltip("Reference to the WaterTankSimulation component")]
        public WaterTankSimulation simulation;

        [Header("Valve Configuration")]
        [Tooltip("Which valve this controller represents")]
        public ValveType valveType = ValveType.Inlet;

        [Tooltip("The valve handle/wheel transform to rotate (if different from this object)")]
        public Transform valveHandle;

        [Tooltip("Rotation axis for the valve handle (local space)")]
        public Vector3 rotationAxis = Vector3.up;

        [Tooltip("Rotation angle when valve is closed (degrees)")]
        [Range(-360f, 360f)]
        public float closedAngle = 0f;

        [Tooltip("Rotation angle when valve is open (degrees)")]
        [Range(-360f, 360f)]
        public float openAngle = 90f;

        [Header("Animation")]
        [Tooltip("Speed of valve open/close animation")]
        [Range(1f, 20f)]
        public float animationSpeed = 5f;

        [Tooltip("Enable smooth animation between states")]
        public bool enableAnimation = true;

        [Header("Visual Feedback")]
        [Tooltip("Optional: Material to change color when valve state changes")]
        public Renderer valveRenderer;

        [Tooltip("Color when valve is open")]
        public Color openColor = new Color(0.2f, 0.8f, 0.3f, 1f);

        [Tooltip("Color when valve is closed")]
        public Color closedColor = new Color(0.8f, 0.2f, 0.2f, 1f);

        [Tooltip("Enable color change on valve state")]
        public bool enableColorFeedback = false;

        [Header("Debug")]
        [SerializeField]
        private bool currentState = false;

        [SerializeField]
        private float currentRotation = 0f;

        private Quaternion initialRotation;
        private Material valveMaterial;
        private bool initialized = false;
        private static readonly int ColorID = Shader.PropertyToID("_Color");
        private static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");

        private void OnEnable()
        {
            if (!initialized)
            {
                Transform target = valveHandle != null ? valveHandle : transform;
                initialRotation = target.localRotation;
                initialized = true;
            }

            if (valveRenderer != null)
            {
                if (Application.isPlaying)
                    valveMaterial = valveRenderer.material;
                else
                    valveMaterial = valveRenderer.sharedMaterial;
            }
        }

        private void Update()
        {
            if (simulation == null) return;

            // Get current valve state from simulation
            bool targetState = valveType == ValveType.Inlet
                ? simulation.inletValveOpen
                : simulation.outletValveOpen;

            currentState = targetState;

            // Calculate target rotation
            float targetRotation = targetState ? openAngle : closedAngle;

            // Animate or snap
            if (enableAnimation)
            {
                float deltaTime = GetDeltaTime();
                currentRotation = Mathf.Lerp(currentRotation, targetRotation, animationSpeed * deltaTime);
            }
            else
            {
                currentRotation = targetRotation;
            }

            // Apply rotation to valve handle
            ApplyRotation(currentRotation);

            // Apply color feedback
            if (enableColorFeedback && valveMaterial != null)
            {
                Color targetColor = targetState ? openColor : closedColor;
                Color currentColor = valveMaterial.HasProperty(BaseColorID)
                    ? valveMaterial.GetColor(BaseColorID)
                    : valveMaterial.GetColor(ColorID);

                float deltaTime = GetDeltaTime();
                Color newColor = Color.Lerp(currentColor, targetColor, animationSpeed * deltaTime);

                if (valveMaterial.HasProperty(BaseColorID))
                    valveMaterial.SetColor(BaseColorID, newColor);
                else if (valveMaterial.HasProperty(ColorID))
                    valveMaterial.SetColor(ColorID, newColor);
            }
        }

        private void ApplyRotation(float angle)
        {
            Transform target = valveHandle != null ? valveHandle : transform;
            Quaternion rotation = Quaternion.AngleAxis(angle, rotationAxis);
            target.localRotation = initialRotation * rotation;
        }

        private float GetDeltaTime()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return 0.016f; // ~60fps in edit mode
            }
#endif
            return Time.deltaTime;
        }

        /// <summary>
        /// Set current transform as initial/closed rotation.
        /// </summary>
        [ContextMenu("Set Current As Closed Position")]
        public void CalibrateClosedPosition()
        {
            Transform target = valveHandle != null ? valveHandle : transform;
            initialRotation = target.localRotation;
            currentRotation = closedAngle;
            initialized = true;
        }

        /// <summary>
        /// Preview open position in editor.
        /// </summary>
        [ContextMenu("Preview Open Position")]
        public void PreviewOpen()
        {
            ApplyRotation(openAngle);
        }

        /// <summary>
        /// Preview closed position in editor.
        /// </summary>
        [ContextMenu("Preview Closed Position")]
        public void PreviewClosed()
        {
            ApplyRotation(closedAngle);
        }

        private void OnValidate()
        {
            if (!initialized && (valveHandle != null || transform != null))
            {
                Transform target = valveHandle != null ? valveHandle : transform;
                initialRotation = target.localRotation;
                initialized = true;
            }
        }
    }
}
