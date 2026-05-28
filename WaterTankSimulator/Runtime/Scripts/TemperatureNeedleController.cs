using UnityEngine;

namespace SindishTech.WaterTankSimulator
{
    /// <summary>
    /// Controls the temperature gauge needle rotation based on simulation temperature.
    /// Attach this to the "needle" object in your Blender-imported model.
    /// 
    /// SETUP:
    /// 1. Position the needle at 0°C (rest position) in the scene
    /// 2. Click "Capture Zero Position" in context menu
    /// 3. Assign the simulation reference
    /// 4. Adjust minAngle/maxAngle if needed
    /// 
    /// Default: X-axis rotation, 0° at 0°C, -260° at 340°C
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("SindishTech/Temperature Needle Controller")]
    public class TemperatureNeedleController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("Reference to the WaterTankSimulation component")]
        public WaterTankSimulation simulation;

        [Header("Needle Configuration")]
        [Tooltip("Which local axis the needle rotates around")]
        public RotationAxisOption axis = RotationAxisOption.X;

        [Tooltip("Euler angle at minimum temperature (0°C)")]
        [Range(-360f, 360f)]
        public float minAngle = 0f;

        [Tooltip("Euler angle at maximum temperature (340°C)")]
        [Range(-360f, 360f)]
        public float maxAngle = -260f;

        [Tooltip("Minimum temperature (maps to minAngle)")]
        public float minTemperature = 0f;

        [Tooltip("Maximum temperature (maps to maxAngle)")]
        public float maxTemperature = 340f;

        [Header("Rest Position (Captured)")]
        [Tooltip("The local euler angles when needle is at 0°C / rest. Click 'Capture Zero Position' to set.")]
        public Vector3 restEulerAngles = Vector3.zero;

        [Header("Smoothing")]
        [Tooltip("How smoothly the needle moves to target position")]
        [Range(1f, 20f)]
        public float smoothSpeed = 8f;

        [Tooltip("Enable smooth needle movement")]
        public bool enableSmoothing = true;

        [Header("Needle Bounce (Realistic)")]
        [Tooltip("Enable bounce/overshoot effect")]
        public bool enableBounce = true;

        [Tooltip("Bounce damping factor")]
        [Range(0.1f, 1f)]
        public float bounceDamping = 0.7f;

        [Tooltip("Bounce spring stiffness")]
        [Range(1f, 50f)]
        public float bounceStiffness = 15f;

        [Header("Debug (Read Only)")]
        [SerializeField] private float currentAngle = 0f;
        [SerializeField] private float targetAngle = 0f;
        [SerializeField] private float debugTemperature = 0f;

        // Internal
        private float velocity = 0f;

        public enum RotationAxisOption { X, Y, Z }

        private void OnEnable()
        {
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

            // Get current temperature
            float temperature = simulation.CurrentTemperature;
            debugTemperature = temperature;

            // Map temperature to angle
            float t = Mathf.InverseLerp(minTemperature, maxTemperature, temperature);
            targetAngle = Mathf.Lerp(minAngle, maxAngle, t);

            // Apply smoothing
            if (enableSmoothing)
            {
                if (enableBounce)
                {
                    float springForce = (targetAngle - currentAngle) * bounceStiffness;
                    float dampingForce = -velocity * bounceDamping * 10f;
                    float acceleration = springForce + dampingForce;

                    float dt = GetDeltaTime();
                    velocity += acceleration * dt;
                    currentAngle += velocity * dt;
                }
                else
                {
                    float dt = GetDeltaTime();
                    currentAngle = Mathf.Lerp(currentAngle, targetAngle, smoothSpeed * dt);
                }
            }
            else
            {
                currentAngle = targetAngle;
                velocity = 0f;
            }

            // Apply the rotation directly using euler angles
            ApplyNeedleRotation(currentAngle);
        }

        private void ApplyNeedleRotation(float angle)
        {
            Vector3 euler = restEulerAngles;

            switch (axis)
            {
                case RotationAxisOption.X:
                    euler.x = restEulerAngles.x + angle;
                    break;
                case RotationAxisOption.Y:
                    euler.y = restEulerAngles.y + angle;
                    break;
                case RotationAxisOption.Z:
                    euler.z = restEulerAngles.z + angle;
                    break;
            }

            transform.localEulerAngles = euler;
        }

        private float GetDeltaTime()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return 0.016f;
#endif
            return Time.deltaTime;
        }

        private void OnTemperatureChanged(float temperature)
        {
            float t = Mathf.InverseLerp(minTemperature, maxTemperature, temperature);
            targetAngle = Mathf.Lerp(minAngle, maxAngle, t);
        }

        /// <summary>
        /// Call this when the needle is visually pointing at 0°C.
        /// It captures the current rotation as the rest/zero position.
        /// </summary>
        [ContextMenu("Capture Zero Position")]
        public void CaptureZeroPosition()
        {
            restEulerAngles = transform.localEulerAngles;
            currentAngle = 0f;
            targetAngle = 0f;
            velocity = 0f;
            Debug.Log($"[NeedleController] Zero position captured: {restEulerAngles}");
        }

        /// <summary>
        /// Reset needle to rest/zero position.
        /// </summary>
        [ContextMenu("Reset Needle")]
        public void ResetNeedle()
        {
            currentAngle = minAngle;
            targetAngle = minAngle;
            velocity = 0f;
            ApplyNeedleRotation(currentAngle);
        }

        /// <summary>
        /// Test: Move needle to max temperature position.
        /// </summary>
        [ContextMenu("Test Max Position")]
        public void TestMaxPosition()
        {
            currentAngle = maxAngle;
            targetAngle = maxAngle;
            velocity = 0f;
            ApplyNeedleRotation(currentAngle);
        }

        /// <summary>
        /// Test: Move needle to mid position (~170°C).
        /// </summary>
        [ContextMenu("Test Mid Position")]
        public void TestMidPosition()
        {
            float midAngle = (minAngle + maxAngle) / 2f;
            currentAngle = midAngle;
            targetAngle = midAngle;
            velocity = 0f;
            ApplyNeedleRotation(midAngle);
        }
    }
}
