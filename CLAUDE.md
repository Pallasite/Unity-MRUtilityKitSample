# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Unity sample project demonstrating **Meta MR Utility Kit (MRUK)** APIs for Mixed Reality on Meta Quest devices. Contains 14 standalone sample scenes showcasing spatial awareness features (scene queries, passthrough, nav mesh, destructible environments, QR detection, etc.).

- **Unity Version:** 6000.3.11f1 (Unity 6)
- **Render Pipeline:** Universal Render Pipeline (URP) 17.3.0
- **Target Platform:** Android (Meta Quest 2/Pro/3/3S), ARM64, Vulkan, IL2CPP
- **Key SDKs:** `com.meta.xr.mrutilitykit` 85.0.0, `com.meta.xr.sdk.core` 85.0.0

## Build Commands

- **Unity Menu Build:** Meta > Samples > Build MRUK Samples (outputs `MRUKSamples.apk`)
- **Build script:** `Assets/Editor/BuildMRUKSamples.cs` — configures Android settings and builds all scenes

There is no CLI build pipeline or CI/CD configured. Builds are done through the Unity Editor.

## Tests

- **Location:** `Assets/Tests/TestsExamples.cs`
- **Framework:** Unity Test Runner (PlayMode), extends `MRUKTestBase` from the MRUK SDK
- **Namespace:** `Meta.XR.MRUtilityKitSamples.Tests`
- **Run via:** Unity Editor > Window > General > Test Runner > PlayMode tab
- Tests use simulated room data from MRUK SDK prefabs/JSON (no device required)

## Code Architecture

All sample code lives under `Assets/MRUKSamples/`, with each subdirectory being a self-contained sample:

```
Assets/MRUKSamples/
├── Basic/              # Core MRUK + EffectMesh visualization
├── BouncingBall/       # Physics interaction with scene anchors
├── DestructibleMesh/   # Global mesh segmentation
├── EnvironmentPanelPlacement/  # Raycast-based panel attachment
├── FloorZone/          # Floor zone detection (7 scripts)
├── HiFiScene/          # Multi-floor/slanted ceiling layouts
├── KeyboardTracker/    # Generic keyboard detection
├── MultiSpawn/         # Object spawning at scene locations
├── NavMesh/            # Nav mesh from scene data
├── PassthroughRelighting/  # Virtual lighting on scene objects
├── QRCodeDetection/    # QR code tracking
├── SceneDecorator/     # Environment decoration
├── SharedAssets/       # Common UI (IntroPanel) and utilities
├── SpaceMap/           # Room texture/gradient mapping
├── StartScene/         # Main menu scene
└── VirtualHome/        # Room reskinning with furniture spawner
```

Each sample has its own scene (`.unity`), scripts, materials, and prefabs. `StartScene` is the main menu that launches other samples.

The single editor script (`Assets/Editor/BuildMRUKSamples.cs`) handles APK build configuration. Scripts use the `[MetaCodeSample]` attribute for Meta's documentation system.

### KinesResearch Sample (Ported from Kines-MR-Meta-All)

`Assets/MRUKSamples/KinesResearch/` contains motor adaptation research scripts ported from a Unity 2022 / SDK v69 project. These scripts use **snake_case** naming (not the `.editorconfig` convention) — preserve this style when editing them.

**Core loop:** CSV file defines trials -> `CSVLoader` parses -> `ObstacleManager` runs distance-based triggering -> `IObstacleBehavior` moves obstacle -> auto-reset advances to next trial.

**Key classes:**
- `ObstacleManager` — Central orchestrator: distance triggers, state machine (armed/moved/reset), delegates to `IObstacleBehavior`
- `CSVLoader` + `TrialCondition` — Loads `trial_conditions.csv` from `Application.persistentDataPath`, manages trial progression
- `IObstacleBehavior` / `DefaultObstacleBehavior` / `DogObstacleBehavior` — Strategy pattern for obstacle movement
- `AnchorManager` — `OVRSpatialAnchor` + `SpatialAnchorCoreBuildingBlock` for world-locking obstacles
- `OcclusionSwapper` — Distance-based material swap with static registry for bulk control
- `FinesseTouch` / `WorldTouch` — Precision nudging (cm/mm) for obstacle calibration
- `Aligner` — Calibration cube-based path alignment
- `TrialCounter` — Color-coded trial status UI (green=active, red=inactive, magenta=missing)

**Scene hierarchy pattern:**
```
obstacle_anchor (parent, OVRSpatialAnchor, world-locked)
  └── obstacle (child, visuals — reset returns to local 0,0,0)
```

**Scene setup required in Unity Editor:** Building Blocks (Camera Rig, Spatial Anchor Core), MRUK component, Manager object with all scripts wired, calibration cubes, experimenter UI panel. See source scene `Kines-MR-Meta-All/Assets/_Scene/03-10-26 Anchor Update - Kill the Crystals.unity` for reference inspector values.

## C# Code Style

Defined in `.editorconfig`:
- 4-space indentation, LF line endings, UTF-8
- Braces on new lines (`csharp_new_line_before_open_brace = all`)
- Max line length: 200 characters
- **Naming:** PascalCase for public types, `_camelCase` for private fields, `s_camelCase` for static private fields, camelCase for local functions
- Braces required (`csharp_prefer_braces = true`)

## Android Configuration

- **Package:** `com.meta.mruksample`
- **Manifest:** `Assets/Plugins/Android/AndroidManifest.xml`
- **Required features:** Passthrough, Scene API, Anchor API, Boundary Visibility
- **Oculus config:** `Assets/Oculus/OculusProjectConfig.asset`
