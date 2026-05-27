using UnityEngine;
using UnityEditor;
using System.Linq;

namespace SindishTech.WaterTankSimulator.Editor
{
    /// <summary>
    /// Custom Editor Window for the Water Tank Simulation.
    /// Provides visual controls, gauges, and status indicators.
    /// </summary>
    public class WaterTankEditorWindow : EditorWindow
    {
        private WaterTankSimulation simulation;
        private Vector2 scrollPosition;

        // UI Colors
        private static readonly Color HeaderColor = new Color(0.15f, 0.15f, 0.2f, 1f);
        private static readonly Color PanelColor = new Color(0.2f, 0.2f, 0.25f, 1f);
        private static readonly Color AccentBlue = new Color(0.3f, 0.6f, 0.9f, 1f);
        private static readonly Color AccentGreen = new Color(0.3f, 0.8f, 0.4f, 1f);
        private static readonly Color AccentRed = new Color(0.9f, 0.2f, 0.2f, 1f);
        private static readonly Color AccentYellow = new Color(0.9f, 0.8f, 0.2f, 1f);
        private static readonly Color AccentOrange = new Color(0.9f, 0.5f, 0.1f, 1f);
        private static readonly Color WaterColor = new Color(0.2f, 0.5f, 0.9f, 0.8f);
        private static readonly Color EmptyColor = new Color(0.1f, 0.1f, 0.15f, 1f);

        // Styles (lazy initialized)
        private GUIStyle headerStyle;
        private GUIStyle subHeaderStyle;
        private GUIStyle valueStyle;
        private GUIStyle warningStyle;
        private GUIStyle buttonStyle;
        private GUIStyle panelStyle;

        [MenuItem("SindishTech/Water Tank Simulator")]
        public static void ShowWindow()
        {
            var window = GetWindow<WaterTankEditorWindow>("Water Tank Simulator");
            window.minSize = new Vector2(400, 700);
            window.Show();
        }

        private void OnEnable()
        {
            EditorApplication.update += Repaint;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Repaint;
        }

        private void InitStyles()
        {
            if (headerStyle != null) return;

            headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            subHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                normal = { textColor = AccentBlue }
            };

            valueStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = Color.white }
            };

            warningStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleCenter
            };

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                fixedHeight = 30
            };

            panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(5, 5, 5, 5)
            };
        }

        private void OnGUI()
        {
            InitStyles();

            // Find simulation in scene
            if (simulation == null)
            {
                simulation = FindObjectOfType<WaterTankSimulation>();
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Header
            DrawHeader();

            if (simulation == null)
            {
                DrawNoSimulationMessage();
                EditorGUILayout.EndScrollView();
                return;
            }

            // Main Panels
            DrawSimulationControls();
            EditorGUILayout.Space(5);
            DrawScenarioSelection();
            EditorGUILayout.Space(5);
            DrawValveControls();
            EditorGUILayout.Space(5);
            DrawFlowRates();
            EditorGUILayout.Space(5);
            DrawTankStatus();
            EditorGUILayout.Space(5);
            DrawTemperatureGauge();
            EditorGUILayout.Space(5);
            DrawPressureGauge();
            EditorGUILayout.Space(5);
            DrawThermalControl();
            EditorGUILayout.Space(5);
            DrawWarnings();

            EditorGUILayout.EndScrollView();

            // Mark dirty for undo support
            if (GUI.changed && simulation != null)
            {
                EditorUtility.SetDirty(simulation);
            }
        }

        // ─── HEADER ─────────────────────────────────────────────────

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(panelStyle);
            var rect = GUILayoutUtility.GetRect(GUIContent.none, headerStyle, GUILayout.Height(40));
            EditorGUI.DrawRect(rect, HeaderColor);
            EditorGUI.LabelField(rect, "WATER TANK SIMULATION CONTROLLER", headerStyle);
            EditorGUILayout.EndVertical();
        }

        private void DrawNoSimulationMessage()
        {
            EditorGUILayout.Space(20);
            EditorGUILayout.HelpBox(
                "No WaterTankSimulation component found in the scene.\n\n" +
                "Add the 'WaterTankSimulation' component to a GameObject in your scene to begin.",
                MessageType.Warning);

            EditorGUILayout.Space(10);
            if (GUILayout.Button("Create Water Tank Simulation", buttonStyle))
            {
                var go = new GameObject("WaterTankSimulation");
                go.AddComponent<WaterTankSimulation>();
                Selection.activeGameObject = go;
                simulation = go.GetComponent<WaterTankSimulation>();
            }
        }

        // ─── SIMULATION CONTROLS ────────────────────────────────────

        private void DrawSimulationControls()
        {
            EditorGUILayout.BeginVertical(panelStyle);
            EditorGUILayout.LabelField("SIMULATION CONTROL", subHeaderStyle);
            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            // Start/Stop Button
            GUI.backgroundColor = simulation.IsSimulationRunning ? AccentRed : AccentGreen;
            string simButtonText = simulation.IsSimulationRunning ? "STOP" : "START";
            if (GUILayout.Button(simButtonText, buttonStyle, GUILayout.Width(120)))
            {
                if (simulation.IsSimulationRunning)
                    simulation.StopSimulation();
                else
                    simulation.StartSimulation();
            }

            // Reset Button
            GUI.backgroundColor = AccentOrange;
            if (GUILayout.Button("RESET", buttonStyle, GUILayout.Width(120)))
            {
                simulation.ResetSimulation();
            }

            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();

            // Status indicator
            EditorGUILayout.Space(5);
            string statusText = simulation.IsSimulationRunning ? "● RUNNING" : "○ STOPPED";
            Color statusColor = simulation.IsSimulationRunning ? AccentGreen : Color.gray;
            var statusStyle = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = statusColor } };
            EditorGUILayout.LabelField(statusText, statusStyle);

            EditorGUILayout.EndVertical();
        }

        // ─── SCENARIO SELECTION ─────────────────────────────────────

        private void DrawScenarioSelection()
        {
            EditorGUILayout.BeginVertical(panelStyle);
            EditorGUILayout.LabelField("SCENARIO", subHeaderStyle);
            EditorGUILayout.Space(5);

            simulation.scenario = (SimulationScenario)EditorGUILayout.EnumPopup("Mode", simulation.scenario);

            string description = simulation.scenario == SimulationScenario.HotWaterInlet_CoolingInTank
                ? "Hot water flows in → Cools down in tank over time"
                : "Cold water flows in → Heated inside the tank";
            EditorGUILayout.HelpBox(description, MessageType.Info);

            EditorGUILayout.EndVertical();
        }

        // ─── VALVE CONTROLS ─────────────────────────────────────────

        private void DrawValveControls()
        {
            EditorGUILayout.BeginVertical(panelStyle);
            EditorGUILayout.LabelField("VALVES", subHeaderStyle);
            EditorGUILayout.Space(5);

            // Inlet Valve
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Inlet Valve:", GUILayout.Width(100));
            GUI.backgroundColor = simulation.inletValveOpen ? AccentGreen : AccentRed;
            string inletText = simulation.inletValveOpen ? "OPEN" : "CLOSED";
            if (GUILayout.Button(inletText, buttonStyle))
            {
                simulation.ToggleInletValve();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(3);

            // Outlet Valve
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Outlet Valve:", GUILayout.Width(100));
            GUI.backgroundColor = simulation.outletValveOpen ? AccentGreen : AccentRed;
            string outletText = simulation.outletValveOpen ? "OPEN" : "CLOSED";
            if (GUILayout.Button(outletText, buttonStyle))
            {
                simulation.ToggleOutletValve();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        // ─── FLOW RATES ─────────────────────────────────────────────

        private void DrawFlowRates()
        {
            EditorGUILayout.BeginVertical(panelStyle);
            EditorGUILayout.LabelField("FLOW RATES", subHeaderStyle);
            EditorGUILayout.Space(5);

            simulation.inletFlowRate = EditorGUILayout.Slider("Inlet (L/s)", simulation.inletFlowRate, 0.1f, 100f);
            simulation.outletFlowRate = EditorGUILayout.Slider("Outlet (L/s)", simulation.outletFlowRate, 0.1f, 100f);

            EditorGUILayout.EndVertical();
        }

        // ─── TANK STATUS ────────────────────────────────────────────

        private void DrawTankStatus()
        {
            EditorGUILayout.BeginVertical(panelStyle);
            EditorGUILayout.LabelField("TANK STATUS", subHeaderStyle);
            EditorGUILayout.Space(5);

            // Tank capacity setting
            simulation.tankCapacity = EditorGUILayout.FloatField("Tank Capacity (L)", simulation.tankCapacity);

            EditorGUILayout.Space(5);

            // Water Level Bar
            float levelPercent = simulation.WaterLevelPercentage / 100f;
            EditorGUILayout.LabelField($"Water Level: {simulation.CurrentWaterLevel:F1} L / {simulation.tankCapacity:F0} L ({simulation.WaterLevelPercentage:F1}%)");

            Rect levelRect = GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.box, GUILayout.Height(30));
            EditorGUI.DrawRect(levelRect, EmptyColor);

            Rect fillRect = new Rect(levelRect.x, levelRect.y, levelRect.width * levelPercent, levelRect.height);
            Color levelColor = GetLevelColor(simulation.WaterLevelPercentage);
            EditorGUI.DrawRect(fillRect, levelColor);

            // Border
            DrawRectBorder(levelRect, new Color(0.4f, 0.4f, 0.5f, 1f), 2);

            // Level text overlay
            var centerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            EditorGUI.LabelField(levelRect, $"{simulation.WaterLevelPercentage:F1}%", centerStyle);

            EditorGUILayout.EndVertical();
        }

        // ─── TEMPERATURE GAUGE ──────────────────────────────────────

        private void DrawTemperatureGauge()
        {
            EditorGUILayout.BeginVertical(panelStyle);
            EditorGUILayout.LabelField("TEMPERATURE", subHeaderStyle);
            EditorGUILayout.Space(5);

            float tempPercent = Mathf.Clamp01(simulation.CurrentTemperature / 340f);

            EditorGUILayout.LabelField($"Current Temperature: {simulation.CurrentTemperature:F1} °C");

            Rect tempRect = GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.box, GUILayout.Height(25));
            EditorGUI.DrawRect(tempRect, EmptyColor);

            // Temperature gradient bar
            Rect tempFillRect = new Rect(tempRect.x, tempRect.y, tempRect.width * tempPercent, tempRect.height);
            Color tempColor = GetTemperatureColor(simulation.CurrentTemperature);
            EditorGUI.DrawRect(tempFillRect, tempColor);
            DrawRectBorder(tempRect, new Color(0.4f, 0.4f, 0.5f, 1f), 2);

            var centerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            EditorGUI.LabelField(tempRect, $"{simulation.CurrentTemperature:F1} °C", centerStyle);

            // Gauge visualization (semicircle representation)
            EditorGUILayout.Space(5);
            DrawTemperatureNeedle(simulation.CurrentTemperature);

            EditorGUILayout.EndVertical();
        }

        private void DrawTemperatureNeedle(float temperature)
        {
            Rect gaugeRect = GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.box, GUILayout.Height(100));
            EditorGUI.DrawRect(gaugeRect, new Color(0.1f, 0.1f, 0.12f, 1f));

            // Draw gauge background arc markings
            Vector2 center = new Vector2(gaugeRect.center.x, gaugeRect.yMax - 10);
            float radius = 40f;

            // Draw scale markings
            Handles.color = new Color(0.3f, 0.8f, 0.9f, 0.8f);
            for (int i = 0; i <= 340; i += 20)
            {
                float angle = Mathf.Lerp(180f, 0f, i / 340f) * Mathf.Deg2Rad;
                Vector3 start = center + new Vector2(Mathf.Cos(angle), -Mathf.Sin(angle)) * (radius - 5);
                Vector3 end = center + new Vector2(Mathf.Cos(angle), -Mathf.Sin(angle)) * radius;
                Handles.DrawLine(start, end);
            }

            // Draw needle
            float needleAngle = Mathf.Lerp(180f, 0f, temperature / 340f) * Mathf.Deg2Rad;
            Vector3 needleEnd = center + new Vector2(Mathf.Cos(needleAngle), -Mathf.Sin(needleAngle)) * (radius - 10);
            Handles.color = Color.white;
            Handles.DrawLine((Vector3)center, needleEnd);

            // Draw center dot
            Handles.color = AccentRed;
            Handles.DrawSolidDisc(center, Vector3.forward, 3f);

            // Labels
            var labelStyle = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = Color.gray } };
            EditorGUI.LabelField(new Rect(gaugeRect.x + 10, gaugeRect.yMax - 20, 30, 15), "0°", labelStyle);
            EditorGUI.LabelField(new Rect(gaugeRect.center.x - 15, gaugeRect.y + 5, 40, 15), "170°", labelStyle);
            EditorGUI.LabelField(new Rect(gaugeRect.xMax - 40, gaugeRect.yMax - 20, 40, 15), "340°", labelStyle);

            DrawRectBorder(gaugeRect, new Color(0.3f, 0.3f, 0.4f, 1f), 1);
        }

        // ─── PRESSURE GAUGE ─────────────────────────────────────────

        private void DrawPressureGauge()
        {
            EditorGUILayout.BeginVertical(panelStyle);
            EditorGUILayout.LabelField("PRESSURE", subHeaderStyle);
            EditorGUILayout.Space(5);

            float pressurePercent = Mathf.Clamp01(simulation.CurrentPressure / simulation.overPressureThreshold);

            EditorGUILayout.LabelField($"Current Pressure: {simulation.CurrentPressure:F2} bar");

            Rect pressRect = GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.box, GUILayout.Height(25));
            EditorGUI.DrawRect(pressRect, EmptyColor);

            Rect pressFillRect = new Rect(pressRect.x, pressRect.y, pressRect.width * pressurePercent, pressRect.height);
            Color pressColor = simulation.CurrentPressure >= simulation.overPressureThreshold ? AccentRed : AccentBlue;
            EditorGUI.DrawRect(pressFillRect, pressColor);
            DrawRectBorder(pressRect, new Color(0.4f, 0.4f, 0.5f, 1f), 2);

            var centerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            EditorGUI.LabelField(pressRect, $"{simulation.CurrentPressure:F2} bar", centerStyle);

            EditorGUILayout.Space(3);
            simulation.basePressure = EditorGUILayout.Slider("Base Pressure (bar)", simulation.basePressure, 0.5f, 2f);

            EditorGUILayout.EndVertical();
        }

        // ─── THERMAL CONTROL ────────────────────────────────────────

        private void DrawThermalControl()
        {
            EditorGUILayout.BeginVertical(panelStyle);
            EditorGUILayout.LabelField("THERMAL CONTROL", subHeaderStyle);
            EditorGUILayout.Space(5);

            simulation.ambientTemperature = EditorGUILayout.Slider("Ambient Temp (°C)", simulation.ambientTemperature, -20f, 50f);

            if (simulation.scenario == SimulationScenario.HotWaterInlet_CoolingInTank)
            {
                simulation.inletWaterTemperature = EditorGUILayout.Slider("Inlet Water Temp (°C)", simulation.inletWaterTemperature, 50f, 340f);
                simulation.coolingRate = EditorGUILayout.Slider("Cooling Rate (°C/s)", simulation.coolingRate, 0.1f, 50f);
            }
            else
            {
                simulation.heatingRate = EditorGUILayout.Slider("Heating Rate (°C/s)", simulation.heatingRate, 0.1f, 50f);
            }

            EditorGUILayout.EndVertical();
        }

        // ─── WARNINGS ───────────────────────────────────────────────

        private void DrawWarnings()
        {
            EditorGUILayout.BeginVertical(panelStyle);
            EditorGUILayout.LabelField("WARNINGS", subHeaderStyle);
            EditorGUILayout.Space(5);

            TankWarnings warnings = simulation.ActiveWarnings;

            if (warnings == TankWarnings.None)
            {
                var safeStyle = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = AccentGreen } };
                EditorGUILayout.LabelField("ALL SYSTEMS NORMAL", safeStyle);
            }
            else
            {
                if ((warnings & TankWarnings.Overheat) != 0)
                {
                    DrawWarningBox($"OVERHEAT! Temperature > {simulation.overheatThreshold}°C", AccentRed);
                }
                if ((warnings & TankWarnings.Overflow) != 0)
                {
                    DrawWarningBox($"OVERFLOW! Tank level > {simulation.overflowThreshold}%", AccentYellow);
                }
                if ((warnings & TankWarnings.EmptyTank) != 0)
                {
                    DrawWarningBox($"EMPTY TANK! Level < {simulation.emptyThreshold}%", AccentYellow);
                }
                if ((warnings & TankWarnings.OverPressure) != 0)
                {
                    DrawWarningBox($"OVER PRESSURE! > {simulation.overPressureThreshold} bar", AccentRed);
                }
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Warning Thresholds", EditorStyles.miniBoldLabel);
            simulation.overheatThreshold = EditorGUILayout.Slider("Overheat (°C)", simulation.overheatThreshold, 100f, 340f);
            simulation.overflowThreshold = EditorGUILayout.Slider("Overflow (%)", simulation.overflowThreshold, 80f, 100f);
            simulation.emptyThreshold = EditorGUILayout.Slider("Empty (%)", simulation.emptyThreshold, 0f, 20f);
            simulation.overPressureThreshold = EditorGUILayout.Slider("Over Pressure (bar)", simulation.overPressureThreshold, 2f, 20f);

            EditorGUILayout.EndVertical();
        }

        private void DrawWarningBox(string message, Color color)
        {
            Rect warnRect = GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.box, GUILayout.Height(25));
            EditorGUI.DrawRect(warnRect, new Color(color.r, color.g, color.b, 0.2f));
            DrawRectBorder(warnRect, color, 2);

            var warnTextStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = color },
                fontSize = 11
            };
            EditorGUI.LabelField(warnRect, $"⚠ {message}", warnTextStyle);
        }

        // ─── UTILITIES ──────────────────────────────────────────────

        private Color GetLevelColor(float percentage)
        {
            if (percentage >= 95f) return AccentRed;
            if (percentage >= 75f) return AccentYellow;
            if (percentage >= 25f) return WaterColor;
            return new Color(0.2f, 0.4f, 0.7f, 0.8f);
        }

        private Color GetTemperatureColor(float temp)
        {
            if (temp <= 50f) return AccentBlue;
            if (temp <= 150f) return AccentGreen;
            if (temp <= 250f) return AccentOrange;
            return AccentRed;
        }

        private void DrawRectBorder(Rect rect, Color color, float thickness)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color); // Top
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color); // Bottom
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color); // Left
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color); // Right
        }
    }
}
