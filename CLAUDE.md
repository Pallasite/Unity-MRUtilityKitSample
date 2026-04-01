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
