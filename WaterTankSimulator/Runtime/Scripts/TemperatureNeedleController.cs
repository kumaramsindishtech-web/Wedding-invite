using UnityEngine;

namespace SindishTech.WaterTankSimulator
{
    /// <summary>
    /// Controls the temperature gauge needle rotation based on simulation temperature.
    /// Attach this to the "needle" object in your Blender-imported model.
    /// The needle rotates around its local Z-axis (or configurable axis).
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("SindishTech/Temperature Needle Controller")]
    public class TemperatureNeedleController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the WaterTankSimulation component")]
        public WaterTankSimulation simulation;

        [Header("Needle Configuration")]
        [Tooltip("Rotation axis for the needle (local space) - Set to (1,0,0) for X-axis rotation")]
        public Vector3 rotationAxis = Vector3.right; // X-axis by default

        [Tooltip("Angle when temperature is at minimum (0°C) - X rotation value")]
        [Range(-360f, 360f)]
        public float minAngle = 0f; // 0 degrees at 0°C

        [Tooltip("Angle when temperature is at maximum (340°C) - X rotation value")]
        [Range(-360f, 360f)]
        public float maxAngle = -260f; // -260 degrees at 340°C

        [Tooltip("Minimum temperature value (maps to minAngle)")]
        public float minTemperature = 0f;

        [Tooltip("Maximum temperature value (maps to maxAngle)")]
        public float maxTemperature = 340f;

        [Header("Smoothing")]
        [Tooltip("How smoothly the needle moves to target position")]
        [Range(1f, 20f)]
        public float smoothSpeed = 8f;

        [Tooltip("Enable smooth needle movement")]
        public bool enableSmoothing = true;

        [Header("Needle Bounce (Realistic)")]
        [Tooltip("Enable bounce/overshoot when needle reaches target")]
        public bool enableBounce = true;

        [Tooltip("Bounce damping factor")]
        [Range(0.1f, 1f)]
        public float bounceDamping = 0.7f;

        [Tooltip("Bounce spring stiffness")]
        [Range(1f, 50f)]
        public float bounceStiffness = 15f;

        [Header("Debug")]
        [Tooltip("Show the current needle angle in inspector")]
        [SerializeField]
        private float currentAngle = 0f;

        [SerializeField]
        private float targetAngle = 0f;

        // Internal state for bounce physics
        private float velocity = 0f;
        private Quaternion initialRotation;
        private bool initialized = false;

        private void OnEnable()
        {
            if (!initialized)
            {
                initialRotation = transform.localRotation;
                initialized = true;
            }

            if (simulation != null)
            {
                simulation.OnTemperatureChanged += OnTemperatureChanged;
            }
        }

        private void OnDisable()
        {
            if (simulation != null)
            {
                simulation.OnTemperatureChanged -= OnTemperatureChanged;
            }
        }

        private void Update()
        {
            if (simulation == null) return;

            // Calculate target angle based on current temperature
            float tempNormalized = Mathf.InverseLerp(minTemperature, maxTemperature, simulation.CurrentTemperature);
            targetAngle = Mathf.Lerp(minAngle, maxAngle, tempNormalized);

            // Apply smoothing or direct positioning
            if (enableSmoothing)
            {
                if (enableBounce)
                {
                    // Spring-damper physics for realistic needle bounce
                    float springForce = (targetAngle - currentAngle) * bounceStiffness;
                    float dampingForce = -velocity * bounceDamping * 10f;
                    float acceleration = springForce + dampingForce;

                    float deltaTime = GetDeltaTime();
                    velocity += acceleration * deltaTime;
                    currentAngle += velocity * deltaTime;
                }
                else
                {
                    // Simple smooth lerp
                    float deltaTime = GetDeltaTime();
                    currentAngle = Mathf.Lerp(currentAngle, targetAngle, smoothSpeed * deltaTime);
                }
            }
            else
            {
                currentAngle = targetAngle;
                velocity = 0f;
            }

            // Apply rotation
            ApplyRotation(currentAngle);
        }

        private void ApplyRotation(float angle)
        {
            Quaternion rotation = Quaternion.AngleAxis(angle, rotationAxis);
            transform.localRotation = initialRotation * rotation;
        }

        private float GetDeltaTime()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // Use a fixed delta time in edit mode
                return 0.016f; // ~60fps equivalent
            }
#endif
            return Time.deltaTime;
        }

        private void OnTemperatureChanged(float temperature)
        {
            // Event-driven update (also updates via Update loop)
            float tempNormalized = Mathf.InverseLerp(minTemperature, maxTemperature, temperature);
            targetAngle = Mathf.Lerp(minAngle, maxAngle, tempNormalized);
        }

        /// <summary>
        /// Reset needle to zero/rest position.
        /// </summary>
        [ContextMenu("Reset Needle")]
        public void ResetNeedle()
        {
            currentAngle = minAngle;
            targetAngle = minAngle;
            velocity = 0f;
            ApplyRotation(currentAngle);
        }

        /// <summary>
        /// Calibrate the needle by setting current position as the initial rotation.
        /// </summary>
        [ContextMenu("Set Current As Initial Rotation")]
        public void CalibrateInitialRotation()
        {
            initialRotation = transform.localRotation;
            currentAngle = 0f;
            velocity = 0f;
        }

        private void OnValidate()
        {
            if (!initialized)
            {
                initialRotation = transform.localRotation;
                initialized = true;
            }
        }
    }
}
