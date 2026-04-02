using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// Editor script that builds the full Kines Research scene hierarchy from a menu item.
/// Menu: Meta > Samples > Build Kines Research Scene
/// </summary>
public static class KinesResearchSceneBuilder
{
    // Layout constants
    private const float ButtonWidth = 0.12f;
    private const float ButtonHeight = 0.04f;
    private const float ButtonSpacingX = 0.13f;
    private const float ButtonSpacingY = 0.045f;
    private const float ButtonDepth = 0.001f;

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
            // Clean up existing
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
            "3. Import obstacle prefabs → assign to ObstacleManager.obstacle_visuals\n" +
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

        // Give it a distinct yellow material
        var renderer = cube.GetComponent<MeshRenderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = Color.yellow;
        renderer.sharedMaterial = mat;

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

    private static GameObject CreatePokeButton(string label, float width = ButtonWidth, float height = ButtonHeight)
    {
        var root = new GameObject("Btn_" + label);

        // Button visual (quad)
        var visual = GameObject.CreatePrimitive(PrimitiveType.Quad);
        visual.name = "ButtonVisual";
        visual.transform.SetParent(root.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(width, height, 1f);

        // Remove the default collider from the quad - we'll add our own on root
        var quadCollider = visual.GetComponent<MeshCollider>();
        if (quadCollider != null)
        {
            Object.DestroyImmediate(quadCollider);
        }

        // Material
        var renderer = visual.GetComponent<MeshRenderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.2f, 0.2f, 0.3f, 1f);
        renderer.sharedMaterial = mat;

        // Text label
        var textGO = new GameObject("Label");
        textGO.transform.SetParent(visual.transform);
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
        backdrop.transform.SetParent(root.transform);
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

        // Add a box collider on the root for poke interaction
        var boxCol = root.AddComponent<BoxCollider>();
        boxCol.size = new Vector3(width, height, 0.02f);
        boxCol.isTrigger = true;

        return root;
    }

    /// <summary>
    /// Wires a button's click event to a method on a component using UnityEvents at edit-time.
    /// </summary>
    private static void WireButtonEvent(GameObject button, UnityEngine.Object target, string methodName)
    {
        // We use a simple MonoBehaviour-based approach:
        // Add a ButtonEventRelay that calls the method on click.
        // Since PokeInteractable requires the Interaction SDK to be imported,
        // we add it conditionally.
        // For now, store the wiring info so it can be connected at runtime or
        // once the Interaction SDK compiles.

        // The pragmatic approach: we'll use the ISDK components if available,
        // otherwise fall back to a lightweight collider-based approach.
        // Since the SDK may not be compiled yet, we store metadata.
        var relay = button.AddComponent<ButtonClickRelay>();
        relay.targetObject = target as Component;
        relay.methodName = methodName;
    }

    // ==================== Grabbable Helper ====================

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
        if (target.GetComponent<Collider>() == null)
        {
            var col = Undo.AddComponent<BoxCollider>(target);
            col.isTrigger = true;
        }

        // Hand grab interaction child
        var handGrab = CreateChild(target, "ISDK_HandGrabInteraction");
        // Placeholder — actual ISDK components will be added once Interaction SDK compiles

        if (includeRayGrab)
        {
            CreateChild(target, "ISDK_RayGrabInteraction");
        }

        if (includeDistanceGrab)
        {
            CreateChild(target, "ISDK_DistanceHandGrabInteraction");
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
        mat.SetFloat("_Surface", 1); // transparent-ish
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
        // Check if tag already exists
        for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++)
        {
            if (UnityEditorInternal.InternalEditorUtility.tags[i] == tag)
            {
                return;
            }
        }
        UnityEditorInternal.InternalEditorUtility.AddTag(tag);
    }
}
