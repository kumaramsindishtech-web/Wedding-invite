using UnityEngine;
using UnityEditor;

namespace SindishTech.WaterTankSimulator.Editor
{
    /// <summary>
    /// Professional Editor Window for the Water Tank Simulation.
    /// Clean, modern UI with visual gauges and controls.
    /// </summary>
    public class WaterTankEditorWindow : EditorWindow
    {
        private WaterTankSimulation simulation;
        private Vector2 scrollPosition;

        // Colors - Modern Dark Theme
        private static readonly Color BgDark = new Color(0.12f, 0.12f, 0.14f, 1f);
        private static readonly Color BgPanel = new Color(0.18f, 0.18f, 0.20f, 1f);
        private static readonly Color BgLight = new Color(0.22f, 0.22f, 0.25f, 1f);
        private static readonly Color BorderColor = new Color(0.3f, 0.3f, 0.35f, 1f);
        
        private static readonly Color Blue = new Color(0.25f, 0.55f, 0.9f, 1f);
        private static readonly Color Green = new Color(0.2f, 0.75f, 0.4f, 1f);
        private static readonly Color Red = new Color(0.9f, 0.25f, 0.25f, 1f);
        private static readonly Color Yellow = new Color(0.95f, 0.75f, 0.1f, 1f);
        private static readonly Color Orange = new Color(0.95f, 0.5f, 0.15f, 1f);
        private static readonly Color Cyan = new Color(0.3f, 0.85f, 0.9f, 1f);
        private static readonly Color TextWhite = new Color(0.9f, 0.9f, 0.92f, 1f);
        private static readonly Color TextGray = new Color(0.6f, 0.6f, 0.65f, 1f);

        // Cached styles
        private GUIStyle titleStyle;
        private GUIStyle sectionStyle;
        private GUIStyle labelStyle;
        private GUIStyle valueStyle;
        private GUIStyle smallLabelStyle;
        private bool stylesInitialized;

        [MenuItem("SindishTech/Water Tank Simulator")]
        public static void ShowWindow()
        {
            var window = GetWindow<WaterTankEditorWindow>("Tank Simulator");
            window.minSize = new Vector2(320, 600);
            window.maxSize = new Vector2(400, 900);
            window.Show();
        }

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (simulation != null && simulation.IsSimulationRunning)
                Repaint();
        }

        private void InitStyles()
        {
            if (stylesInitialized) return;

            titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = Cyan },
                padding = new RectOffset(0, 0, 2, 2)
            };

            sectionStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = TextGray }
            };

            labelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 10,
                normal = { textColor = TextGray }
            };

            valueStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = TextWhite }
            };

            smallLabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 9,
                normal = { textColor = TextGray }
            };

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitStyles();

            // Find simulation
            if (simulation == null)
                simulation = FindObjectOfType<WaterTankSimulation>();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            GUILayout.Space(8);

            if (simulation == null)
            {
                DrawNoSimulation();
            }
            else
            {
                DrawControlPanel();
                GUILayout.Space(6);
                DrawStatusPanel();
                GUILayout.Space(6);
                DrawValvesPanel();
                GUILayout.Space(6);
                DrawGaugesPanel();
                GUILayout.Space(6);
                DrawSettingsPanel();
                GUILayout.Space(6);
                DrawWarningsPanel();
            }

            GUILayout.Space(8);
            EditorGUILayout.EndScrollView();

            if (GUI.changed && simulation != null)
                EditorUtility.SetDirty(simulation);
        }

        private void DrawNoSimulation()
        {
            DrawPanel("NO SIMULATION FOUND", () =>
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField("Add WaterTankSimulation to a GameObject", labelStyle);
                GUILayout.Space(10);
                
                if (DrawButton("Create Simulation", Green, 32))
                {
                    var go = new GameObject("WaterTankSimulation");
                    go.AddComponent<WaterTankSimulation>();
                    Selection.activeGameObject = go;
                    simulation = go.GetComponent<WaterTankSimulation>();
                }
            });
        }

        // ═══════════════════════════════════════════════════════════
        // CONTROL PANEL
        // ═══════════════════════════════════════════════════════════
        private void DrawControlPanel()
        {
            DrawPanel("SIMULATION", () =>
            {
                EditorGUILayout.BeginHorizontal();
                
                // Start/Stop
                Color btnColor = simulation.IsSimulationRunning ? Red : Green;
                string btnText = simulation.IsSimulationRunning ? "■ STOP" : "▶ START";
                if (DrawButton(btnText, btnColor, 28))
                {
                    if (simulation.IsSimulationRunning)
                        simulation.StopSimulation();
                    else
                        simulation.StartSimulation();
                }

                GUILayout.Space(4);

                // Reset
                if (DrawButton("↺ RESET", Orange, 28))
                {
                    simulation.ResetSimulation();
                }

                EditorGUILayout.EndHorizontal();

                GUILayout.Space(4);

                // Status indicator
                Rect statusRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(20));
                Color statusColor = simulation.IsSimulationRunning ? Green : TextGray;
                string statusText = simulation.IsSimulationRunning ? "● RUNNING" : "○ STOPPED";
                
                var statusStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 10,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = statusColor }
                };
                EditorGUI.LabelField(statusRect, statusText, statusStyle);
            });
        }

        // ═══════════════════════════════════════════════════════════
        // STATUS PANEL - Tank Level
        // ═══════════════════════════════════════════════════════════
        private void DrawStatusPanel()
        {
            DrawPanel("TANK STATUS", () =>
            {
                // Capacity
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Capacity", labelStyle, GUILayout.Width(70));
                simulation.tankCapacity = EditorGUILayout.FloatField(simulation.tankCapacity, GUILayout.Width(60));
                EditorGUILayout.LabelField("L", smallLabelStyle, GUILayout.Width(20));
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(8);

                // Water Level Visual
                float fillPercent = simulation.WaterLevelPercentage / 100f;
                DrawProgressBar(
                    $"{simulation.CurrentWaterLevel:F1} L ({simulation.WaterLevelPercentage:F1}%)",
                    fillPercent,
                    GetWaterColor(simulation.WaterLevelPercentage),
                    35
                );
            });
        }

        // ═══════════════════════════════════════════════════════════
        // VALVES PANEL
        // ═══════════════════════════════════════════════════════════
        private void DrawValvesPanel()
        {
            DrawPanel("VALVES", () =>
            {
                // Inlet
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Inlet", labelStyle, GUILayout.Width(50));
                
                Color inletColor = simulation.inletValveOpen ? Green : Red;
                string inletText = simulation.inletValveOpen ? "OPEN" : "CLOSED";
                if (DrawButton(inletText, inletColor, 24, 80))
                    simulation.ToggleInletValve();

                GUILayout.Space(8);
                simulation.inletFlowRate = EditorGUILayout.Slider(simulation.inletFlowRate, 0.1f, 100f);
                EditorGUILayout.LabelField("L/s", smallLabelStyle, GUILayout.Width(25));
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(4);

                // Outlet
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Outlet", labelStyle, GUILayout.Width(50));
                
                Color outletColor = simulation.outletValveOpen ? Green : Red;
                string outletText = simulation.outletValveOpen ? "OPEN" : "CLOSED";
                if (DrawButton(outletText, outletColor, 24, 80))
                    simulation.ToggleOutletValve();

                GUILayout.Space(8);
                simulation.outletFlowRate = EditorGUILayout.Slider(simulation.outletFlowRate, 0.1f, 100f);
                EditorGUILayout.LabelField("L/s", smallLabelStyle, GUILayout.Width(25));
                EditorGUILayout.EndHorizontal();
            });
        }

        // ═══════════════════════════════════════════════════════════
        // GAUGES PANEL - Temperature & Pressure
        // ═══════════════════════════════════════════════════════════
        private void DrawGaugesPanel()
        {
            DrawPanel("GAUGES", () =>
            {
                // Temperature
                float tempPercent = Mathf.Clamp01(simulation.CurrentTemperature / 340f);
                DrawProgressBar(
                    $"TEMP: {simulation.CurrentTemperature:F1} °C",
                    tempPercent,
                    GetTempColor(simulation.CurrentTemperature),
                    24
                );

                GUILayout.Space(6);

                // Pressure
                float pressPercent = Mathf.Clamp01(simulation.CurrentPressure / 10f);
                DrawProgressBar(
                    $"PRESS: {simulation.CurrentPressure:F2} bar",
                    pressPercent,
                    simulation.CurrentPressure >= simulation.overPressureThreshold ? Red : Blue,
                    24
                );
            });
        }

        // ═══════════════════════════════════════════════════════════
        // SETTINGS PANEL
        // ═══════════════════════════════════════════════════════════
        private void DrawSettingsPanel()
        {
            DrawPanel("THERMAL SETTINGS", () =>
            {
                // Scenario
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Mode", labelStyle, GUILayout.Width(80));
                simulation.scenario = (SimulationScenario)EditorGUILayout.EnumPopup(simulation.scenario);
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(4);

                // Ambient
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Ambient", labelStyle, GUILayout.Width(80));
                simulation.ambientTemperature = EditorGUILayout.Slider(simulation.ambientTemperature, -20f, 50f);
                EditorGUILayout.LabelField("°C", smallLabelStyle, GUILayout.Width(20));
                EditorGUILayout.EndHorizontal();

                if (simulation.scenario == SimulationScenario.HotWaterInlet_CoolingInTank)
                {
                    // Inlet temp
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Inlet Temp", labelStyle, GUILayout.Width(80));
                    simulation.inletWaterTemperature = EditorGUILayout.Slider(simulation.inletWaterTemperature, 50f, 340f);
                    EditorGUILayout.LabelField("°C", smallLabelStyle, GUILayout.Width(20));
                    EditorGUILayout.EndHorizontal();

                    // Cooling
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Cooling", labelStyle, GUILayout.Width(80));
                    simulation.coolingRate = EditorGUILayout.Slider(simulation.coolingRate, 0.1f, 50f);
                    EditorGUILayout.LabelField("°C/s", smallLabelStyle, GUILayout.Width(30));
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    // Heating
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Heating", labelStyle, GUILayout.Width(80));
                    simulation.heatingRate = EditorGUILayout.Slider(simulation.heatingRate, 0.1f, 50f);
                    EditorGUILayout.LabelField("°C/s", smallLabelStyle, GUILayout.Width(30));
                    EditorGUILayout.EndHorizontal();
                }
            });
        }

        // ═══════════════════════════════════════════════════════════
        // WARNINGS PANEL
        // ═══════════════════════════════════════════════════════════
        private void DrawWarningsPanel()
        {
            DrawPanel("ALERTS", () =>
            {
                TankWarnings warnings = simulation.ActiveWarnings;

                if (warnings == TankWarnings.None)
                {
                    DrawAlertBox("✓ ALL SYSTEMS NORMAL", Green);
                }
                else
                {
                    if ((warnings & TankWarnings.Overheat) != 0)
                        DrawAlertBox("⚠ OVERHEAT", Red);
                    if ((warnings & TankWarnings.Overflow) != 0)
                        DrawAlertBox("⚠ OVERFLOW", Yellow);
                    if ((warnings & TankWarnings.EmptyTank) != 0)
                        DrawAlertBox("⚠ EMPTY", Yellow);
                    if ((warnings & TankWarnings.OverPressure) != 0)
                        DrawAlertBox("⚠ HIGH PRESSURE", Red);
                }
            });
        }

        // ═══════════════════════════════════════════════════════════
        // UI HELPERS
        // ═══════════════════════════════════════════════════════════

        private void DrawPanel(string title, System.Action content)
        {
            // Reserve space first to measure, then draw
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            GUILayout.Space(2);
            
            // Title with cyan color
            var titleLabelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 11,
                normal = { textColor = Cyan }
            };
            EditorGUILayout.LabelField(title, titleLabelStyle);
            
            GUILayout.Space(4);

            // Content
            content?.Invoke();

            GUILayout.Space(4);
            
            EditorGUILayout.EndVertical();
        }

        private bool DrawButton(string text, Color color, float height, float width = 0)
        {
            GUI.backgroundColor = color;
            
            var style = new GUIStyle(GUI.skin.button)
            {
                fontSize = 10,
                fontStyle = FontStyle.Bold,
                fixedHeight = height,
                normal = { textColor = Color.white },
                hover = { textColor = Color.white },
                active = { textColor = Color.white }
            };

            bool result;
            if (width > 0)
                result = GUILayout.Button(text, style, GUILayout.Width(width));
            else
                result = GUILayout.Button(text, style);

            GUI.backgroundColor = Color.white;
            return result;
        }

        private void DrawProgressBar(string label, float value, Color fillColor, float height)
        {
            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(height));

            // Background
            EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.18f, 1f));

            // Fill
            Rect fillRect = new Rect(rect.x + 1, rect.y + 1, (rect.width - 2) * Mathf.Clamp01(value), rect.height - 2);
            EditorGUI.DrawRect(fillRect, fillColor);

            // Border
            DrawBorder(rect, new Color(0.3f, 0.3f, 0.35f, 1f), 1);

            // Label
            var barLabelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };
            EditorGUI.LabelField(rect, label, barLabelStyle);
        }

        private void DrawAlertBox(string message, Color color)
        {
            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(24));

            // Background
            Color bgColor = new Color(color.r, color.g, color.b, 0.2f);
            EditorGUI.DrawRect(rect, bgColor);
            DrawBorder(rect, color, 1);

            // Text
            var alertStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = color }
            };
            EditorGUI.LabelField(rect, message, alertStyle);
        }

        private void DrawBorder(Rect rect, Color color, float thickness)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        private Color GetWaterColor(float percent)
        {
            if (percent >= 95f) return Red;
            if (percent >= 75f) return Yellow;
            return Blue;
        }

        private Color GetTempColor(float temp)
        {
            if (temp >= 250f) return Red;
            if (temp >= 150f) return Orange;
            if (temp >= 50f) return Yellow;
            return Cyan;
        }
    }
}
