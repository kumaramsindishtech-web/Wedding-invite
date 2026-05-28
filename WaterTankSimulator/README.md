# Water Tank Simulator - Unity Package

A realistic water tank simulation addon for Unity 2021.3+ with full Editor UI for visualization and training purposes.

## Features

- **Two Simulation Scenarios:**
  - Hot Water Inlet → Cooling in Tank
  - Cold Water Inlet → Heating in Tank
- **Binary Valve Controls** (Open/Closed) for Inlet and Outlet
- **Configurable Flow Rates** (liters/second)
- **Temperature Simulation** with configurable cooling/heating rates
- **Pressure Simulation** based on temperature and water level
- **Warning System:** Overheat, Overflow, Empty Tank, Over Pressure
- **Custom Editor Window** with visual gauges and controls
- **Realistic Water Shader** with caustics, fresnel, waves, and heat distortion
- **Pipe Flow Visualization** with UV scrolling and particle support
- **Temperature Gauge Needle** with spring-damper physics
- **Works in Edit Mode** - no need to enter Play mode

## Installation

### Via Package Manager (Local):
1. Copy the `WaterTankSimulator` folder to your Unity project
2. In Unity, go to **Window > Package Manager**
3. Click the **+** button → **Add package from disk...**
4. Navigate to and select the `package.json` file inside the `WaterTankSimulator` folder

### Via Git URL:
1. In Unity Package Manager, click **+** → **Add package from git URL...**
2. Enter: `https://github.com/kumaramsindishtech-web/Wedding-invite.git?path=WaterTankSimulator`

## Setup Guide

### 1. Add Simulation Component
- Create an empty GameObject or select your imported tank model
- Add Component → **SindishTech > Water Tank Simulation**

### 2. Setup Water Visual (Tank)
- Select your Tank mesh object
- Add Component → **SindishTech > Water Level Controller**
- Assign the simulation reference
- Apply the `SindishTech/WaterTankRealistic` shader to a material on the tank

### 3. Setup Temperature Gauge
- Select the "needle" object from your imported model
- Add Component → **SindishTech > Temperature Needle Controller**
- Assign the simulation reference
- Configure min/max angles to match your gauge model

### 4. Setup Valves
- Select "Valve_Inlet" object
- Add Component → **SindishTech > Valve Controller**
- Set Valve Type to "Inlet"
- Repeat for "Valve_Outlet" with type "Outlet"

### 5. Setup Pipe Flow Visuals
- Select "Inlet" pipe object
- Add Component → **SindishTech > Pipe Flow Visual**
- Set Pipe Type to "Inlet"
- Repeat for "Outlet" pipe

### 6. Open Editor Dashboard
- Go to menu: **SindishTech > Water Tank Simulator**
- Or click "Open Simulation Dashboard" in the inspector

## Editor Window Controls

| Section | Controls |
|---------|----------|
| Simulation | Start/Stop/Reset |
| Scenario | Hot Inlet + Cooling / Cold Inlet + Heating |
| Valves | Toggle Inlet/Outlet (Binary) |
| Flow Rates | Inlet/Outlet L/s sliders |
| Tank Status | Visual water level bar |
| Temperature | Gauge with animated needle |
| Pressure | Pressure bar with threshold |
| Thermal | Ambient temp, cooling/heating rate |
| Warnings | Overheat, Overflow, Empty, Pressure |

## Blender Model Hierarchy

This package is designed to work with models containing:
- `Tank` - Main vessel
- `Inlet` - Inlet pipe
- `Outlet` - Outlet pipe  
- `Valve_Inlet` - Inlet valve
- `Valve_Outlet` - Outlet valve
- `Temp Indicator` - Temperature gauge body
- `meter` - Gauge face
- `needle` - Gauge needle

## Requirements

- Unity 2021.3 or later
- Built-in Render Pipeline (shader compatible)
