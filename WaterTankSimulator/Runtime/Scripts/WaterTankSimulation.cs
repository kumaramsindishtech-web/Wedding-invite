using UnityEngine;
using System;

namespace SindishTech.WaterTankSimulator
{
    /// <summary>
    /// Simulation scenario type for the water tank.
    /// </summary>
    public enum SimulationScenario
    {
        HotWaterInlet_CoolingInTank,
        ColdWaterInlet_HeatingInTank
    }

    /// <summary>
    /// Warning flags for the tank simulation.
    /// </summary>
    [Flags]
    public enum TankWarnings
    {
        None = 0,
        Overheat = 1 << 0,
        Overflow = 1 << 1,
        EmptyTank = 1 << 2,
        OverPressure = 1 << 3
    }

    /// <summary>
    /// Core simulation component for the Water Tank system.
    /// Handles water flow, temperature, pressure, and level calculations.
    /// Works in Edit mode for visualization/training purposes.
    /// </summary>
    [ExecuteInEditMode]
    [AddComponentMenu("SindishTech/Water Tank Simulation")]
    public class WaterTankSimulation : MonoBehaviour
    {
        // ─── SCENARIO ────────────────────────────────────────────────
        [Header("Simulation Scenario")]
        [Tooltip("Select simulation mode")]
        public SimulationScenario scenario = SimulationScenario.HotWaterInlet_CoolingInTank;

        // ─── TANK PROPERTIES ─────────────────────────────────────────
        [Header("Tank Properties")]
        [Tooltip("Maximum tank capacity in liters")]
        [Range(100f, 50000f)]
        public float tankCapacity = 1000f;

        [Tooltip("Current water level in liters (read-only at runtime)")]
        [SerializeField]
        private float currentWaterLevel = 0f;

        // ─── VALVE CONTROLS ──────────────────────────────────────────
        [Header("Valve Controls (Binary)")]
        [Tooltip("Inlet valve state: true = open, false = closed")]
        public bool inletValveOpen = false;

        [Tooltip("Outlet valve state: true = open, false = closed")]
        public bool outletValveOpen = false;

        // ─── FLOW RATES ──────────────────────────────────────────────
        [Header("Flow Rates")]
        [Tooltip("Inlet flow rate in liters per second")]
        [Range(0.1f, 100f)]
        public float inletFlowRate = 5f;

        [Tooltip("Outlet flow rate in liters per second")]
        [Range(0.1f, 100f)]
        public float outletFlowRate = 3f;

        // ─── TEMPERATURE ─────────────────────────────────────────────
        [Header("Temperature Settings")]
        [Tooltip("Current water temperature in Celsius (read-only at runtime)")]
        [SerializeField]
        private float currentTemperature = 25f;

        [Tooltip("Inlet water temperature in Celsius")]
        [Range(0f, 340f)]
        public float inletWaterTemperature = 200f;

        [Tooltip("Ambient/environment temperature in Celsius")]
        [Range(-20f, 50f)]
        public float ambientTemperature = 25f;

        [Tooltip("Cooling rate in degrees Celsius per second (Scenario 1)")]
        [Range(0.1f, 50f)]
        public float coolingRate = 3f;

        [Tooltip("Heating rate in degrees Celsius per second (Scenario 2)")]
        [Range(0.1f, 50f)]
        public float heatingRate = 5f;

        // ─── PRESSURE ────────────────────────────────────────────────
        [Header("Pressure Settings")]
        [Tooltip("Base pressure in bar (atmospheric)")]
        [Range(0.5f, 2f)]
        public float basePressure = 1.013f;

        [Tooltip("Current pressure in bar (read-only at runtime)")]
        [SerializeField]
        private float currentPressure = 1.013f;

        // ─── WARNING THRESHOLDS ──────────────────────────────────────
        [Header("Warning Thresholds")]
        [Tooltip("Temperature threshold for overheat warning (Celsius)")]
        [Range(100f, 340f)]
        public float overheatThreshold = 300f;

        [Tooltip("Water level percentage for overflow warning")]
        [Range(80f, 100f)]
        public float overflowThreshold = 95f;

        [Tooltip("Water level percentage for empty tank warning")]
        [Range(0f, 20f)]
        public float emptyThreshold = 5f;

        [Tooltip("Pressure threshold for over-pressure warning (bar)")]
        [Range(2f, 20f)]
        public float overPressureThreshold = 8f;

        // ─── SIMULATION STATE ────────────────────────────────────────
        [Header("Simulation State (Read-Only)")]
        [SerializeField]
        private TankWarnings activeWarnings = TankWarnings.None;

        [SerializeField]
        private bool simulationRunning = false;

        private double lastEditorTime;

        // ─── PUBLIC PROPERTIES ───────────────────────────────────────
        public float CurrentWaterLevel => currentWaterLevel;
        public float CurrentTemperature => currentTemperature;
        public float CurrentPressure => currentPressure;
        public float WaterLevelPercentage => tankCapacity > 0 ? (currentWaterLevel / tankCapacity) * 100f : 0f;
        public TankWarnings ActiveWarnings => activeWarnings;
        public bool IsSimulationRunning => simulationRunning;

        // ─── EVENTS ─────────────────────────────────────────────────
        public event Action<float> OnWaterLevelChanged;
        public event Action<float> OnTemperatureChanged;
        public event Action<float> OnPressureChanged;
        public event Action<TankWarnings> OnWarningTriggered;

        // ─── UNITY LIFECYCLE ─────────────────────────────────────────

        private void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update += EditorUpdate;
            lastEditorTime = UnityEditor.EditorApplication.timeSinceStartup;
#endif
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= EditorUpdate;
#endif
        }

#if UNITY_EDITOR
        private void EditorUpdate()
        {
            if (!simulationRunning) return;

            double currentTime = UnityEditor.EditorApplication.timeSinceStartup;
            float deltaTime = (float)(currentTime - lastEditorTime);
            lastEditorTime = currentTime;

            // Clamp deltaTime to avoid large jumps
            deltaTime = Mathf.Clamp(deltaTime, 0f, 0.1f);

            UpdateSimulation(deltaTime);
        }
#endif

        // ─── SIMULATION LOGIC ────────────────────────────────────────

        public void UpdateSimulation(float deltaTime)
        {
            UpdateWaterLevel(deltaTime);
            UpdateTemperature(deltaTime);
            UpdatePressure();
            CheckWarnings();
        }

        private void UpdateWaterLevel(float deltaTime)
        {
            float previousLevel = currentWaterLevel;

            // Inlet flow
            if (inletValveOpen)
            {
                currentWaterLevel += inletFlowRate * deltaTime;
            }

            // Outlet flow (only if there's water in the tank)
            if (outletValveOpen && currentWaterLevel > 0f)
            {
                currentWaterLevel -= outletFlowRate * deltaTime;
            }

            // Clamp water level
            currentWaterLevel = Mathf.Clamp(currentWaterLevel, 0f, tankCapacity);

            if (!Mathf.Approximately(previousLevel, currentWaterLevel))
            {
                OnWaterLevelChanged?.Invoke(currentWaterLevel);
            }
        }

        private void UpdateTemperature(float deltaTime)
        {
            float previousTemp = currentTemperature;

            if (scenario == SimulationScenario.HotWaterInlet_CoolingInTank)
            {
                // Hot water flows in, mixing temperature
                if (inletValveOpen && currentWaterLevel > 0f)
                {
                    float inletVolume = inletFlowRate * deltaTime;
                    float mixRatio = inletVolume / currentWaterLevel;
                    mixRatio = Mathf.Clamp01(mixRatio);
                    currentTemperature = Mathf.Lerp(currentTemperature, inletWaterTemperature, mixRatio);
                }

                // Cooling towards ambient
                if (currentWaterLevel > 0f && currentTemperature > ambientTemperature)
                {
                    float coolAmount = coolingRate * deltaTime;
                    currentTemperature = Mathf.MoveTowards(currentTemperature, ambientTemperature, coolAmount);
                }
            }
            else // ColdWaterInlet_HeatingInTank
            {
                // Cold water flows in at ambient temperature
                if (inletValveOpen && currentWaterLevel > 0f)
                {
                    float inletVolume = inletFlowRate * deltaTime;
                    float mixRatio = inletVolume / currentWaterLevel;
                    mixRatio = Mathf.Clamp01(mixRatio);
                    currentTemperature = Mathf.Lerp(currentTemperature, ambientTemperature, mixRatio);
                }

                // Heating in tank
                if (currentWaterLevel > 0f)
                {
                    currentTemperature += heatingRate * deltaTime;
                }
            }

            // Clamp temperature
            currentTemperature = Mathf.Clamp(currentTemperature, -20f, 340f);

            if (!Mathf.Approximately(previousTemp, currentTemperature))
            {
                OnTemperatureChanged?.Invoke(currentTemperature);
            }
        }

        private void UpdatePressure()
        {
            float previousPressure = currentPressure;

            // Pressure based on temperature and water level
            // Using simplified gas law: P = basePressure * (T/100) * (level/capacity)
            float tempFactor = 1f + (currentTemperature / 100f) * 0.5f;
            float levelFactor = 1f + (WaterLevelPercentage / 100f) * 0.3f;
            currentPressure = basePressure * tempFactor * levelFactor;

            // Clamp pressure
            currentPressure = Mathf.Clamp(currentPressure, 0f, 20f);

            if (!Mathf.Approximately(previousPressure, currentPressure))
            {
                OnPressureChanged?.Invoke(currentPressure);
            }
        }

        private void CheckWarnings()
        {
            TankWarnings previousWarnings = activeWarnings;
            activeWarnings = TankWarnings.None;

            if (currentTemperature >= overheatThreshold)
                activeWarnings |= TankWarnings.Overheat;

            if (WaterLevelPercentage >= overflowThreshold)
                activeWarnings |= TankWarnings.Overflow;

            if (WaterLevelPercentage <= emptyThreshold && currentWaterLevel > 0f)
                activeWarnings |= TankWarnings.EmptyTank;

            if (currentWaterLevel <= 0f)
                activeWarnings |= TankWarnings.EmptyTank;

            if (currentPressure >= overPressureThreshold)
                activeWarnings |= TankWarnings.OverPressure;

            if (previousWarnings != activeWarnings)
            {
                OnWarningTriggered?.Invoke(activeWarnings);
            }
        }

        // ─── PUBLIC METHODS ──────────────────────────────────────────

        /// <summary>
        /// Start the simulation in Edit mode.
        /// </summary>
        public void StartSimulation()
        {
            simulationRunning = true;
#if UNITY_EDITOR
            lastEditorTime = UnityEditor.EditorApplication.timeSinceStartup;
#endif
        }

        /// <summary>
        /// Stop/Pause the simulation.
        /// </summary>
        public void StopSimulation()
        {
            simulationRunning = false;
        }

        /// <summary>
        /// Reset simulation to default state.
        /// </summary>
        public void ResetSimulation()
        {
            simulationRunning = false;
            currentWaterLevel = 0f;
            currentTemperature = ambientTemperature;
            currentPressure = basePressure;
            inletValveOpen = false;
            outletValveOpen = false;
            activeWarnings = TankWarnings.None;

            OnWaterLevelChanged?.Invoke(currentWaterLevel);
            OnTemperatureChanged?.Invoke(currentTemperature);
            OnPressureChanged?.Invoke(currentPressure);
            OnWarningTriggered?.Invoke(activeWarnings);
        }

        /// <summary>
        /// Toggle inlet valve state.
        /// </summary>
        public void ToggleInletValve()
        {
            inletValveOpen = !inletValveOpen;
        }

        /// <summary>
        /// Toggle outlet valve state.
        /// </summary>
        public void ToggleOutletValve()
        {
            outletValveOpen = !outletValveOpen;
        }

        /// <summary>
        /// Set water level directly (for testing purposes).
        /// </summary>
        public void SetWaterLevel(float liters)
        {
            currentWaterLevel = Mathf.Clamp(liters, 0f, tankCapacity);
            OnWaterLevelChanged?.Invoke(currentWaterLevel);
        }

        /// <summary>
        /// Set temperature directly (for testing purposes).
        /// </summary>
        public void SetTemperature(float celsius)
        {
            currentTemperature = Mathf.Clamp(celsius, -20f, 340f);
            OnTemperatureChanged?.Invoke(currentTemperature);
        }
    }
}
