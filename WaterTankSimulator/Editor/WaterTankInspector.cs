using UnityEngine;
using UnityEditor;

namespace SindishTech.WaterTankSimulator.Editor
{
    /// <summary>
    /// Custom Inspector for the WaterTankSimulation component.
    /// Provides a clean, organized inspector with quick controls.
    /// </summary>
    [CustomEditor(typeof(WaterTankSimulation))]
    [CanEditMultipleObjects]
    public class WaterTankInspector : UnityEditor.Editor
    {
        private WaterTankSimulation tank;

        // Serialized Properties
        private SerializedProperty scenarioProp;
        private SerializedProperty tankCapacityProp;
        private SerializedProperty inletValveOpenProp;
        private SerializedProperty outletValveOpenProp;
        private SerializedProperty inletFlowRateProp;
        private SerializedProperty outletFlowRateProp;
        private SerializedProperty inletWaterTemperatureProp;
        private SerializedProperty ambientTemperatureProp;
        private SerializedProperty coolingRateProp;
        private SerializedProperty heatingRateProp;
        private SerializedProperty basePressureProp;
        private SerializedProperty overheatThresholdProp;
        private SerializedProperty overflowThresholdProp;
        private SerializedProperty emptyThresholdProp;
        private SerializedProperty overPressureThresholdProp;

        // Foldout states
        private bool showValves = true;
        private bool showFlow = true;
        private bool showThermal = true;
        private bool showWarnings = true;
        private bool showStatus = true;

        // Colors
        private static readonly Color AccentGreen = new Color(0.3f, 0.8f, 0.4f, 1f);
        private static readonly Color AccentRed = new Color(0.9f, 0.2f, 0.2f, 1f);
        private static readonly Color AccentBlue = new Color(0.3f, 0.6f, 0.9f, 1f);

        private void OnEnable()
        {
            tank = (WaterTankSimulation)target;

            scenarioProp = serializedObject.FindProperty("scenario");
            tankCapacityProp = serializedObject.FindProperty("tankCapacity");
            inletValveOpenProp = serializedObject.FindProperty("inletValveOpen");
            outletValveOpenProp = serializedObject.FindProperty("outletValveOpen");
            inletFlowRateProp = serializedObject.FindProperty("inletFlowRate");
            outletFlowRateProp = serializedObject.FindProperty("outletFlowRate");
            inletWaterTemperatureProp = serializedObject.FindProperty("inletWaterTemperature");
            ambientTemperatureProp = serializedObject.FindProperty("ambientTemperature");
            coolingRateProp = serializedObject.FindProperty("coolingRate");
            heatingRateProp = serializedObject.FindProperty("heatingRate");
            basePressureProp = serializedObject.FindProperty("basePressure");
            overheatThresholdProp = serializedObject.FindProperty("overheatThreshold");
            overflowThresholdProp = serializedObject.FindProperty("overflowThreshold");
            emptyThresholdProp = serializedObject.FindProperty("emptyThreshold");
            overPressureThresholdProp = serializedObject.FindProperty("overPressureThreshold");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header
            EditorGUILayout.Space(5);
            DrawHeaderBox("Water Tank Simulation", "Configure the tank simulation parameters");
            EditorGUILayout.Space(10);

            // Quick Controls
            DrawQuickControls();
            EditorGUILayout.Space(5);

            // Scenario
            EditorGUILayout.PropertyField(scenarioProp, new GUIContent("Simulation Scenario"));
            EditorGUILayout.Space(5);

            // Tank Properties
            EditorGUILayout.PropertyField(tankCapacityProp, new GUIContent("Tank Capacity (L)"));
            EditorGUILayout.Space(10);

            // Status (Read-Only)
            showStatus = EditorGUILayout.BeginFoldoutHeaderGroup(showStatus, "Live Status");
            if (showStatus)
            {
                EditorGUI.indentLevel++;
                DrawStatusReadout();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(5);

            // Valve Controls
            showValves = EditorGUILayout.BeginFoldoutHeaderGroup(showValves, "Valve Controls");
            if (showValves)
            {
                EditorGUI.indentLevel++;
                DrawValveSection();
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(5);

            // Flow Rates
            showFlow = EditorGUILayout.BeginFoldoutHeaderGroup(showFlow, "Flow Rates");
            if (showFlow)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(inletFlowRateProp, new GUIContent("Inlet (L/s)"));
                EditorGUILayout.PropertyField(outletFlowRateProp, new GUIContent("Outlet (L/s)"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(5);

            // Thermal Control
            showThermal = EditorGUILayout.BeginFoldoutHeaderGroup(showThermal, "Thermal Control");
            if (showThermal)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(ambientTemperatureProp, new GUIContent("Ambient Temp (°C)"));
                EditorGUILayout.PropertyField(basePressureProp, new GUIContent("Base Pressure (bar)"));

                if (tank.scenario == SimulationScenario.HotWaterInlet_CoolingInTank)
                {
                    EditorGUILayout.PropertyField(inletWaterTemperatureProp, new GUIContent("Inlet Temp (°C)"));
                    EditorGUILayout.PropertyField(coolingRateProp, new GUIContent("Cooling Rate (°C/s)"));
                }
                else
                {
                    EditorGUILayout.PropertyField(heatingRateProp, new GUIContent("Heating Rate (°C/s)"));
                }
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space(5);

            // Warning Thresholds
            showWarnings = EditorGUILayout.BeginFoldoutHeaderGroup(showWarnings, "Warning Thresholds");
            if (showWarnings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(overheatThresholdProp, new GUIContent("Overheat (°C)"));
                EditorGUILayout.PropertyField(overflowThresholdProp, new GUIContent("Overflow (%)"));
                EditorGUILayout.PropertyField(emptyThresholdProp, new GUIContent("Empty (%)"));
                EditorGUILayout.PropertyField(overPressureThresholdProp, new GUIContent("Over Pressure (bar)"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(10);

            // Open Editor Window Button
            if (GUILayout.Button("Open Simulation Dashboard", GUILayout.Height(30)))
            {
                WaterTankEditorWindow.ShowWindow();
            }

            serializedObject.ApplyModifiedProperties();

            // Force repaint while simulation is running
            if (tank.IsSimulationRunning)
            {
                Repaint();
            }
        }

        private void DrawHeaderBox(string title, string subtitle)
        {
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            var subtitleStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(title, headerStyle);
            EditorGUILayout.LabelField(subtitle, subtitleStyle);
            EditorGUILayout.EndVertical();
        }

        private void DrawQuickControls()
        {
            EditorGUILayout.BeginHorizontal();

            // Start/Stop
            GUI.backgroundColor = tank.IsSimulationRunning ? AccentRed : AccentGreen;
            string btnText = tank.IsSimulationRunning ? "■ Stop" : "▶ Start";
            if (GUILayout.Button(btnText, GUILayout.Height(28)))
            {
                if (tank.IsSimulationRunning)
                    tank.StopSimulation();
                else
                    tank.StartSimulation();
            }

            // Reset
            GUI.backgroundColor = new Color(0.9f, 0.5f, 0.1f, 1f);
            if (GUILayout.Button("↺ Reset", GUILayout.Height(28)))
            {
                tank.ResetSimulation();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawStatusReadout()
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.FloatField("Water Level (L)", tank.CurrentWaterLevel);
            EditorGUILayout.FloatField("Level (%)", tank.WaterLevelPercentage);
            EditorGUILayout.FloatField("Temperature (°C)", tank.CurrentTemperature);
            EditorGUILayout.FloatField("Pressure (bar)", tank.CurrentPressure);
            EditorGUI.EndDisabledGroup();

            // Warning display
            if (tank.ActiveWarnings != TankWarnings.None)
            {
                EditorGUILayout.Space(3);
                var warnStyle = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = AccentRed } };

                if ((tank.ActiveWarnings & TankWarnings.Overheat) != 0)
                    EditorGUILayout.LabelField("⚠ OVERHEAT", warnStyle);
                if ((tank.ActiveWarnings & TankWarnings.Overflow) != 0)
                    EditorGUILayout.LabelField("⚠ OVERFLOW", warnStyle);
                if ((tank.ActiveWarnings & TankWarnings.EmptyTank) != 0)
                    EditorGUILayout.LabelField("⚠ EMPTY TANK", warnStyle);
                if ((tank.ActiveWarnings & TankWarnings.OverPressure) != 0)
                    EditorGUILayout.LabelField("⚠ OVER PRESSURE", warnStyle);
            }
        }

        private void DrawValveSection()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Inlet:", GUILayout.Width(60));
            GUI.backgroundColor = tank.inletValveOpen ? AccentGreen : AccentRed;
            if (GUILayout.Button(tank.inletValveOpen ? "OPEN" : "CLOSED", GUILayout.Height(24)))
            {
                Undo.RecordObject(tank, "Toggle Inlet Valve");
                tank.ToggleInletValve();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Outlet:", GUILayout.Width(60));
            GUI.backgroundColor = tank.outletValveOpen ? AccentGreen : AccentRed;
            if (GUILayout.Button(tank.outletValveOpen ? "OPEN" : "CLOSED", GUILayout.Height(24)))
            {
                Undo.RecordObject(tank, "Toggle Outlet Valve");
                tank.ToggleOutletValve();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }
    }
}
