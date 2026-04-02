using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Grab;

/// <summary>
/// Editor script that builds the full Kines Research scene hierarchy from a menu item.
/// Menu: Meta > Samples > Build Kines Research Scene
///
/// Creates: Manager (9 components), obstacle hierarchy, 4 calibration cubes,
/// NudgeManager panel (12 poke buttons), CommandStaff panel (8 poke buttons + 3 indicators),
/// and trial status displays. All cross-references are wired automatically.
///
/// ISDK components (PokeInteractable, HandGrabInteractable, etc.) are added for
/// hand-tracked poke and grab interaction support.
/// </summary>
public static class KinesResearchSceneBuilder
{
    // Layout constants
    private const float ButtonWidth = 0.12f;
    private const float ButtonHeight = 0.04f;
    private const float ButtonSpacingX = 0.13f;
    private const float ButtonSpacingY = 0.045f;

    [MenuItem("Meta/Samples/Build Kines Research Scene")]
    public static void BuildScene()
    {
        EnsureTagExists("TrialNumber");

        // Check for existing objects to avoid duplicates
        if (GameObject.Find("KinesResearch_Manager") != null)
        {
            if (!EditorUtility.DisplayDialog(
                "Kines Research Scene Builder",
                "Scene already contains Kines Research objects (KinesResearch_Manager found). Delete existing and rebuild?",
                "Rebuild", "Cancel"))
            {
                return;
            }
            CleanupExisting();
        }

        // Build everything
        var obstacleHierarchy = CreateObstacleHierarchy();
        var obstacleAnchorHolder = obstacleHierarchy.anchorHolder;
        var obstacles = obstacleHierarchy.obstacles;

        var calibrationCubes = CreateCalibrationCubes();
        var trialStatus = CreateTrialStatusDisplay();
        var manager = CreateManager(obstacleAnchorHolder, obstacles, calibrationCubes, trialStatus);

        // Get component references from manager for button wiring
        var obstacleManager = manager.GetComponent<ObstacleManager>();
        var csvLoader = manager.GetComponent<CSVLoader>();
        var finesseTouch = manager.GetComponent<FinesseTouch>();
        var anchorManager = manager.GetComponent<AnchorManager>();

        var nudgePanel = CreateNudgeManagerPanel(finesseTouch, obstacleManager);
        var commandStaff = CreateCommandStaffPanel(csvLoader, obstacleManager, anchorManager);

        // Wire the staff_of_science reference
        obstacleManager.staff_of_science = commandStaff;

        // Wire indicator references from command staff children
        obstacleManager.active_indicator = commandStaff.transform.Find("active_indicator")?.gameObject;
        obstacleManager.auto_indicator = commandStaff.transform.Find("auto_indicator")?.gameObject;
        obstacleManager.auto_and_active_indicator = commandStaff.transform.Find("auto_and_active_indicator")?.gameObject;

        // Mark scene dirty
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("Kines Research Scene built successfully. Manual steps remaining:\n" +
            "1. Add Building Blocks (Camera Rig, Spatial Anchor Core)\n" +
            "2. Wire anchor_core_building_block on AnchorManager\n" +
            "3. Import obstacle prefabs -> assign to ObstacleManager.obstacle_visuals\n" +
            "4. Import depth occlusion materials\n" +
            "5. Add scene to Build Settings");
    }

    private static void CleanupExisting()
    {
        var names = new[]
        {
            "KinesResearch_Manager", "Obstacle Anchor Holder",
            "CalibrationCubeStartOne", "CalibrationCubeStartTwo",
            "CalibrationCubeEndOne", "CalibrationCubeEndTwo",
            "NudgeManager", "CommandStaff",
            "TrialStatusDisplay", "TrialLoadingStatus"
        };
        foreach (var name in names)
        {
            var go = GameObject.Find(name);
            if (go != null)
            {
                Undo.DestroyObjectImmediate(go);
            }
        }
    }

    // ==================== Manager ====================

    private static GameObject CreateManager(
        GameObject obstacleAnchorHolder,
        GameObject obstacles,
        CalibrationCubeRefs cubes,
        TrialStatusRefs trialStatus)
    {
        var manager = new GameObject("KinesResearch_Manager");
        manager.transform.position = new Vector3(0f, 2.136f, 0f);
        Undo.RegisterCreatedObjectUndo(manager, "Create KinesResearch Manager");

        // Create tuner children
        var cmHeight = CreateChild(manager, "CentimeterHeight");
        var mmHeight = CreateChild(manager, "MillimeterHeight");
        var cmWidth = CreateChild(manager, "CentimeterWidth");
        var mmWidth = CreateChild(manager, "MillimeterWidth");

        // Add components
        var obstacleManager = Undo.AddComponent<ObstacleManager>(manager);
        obstacleManager.trigger_distance = 0.68f;
        obstacleManager.move_distance = 0.34f;
        obstacleManager.reset_distance = 2f;
        obstacleManager.time_buffer = 2f;
        obstacleManager.obstacle = obstacles;
        obstacleManager.obstacle_anchor = obstacleAnchorHolder;
        obstacleManager.obstacle_visuals = new List<GameObject>();
        obstacleManager.setup_visual = new List<GameObject>();

        var csvLoader = Undo.AddComponent<CSVLoader>(manager);
        csvLoader.trial_loading_status = trialStatus.loadingStatus;

        var trialCounter = Undo.AddComponent<TrialCounter>(manager);
        trialCounter.csv_loader = csvLoader;
        trialCounter.obstacle_manager = obstacleManager;

        obstacleManager.csv_loader = csvLoader;

        var anchorManager = Undo.AddComponent<AnchorManager>(manager);
        anchorManager.target_object = obstacleAnchorHolder;

        var finesseTouch = Undo.AddComponent<FinesseTouch>(manager);
        finesseTouch.obstacles = new List<GameObject> { obstacleAnchorHolder };
        finesseTouch.centimeter_height = cmHeight;
        finesseTouch.milimeter_height = mmHeight;
        finesseTouch.centimeter_width = cmWidth;
        finesseTouch.milimeter_width = mmWidth;

        var worldTouch = Undo.AddComponent<WorldTouch>(manager);
        worldTouch.obstacles = obstacleAnchorHolder;

        var aligner = Undo.AddComponent<Aligner>(manager);
        aligner.calibration_cube_start_one = cubes.startOne;
        aligner.calibration_cube_start_two = cubes.startTwo;
        aligner.calibration_cube_end_one = cubes.endOne;
        aligner.calibration_cube_end_two = cubes.endTwo;
        aligner.obstacles = new List<GameObject> { obstacleAnchorHolder };

        var summon = Undo.AddComponent<Summon>(manager);
        summon.summon_attractor = manager;
        summon.objects_to_summon = new List<GameObject>();

        Undo.AddComponent<ShowtimeConcealer>(manager);

        return manager;
    }

    // ==================== Obstacle Hierarchy ====================

    private struct ObstacleHierarchyRefs
    {
        public GameObject anchorHolder;
        public GameObject obstacles;
    }

    private static ObstacleHierarchyRefs CreateObstacleHierarchy()
    {
        var anchorHolder = new GameObject("Obstacle Anchor Holder");
        Undo.RegisterCreatedObjectUndo(anchorHolder, "Create Obstacle Anchor Holder");

        var obstacles = CreateChild(anchorHolder, "Obstacles");

        return new ObstacleHierarchyRefs
        {
            anchorHolder = anchorHolder,
            obstacles = obstacles
        };
    }

    // ==================== Calibration Cubes ====================

    private struct CalibrationCubeRefs
    {
        public GameObject startOne, startTwo, endOne, endTwo;
    }

    private static CalibrationCubeRefs CreateCalibrationCubes()
    {
        var refs = new CalibrationCubeRefs
        {
            startOne = CreateCalibrationCube("CalibrationCubeStartOne", new Vector3(0f, 0.5f, -2f)),
            startTwo = CreateCalibrationCube("CalibrationCubeStartTwo", new Vector3(1f, 0.5f, -2f)),
            endOne = CreateCalibrationCube("CalibrationCubeEndOne", new Vector3(0f, 0.5f, 2f)),
            endTwo = CreateCalibrationCube("CalibrationCubeEndTwo", new Vector3(1f, 0.5f, 2f)),
        };
        return refs;
    }

    private static GameObject CreateCalibrationCube(string name, Vector3 position)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = Vector3.one * 0.05f;

        var renderer = cube.GetComponent<MeshRenderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = Color.yellow;
        renderer.sharedMaterial = mat;

        MakeGrabbable(cube, includeRayGrab: false, includeDistanceGrab: false);
        Undo.RegisterCreatedObjectUndo(cube, "Create " + name);
        return cube;
    }

    // ==================== Trial Status Display ====================

    private struct TrialStatusRefs
    {
        public TextMeshPro trialNumber;
        public TextMeshPro loadingStatus;
    }

    private static TrialStatusRefs CreateTrialStatusDisplay()
    {
        var trialDisplay = new GameObject("TrialStatusDisplay");
        trialDisplay.transform.position = new Vector3(0f, 1.8f, 0.5f);
        Undo.RegisterCreatedObjectUndo(trialDisplay, "Create Trial Status Display");

        var trialNumberTMP = Undo.AddComponent<TextMeshPro>(trialDisplay);
        trialNumberTMP.text = "0";
        trialNumberTMP.fontSize = 36;
        trialNumberTMP.alignment = TextAlignmentOptions.Center;
        trialNumberTMP.rectTransform.sizeDelta = new Vector2(1f, 0.5f);
        trialDisplay.tag = "TrialNumber";

        var loadingStatusGO = new GameObject("TrialLoadingStatus");
        loadingStatusGO.transform.SetParent(trialDisplay.transform);
        loadingStatusGO.transform.localPosition = new Vector3(0f, -0.3f, 0f);
        Undo.RegisterCreatedObjectUndo(loadingStatusGO, "Create Trial Loading Status");

        var loadingTMP = Undo.AddComponent<TextMeshPro>(loadingStatusGO);
        loadingTMP.text = "No CSV loaded";
        loadingTMP.fontSize = 18;
        loadingTMP.alignment = TextAlignmentOptions.Center;
        loadingTMP.rectTransform.sizeDelta = new Vector2(2f, 0.3f);

        return new TrialStatusRefs
        {
            trialNumber = trialNumberTMP,
            loadingStatus = loadingTMP
        };
    }

    // ==================== Poke Button ====================

    /// <summary>
    /// Creates a poke button with full ISDK component stack:
    /// Root (PokeInteractable, InteractableUnityEventWrapper, ButtonClickRelay)
    ///   Model/ Surface/ (PlaneSurface, ClippedPlaneSurface, BoundsClipper)
    ///   Visuals/ ButtonVisual/ (quad + text label)
    ///            ButtonBack/ (backdrop quad)
    /// </summary>
    private static GameObject CreatePokeButton(string label, float width = ButtonWidth, float height = ButtonHeight)
    {
        var root = new GameObject("Btn_" + label);

        // ---- Model / Surface child (for ISDK poke surface) ----
        var model = CreateChild(root, "Model");
        var surface = CreateChild(model, "Surface");
        surface.transform.localScale = new Vector3(2f, 1f, 0.001f);

        // PlaneSurface — facing forward so poke works from the front
        var planeSurface = surface.AddComponent<PlaneSurface>();
        planeSurface.Facing = PlaneSurface.NormalFacing.Forward;

        // BoundsClipper — defines the rectangular interactive area
        var boundsClipper = surface.AddComponent<BoundsClipper>();
        boundsClipper.Size = new Vector3(width, height, 0.1f);
        boundsClipper.Position = Vector3.zero;

        // ClippedPlaneSurface — wires PlaneSurface + BoundsClipper
        var clippedSurface = surface.AddComponent<ClippedPlaneSurface>();
        SetSerializedField(clippedSurface, "_planeSurface", planeSurface);
        SetSerializedInterfaceListField(clippedSurface, "_clippers", new Object[] { boundsClipper });

        // ---- PokeInteractable on root ----
        var pokeInteractable = root.AddComponent<PokeInteractable>();
        SetSerializedInterfaceField(pokeInteractable, "_surfacePatch", clippedSurface);
        SetSerializedField(pokeInteractable, "_enterHoverNormal", 0.065f);
        SetSerializedField(pokeInteractable, "_exitHoverNormal", 0.08f);
        SetSerializedField(pokeInteractable, "_cancelSelectNormal", 0.2f);

        // MinThresholds config
        var so = new SerializedObject(pokeInteractable);
        var minThreshProp = so.FindProperty("_minThresholds");
        if (minThreshProp != null)
        {
            var enabledProp = minThreshProp.FindPropertyRelative("Enabled");
            var minNormalProp = minThreshProp.FindPropertyRelative("MinNormal");
            if (enabledProp != null) enabledProp.boolValue = true;
            if (minNormalProp != null) minNormalProp.floatValue = 0.015f;
        }
        so.ApplyModifiedPropertiesWithoutUndo();

        // ---- InteractableUnityEventWrapper — wires WhenSelect to ButtonClickRelay.Invoke ----
        var eventWrapper = root.AddComponent<InteractableUnityEventWrapper>();
        SetSerializedInterfaceField(eventWrapper, "_interactableView", pokeInteractable);

        // ---- ButtonClickRelay for reflection-based method invocation ----
        var relay = root.AddComponent<ButtonClickRelay>();
        // Target and method will be set by WireButtonEvent after creation

        // Wire WhenSelect -> relay.Invoke() + relay.FlashButton()
        WireUnityEventPersistent(eventWrapper, "_whenSelect", relay, "Invoke");
        WireUnityEventPersistent(eventWrapper, "_whenSelect", relay, "FlashButton");

        // ---- Visuals ----
        var visuals = CreateChild(root, "Visuals");

        // Button visual (quad)
        var buttonVisual = GameObject.CreatePrimitive(PrimitiveType.Quad);
        buttonVisual.name = "ButtonVisual";
        buttonVisual.transform.SetParent(visuals.transform, false);
        buttonVisual.transform.localPosition = Vector3.zero;
        buttonVisual.transform.localScale = new Vector3(width, height, 1f);

        // Remove default mesh collider from quad
        var quadCollider = buttonVisual.GetComponent<MeshCollider>();
        if (quadCollider != null)
        {
            Object.DestroyImmediate(quadCollider);
        }

        // Material
        var renderer = buttonVisual.GetComponent<MeshRenderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.2f, 0.2f, 0.3f, 1f);
        renderer.sharedMaterial = mat;

        // Text label
        var textGO = new GameObject("Label");
        textGO.transform.SetParent(buttonVisual.transform, false);
        textGO.transform.localPosition = new Vector3(0f, 0f, -0.01f);
        textGO.transform.localScale = new Vector3(1f / width, 1f / height, 1f);
        var tmp = textGO.AddComponent<TextMeshPro>();
        tmp.text = label;
        tmp.fontSize = 2;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.rectTransform.sizeDelta = new Vector2(width, height);
        tmp.color = Color.white;

        // Backdrop
        var backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
        backdrop.name = "ButtonBack";
        backdrop.transform.SetParent(visuals.transform, false);
        backdrop.transform.localPosition = new Vector3(0f, 0f, 0.001f);
        backdrop.transform.localScale = new Vector3(width + 0.005f, height + 0.005f, 1f);
        var backCollider = backdrop.GetComponent<MeshCollider>();
        if (backCollider != null)
        {
            Object.DestroyImmediate(backCollider);
        }
        var backRenderer = backdrop.GetComponent<MeshRenderer>();
        var backMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        backMat.color = new Color(0.1f, 0.1f, 0.15f, 1f);
        backRenderer.sharedMaterial = backMat;

        // Box collider on root for poke interaction detection
        var boxCol = root.AddComponent<BoxCollider>();
        boxCol.size = new Vector3(width, height, 0.02f);
        boxCol.isTrigger = true;

        return root;
    }

    /// <summary>
    /// Wires a button's click event to a method on a component via ButtonClickRelay.
    /// </summary>
    private static void WireButtonEvent(GameObject button, UnityEngine.Object target, string methodName)
    {
        var relay = button.GetComponent<ButtonClickRelay>();
        if (relay == null)
        {
            relay = button.AddComponent<ButtonClickRelay>();
        }
        relay.targetObject = target as Component;
        relay.methodName = methodName;
    }

    // ==================== Grabbable Helper ====================

    /// <summary>
    /// Makes an object grabbable with ISDK HandGrabInteractable (and optionally ray/distance grab).
    /// Adds: Rigidbody (kinematic), Grabbable, HandGrabInteractable child,
    /// and optionally RayInteractable + DistanceHandGrabInteractable children.
    /// </summary>
    private static void MakeGrabbable(GameObject target, bool includeRayGrab = false, bool includeDistanceGrab = false)
    {
        // Rigidbody
        var rb = target.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = Undo.AddComponent<Rigidbody>(target);
        }
        rb.isKinematic = true;
        rb.useGravity = false;

        // Collider (BoxCollider if none exists)
        var existingCollider = target.GetComponent<Collider>();
        if (existingCollider == null)
        {
            var col = Undo.AddComponent<BoxCollider>(target);
            col.isTrigger = true;
        }

        // Grabbable component (ISDK base for grab interactions)
        if (target.GetComponent<Grabbable>() == null)
        {
            target.AddComponent<Grabbable>();
        }

        // ---- ISDK_HandGrabInteraction child ----
        var handGrabGO = CreateChild(target, "ISDK_HandGrabInteraction");
        var handGrabInteractable = handGrabGO.AddComponent<HandGrabInteractable>();
        SetSerializedField(handGrabInteractable, "_rigidbody", rb);
        SetSerializedField(handGrabInteractable, "_supportedGrabTypes", (int)GrabTypeFlags.All);

        if (includeRayGrab)
        {
            // ---- ISDK_RayGrabInteraction child ----
            var rayGrabGO = CreateChild(target, "ISDK_RayGrabInteraction");

            // RayInteractable needs a surface — add PlaneSurface on the child
            var raySurface = rayGrabGO.AddComponent<PlaneSurface>();
            raySurface.Facing = PlaneSurface.NormalFacing.Forward;

            var rayInteractable = rayGrabGO.AddComponent<RayInteractable>();
            SetSerializedInterfaceField(rayInteractable, "_surface", raySurface);
        }

        if (includeDistanceGrab)
        {
            // ---- ISDK_DistanceHandGrabInteraction child ----
            var distGrabGO = CreateChild(target, "ISDK_DistanceHandGrabInteraction");
            var distGrabInteractable = distGrabGO.AddComponent<DistanceHandGrabInteractable>();
            SetSerializedField(distGrabInteractable, "_rigidbody", rb);
            SetSerializedField(distGrabInteractable, "_supportedGrabTypes", (int)GrabTypeFlags.Pinch);
        }
    }

    // ==================== Nudge Manager Panel ====================

    private static GameObject CreateNudgeManagerPanel(FinesseTouch finesseTouch, ObstacleManager obstacleManager)
    {
        var panel = new GameObject("NudgeManager");
        panel.transform.position = new Vector3(0.25f, 1f, 0.75f);
        panel.transform.localScale = Vector3.one * 0.3f;
        Undo.RegisterCreatedObjectUndo(panel, "Create Nudge Manager Panel");

        // Collider for the whole panel
        var col = panel.AddComponent<BoxCollider>();
        col.size = new Vector3(0.9f, 0.7f, 0.09f);
        col.isTrigger = true;

        MakeGrabbable(panel, includeRayGrab: false, includeDistanceGrab: false);

        // Button definitions: (row, col, label, target, method)
        var buttons = new (int row, int col, string label, Component target, string method)[]
        {
            (0, 0, "Z Up",      finesseTouch,    "NudgeZUp"),
            (0, 1, "Z Down",    finesseTouch,    "NudgeZDown"),
            (1, 0, "Y Up",      finesseTouch,    "NudgeYUp"),
            (1, 1, "Y Down",    finesseTouch,    "NudgeYDown"),
            (2, 0, "X Up",      finesseTouch,    "NudgeXUp"),
            (2, 1, "X Down",    finesseTouch,    "NudgeXDown"),
            (3, 0, "Rot Left",  finesseTouch,    "RotateYDown"),
            (3, 1, "Rot Right", finesseTouch,    "RotateYUp"),
            (4, 0, "cm mode",   finesseTouch,    "SetCentimeter"),
            (4, 1, "mm mode",   finesseTouch,    "SetMilimeter"),
            (5, 0, "Setup Vis", obstacleManager, "ToggleSetupVisuals"),
            (5, 1, "Activate",  obstacleManager, "ToggleTrialSequenceActive"),
        };

        float startX = -ButtonSpacingX * 0.5f;
        float startY = ButtonSpacingY * 2.5f;

        foreach (var (row, c, label, target, method) in buttons)
        {
            var btn = CreatePokeButton(label);
            btn.transform.SetParent(panel.transform, false);
            btn.transform.localPosition = new Vector3(
                startX + c * ButtonSpacingX,
                startY - row * ButtonSpacingY,
                0f);
            WireButtonEvent(btn, target, method);
        }

        // Panel title
        var titleGO = new GameObject("PanelTitle");
        titleGO.transform.SetParent(panel.transform, false);
        titleGO.transform.localPosition = new Vector3(0f, startY + 0.04f, -0.001f);
        var titleTMP = titleGO.AddComponent<TextMeshPro>();
        titleTMP.text = "Nudge Manager";
        titleTMP.fontSize = 3;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.rectTransform.sizeDelta = new Vector2(0.3f, 0.05f);
        titleTMP.color = Color.cyan;

        return panel;
    }

    // ==================== Command Staff Panel ====================

    private static GameObject CreateCommandStaffPanel(
        CSVLoader csvLoader,
        ObstacleManager obstacleManager,
        AnchorManager anchorManager)
    {
        var staff = new GameObject("CommandStaff");
        staff.transform.position = new Vector3(0.5f, 1.5f, 0f);
        Undo.RegisterCreatedObjectUndo(staff, "Create Command Staff");

        // Collider
        var col = staff.AddComponent<BoxCollider>();
        col.size = new Vector3(0.2f, 0.5f, 0.05f);
        col.isTrigger = true;

        // All 3 grab types for experimenter access from any distance/input
        MakeGrabbable(staff, includeRayGrab: true, includeDistanceGrab: true);

        // Button definitions (single column)
        var buttons = new (string label, Component target, string method)[]
        {
            ("Load CSV",       csvLoader,       "TryLoadingCSV"),
            ("Next Trial",     csvLoader,       "GetNextTrial"),
            ("Prev Trial",     csvLoader,       "GetPreviousTrial"),
            ("Arm Obstacle",   obstacleManager, "ToggleObstacleMovement"),
            ("Auto Reset",     obstacleManager, "ToggleAutoReset"),
            ("Anchor",         anchorManager,   "AnchorTargetVoid"),
            ("Reset Anchors",  anchorManager,   "ResetAllAnchors"),
            ("Recenter",       anchorManager,   "RecenterAndReset"),
        };

        float startY = ButtonSpacingY * 3.5f;

        for (int i = 0; i < buttons.Length; i++)
        {
            var (label, target, method) = buttons[i];
            var btn = CreatePokeButton(label);
            btn.transform.SetParent(staff.transform, false);
            btn.transform.localPosition = new Vector3(0f, startY - i * ButtonSpacingY, 0f);
            WireButtonEvent(btn, target, method);
        }

        // Indicator cubes
        var activeIndicator = CreateIndicatorCube("active_indicator", Color.green);
        activeIndicator.transform.SetParent(staff.transform, false);
        activeIndicator.transform.localPosition = new Vector3(0.1f, startY + 0.03f, 0f);
        activeIndicator.SetActive(false);

        var autoIndicator = CreateIndicatorCube("auto_indicator", Color.blue);
        autoIndicator.transform.SetParent(staff.transform, false);
        autoIndicator.transform.localPosition = new Vector3(0.12f, startY + 0.03f, 0f);
        autoIndicator.SetActive(false);

        var autoAndActiveIndicator = CreateIndicatorCube("auto_and_active_indicator", Color.yellow);
        autoAndActiveIndicator.transform.SetParent(staff.transform, false);
        autoAndActiveIndicator.transform.localPosition = new Vector3(0.14f, startY + 0.03f, 0f);
        autoAndActiveIndicator.SetActive(false);

        // Panel title
        var titleGO = new GameObject("StaffTitle");
        titleGO.transform.SetParent(staff.transform, false);
        titleGO.transform.localPosition = new Vector3(0f, startY + 0.05f, -0.001f);
        var titleTMP = titleGO.AddComponent<TextMeshPro>();
        titleTMP.text = "Command Staff";
        titleTMP.fontSize = 3;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.rectTransform.sizeDelta = new Vector2(0.3f, 0.05f);
        titleTMP.color = Color.cyan;

        return staff;
    }

    private static GameObject CreateIndicatorCube(string name, Color color)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.localScale = Vector3.one * 0.015f;

        var renderer = cube.GetComponent<MeshRenderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = color;
        renderer.sharedMaterial = mat;

        return cube;
    }

    // ==================== Helpers ====================

    private static GameObject CreateChild(GameObject parent, string name)
    {
        var child = new GameObject(name);
        child.transform.SetParent(parent.transform, false);
        child.transform.localPosition = Vector3.zero;
        return child;
    }

    /// <summary>
    /// Ensures a tag exists in the project's TagManager. Adds it if missing.
    /// </summary>
    private static void EnsureTagExists(string tag)
    {
        for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
        {
            if (UnityEditorInternal.InternalEditorUtility.tags[i] == tag)
            {
                return;
            }
        }
        UnityEditorInternal.InternalEditorUtility.AddTag(tag);
    }

    // ==================== SerializedObject Helpers ====================
    // ISDK components use private [SerializeField] fields (often with [Interface] attribute
    // serialized as UnityEngine.Object). These helpers set them via SerializedObject.

    /// <summary>
    /// Sets a serialized field on a component via SerializedObject.
    /// Works for object references, floats, ints, enums, etc.
    /// </summary>
    private static void SetSerializedField(Component component, string fieldName, Object value)
    {
        var so = new SerializedObject(component);
        var prop = so.FindProperty(fieldName);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        else
        {
            Debug.LogWarning($"SetSerializedField: Property '{fieldName}' not found on {component.GetType().Name}");
        }
    }

    private static void SetSerializedField(Component component, string fieldName, float value)
    {
        var so = new SerializedObject(component);
        var prop = so.FindProperty(fieldName);
        if (prop != null)
        {
            prop.floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        else
        {
            Debug.LogWarning($"SetSerializedField: Property '{fieldName}' not found on {component.GetType().Name}");
        }
    }

    private static void SetSerializedField(Component component, string fieldName, int value)
    {
        var so = new SerializedObject(component);
        var prop = so.FindProperty(fieldName);
        if (prop != null)
        {
            prop.intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        else
        {
            Debug.LogWarning($"SetSerializedField: Property '{fieldName}' not found on {component.GetType().Name}");
        }
    }

    /// <summary>
    /// Sets a [SerializeField, Interface(typeof(T))] field which is serialized as UnityEngine.Object.
    /// This is how ISDK stores interface references (e.g., ISurfacePatch, IInteractableView).
    /// </summary>
    private static void SetSerializedInterfaceField(Component component, string fieldName, Object value)
    {
        SetSerializedField(component, fieldName, value);
    }

    /// <summary>
    /// Sets a List of [Interface] fields (e.g., ClippedPlaneSurface._clippers).
    /// </summary>
    private static void SetSerializedInterfaceListField(Component component, string fieldName, Object[] values)
    {
        var so = new SerializedObject(component);
        var prop = so.FindProperty(fieldName);
        if (prop != null && prop.isArray)
        {
            prop.ClearArray();
            for (int i = 0; i < values.Length; i++)
            {
                prop.InsertArrayElementAtIndex(i);
                prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        else
        {
            Debug.LogWarning($"SetSerializedInterfaceListField: Array property '{fieldName}' not found on {component.GetType().Name}");
        }
    }

    /// <summary>
    /// Adds a persistent listener to a serialized UnityEvent field at edit-time.
    /// Uses UnityEventTools to persist the listener into the scene/prefab.
    /// </summary>
    private static void WireUnityEventPersistent(Component component, string eventFieldName, Object target, string methodName)
    {
        var so = new SerializedObject(component);
        var eventProp = so.FindProperty(eventFieldName);
        if (eventProp == null)
        {
            Debug.LogWarning($"WireUnityEventPersistent: Property '{eventFieldName}' not found on {component.GetType().Name}");
            return;
        }

        // Use reflection to get the actual UnityEvent field and add persistent listener
        var fieldInfo = component.GetType().GetField(eventFieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (fieldInfo != null)
        {
            var unityEvent = fieldInfo.GetValue(component) as UnityEvent;
            if (unityEvent != null)
            {
                var targetMethod = UnityEventBase.GetValidMethodInfo(target, methodName, new System.Type[0]);
                if (targetMethod != null)
                {
                    var action = System.Delegate.CreateDelegate(typeof(UnityAction), target, targetMethod) as UnityAction;
                    UnityEventTools.AddPersistentListener(unityEvent, action);
                }
                else
                {
                    Debug.LogWarning($"WireUnityEventPersistent: Method '{methodName}' not found on {target.GetType().Name}");
                }
            }
        }
    }
}
