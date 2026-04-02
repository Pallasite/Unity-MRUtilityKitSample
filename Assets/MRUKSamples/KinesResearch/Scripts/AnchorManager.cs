using UnityEngine;
using Meta.XR.BuildingBlocks;
using UnityEngine.Events;

/// <summary>
/// Manages spatial anchors for obstacle positioning.
/// Provides methods to anchor existing objects (preferred) or instantiate new anchored objects (fallback/reference).
/// </summary>
public class AnchorManager : MonoBehaviour
{
    [Header("Target to Anchor")]
    [Tooltip("The default GameObject to attach anchors to (usually obstacle_anchor).")]
    public GameObject target_object;

    [Header("Spatial Anchor Core Building Block Reference")]
    [Tooltip("Reference to the SpatialAnchorCoreBuildingBlock component in the scene.")]
    public SpatialAnchorCoreBuildingBlock anchor_core_building_block;

    [Header("Events")]
    [Tooltip("Fired when an anchor is successfully created.")]
    public UnityEvent<GameObject> OnAnchorCreated;

    [Tooltip("Fired when an anchor is removed.")]
    public UnityEvent<GameObject> OnAnchorRemoved;

    // Track manually added anchors to clean them up properly (destructively removing component only)
    private System.Collections.Generic.List<OVRSpatialAnchor> _created_anchors = new System.Collections.Generic.List<OVRSpatialAnchor>();

    // ==================== PRIMARY METHODS (Anchor Existing Objects) ====================

    /// <summary>
    /// Anchors an existing GameObject by adding OVRSpatialAnchor component directly.
    /// This is the PREFERRED method - it anchors your object in place without instantiating a new one.
    /// After anchoring, the object's world position is locked by the Meta SDK.
    /// </summary>
    /// <param name="target">The GameObject to anchor (usually obstacle_anchor)</param>
    /// <returns>The OVRSpatialAnchor component, or null if failed</returns>
    public OVRSpatialAnchor AnchorTarget(GameObject target)
    {
        if (target == null)
        {
            Debug.LogError("AnchorManager: Cannot anchor null target.");
            return null;
        }

        // Check if already anchored
        OVRSpatialAnchor existing_anchor = target.GetComponent<OVRSpatialAnchor>();
        if (existing_anchor != null)
        {
            Debug.LogWarning($"AnchorManager: {target.name} is already anchored.");
            // Ensure it's tracked if we missed it (e.g. added in editor)
            if (!_created_anchors.Contains(existing_anchor))
            {
                _created_anchors.Add(existing_anchor);
            }
            return existing_anchor;
        }

        // Add the spatial anchor component - this locks the world position
        OVRSpatialAnchor anchor = target.AddComponent<OVRSpatialAnchor>();

        // Track it so we can remove it later without destroying the GameObject
        _created_anchors.Add(anchor);

        Debug.Log($"AnchorManager: Anchored {target.name} at position {target.transform.position}");
        OnAnchorCreated?.Invoke(target);

        return anchor;
    }

    /// <summary>
    /// Anchors the default target_object.
    /// </summary>
    public OVRSpatialAnchor AnchorTarget()
    {
        return AnchorTarget(target_object);
    }

    /// <summary>
    /// [UNITY EVENT] Void wrapper for AnchorTarget - visible in Unity's function picker.
    /// Anchors the default target_object.
    /// </summary>
    public void AnchorTargetVoid()
    {
        AnchorTarget(target_object);
    }

    /// <summary>
    /// [UNITY EVENT] Void wrapper with GameObject parameter - for dynamic Unity events.
    /// </summary>
    public void AnchorTargetVoid(GameObject target)
    {
        AnchorTarget(target);
    }

    /// <summary>
    /// Removes the spatial anchor from a GameObject, allowing it to be moved/nudged again.
    /// </summary>
    /// <param name="target">The GameObject to unanchor</param>
    /// <returns>True if anchor was removed, false if no anchor existed</returns>
    public bool RemoveAnchor(GameObject target)
    {
        if (target == null)
        {
            Debug.LogError("AnchorManager: Cannot remove anchor from null target.");
            return false;
        }

        OVRSpatialAnchor anchor = target.GetComponent<OVRSpatialAnchor>();
        if (anchor == null)
        {
            Debug.LogWarning($"AnchorManager: {target.name} has no anchor to remove.");
            return false;
        }

        // Stop tracking
        if (_created_anchors.Contains(anchor))
        {
            _created_anchors.Remove(anchor);
        }

        // Destroy the anchor component
        Destroy(anchor);

        Debug.Log($"AnchorManager: Removed anchor from {target.name}. Object can now be nudged.");
        OnAnchorRemoved?.Invoke(target);

        return true;
    }

    /// <summary>
    /// Removes the anchor from the default target_object.
    /// </summary>
    public bool RemoveAnchor()
    {
        return RemoveAnchor(target_object);
    }

    /// <summary>
    /// [UNITY EVENT] Void wrapper for RemoveAnchor - visible in Unity's function picker.
    /// Removes the anchor from the default target_object.
    /// </summary>
    public void RemoveAnchorVoid()
    {
        RemoveAnchor(target_object);
    }

    /// <summary>
    /// [UNITY EVENT] Void wrapper with GameObject parameter - for dynamic Unity events.
    /// </summary>
    public void RemoveAnchorVoid(GameObject target)
    {
        RemoveAnchor(target);
    }

    /// <summary>
    /// Checks if a GameObject is currently anchored.
    /// </summary>
    public bool IsAnchored(GameObject target)
    {
        if (target == null) return false;
        return target.GetComponent<OVRSpatialAnchor>() != null;
    }

    /// <summary>
    /// Checks if the default target_object is anchored.
    /// </summary>
    public bool IsAnchored()
    {
        return IsAnchored(target_object);
    }

    /// <summary>
    /// Toggles anchor state - anchors if not anchored, removes if anchored.
    /// </summary>
    public void ToggleAnchor(GameObject target)
    {
        if (IsAnchored(target))
        {
            RemoveAnchor(target);
        }
        else
        {
            AnchorTarget(target);
        }
    }

    /// <summary>
    /// Toggles anchor state on default target_object.
    /// </summary>
    public void ToggleAnchor()
    {
        ToggleAnchor(target_object);
    }

    // ==================== UTILITY METHODS ====================

    /// <summary>
    /// Recenters the headset's origin and erases all existing anchors.
    /// Handles both manual anchors (non-destructive) and fallback anchors (destructive).
    /// </summary>
    public void RecenterAndReset()
    {
        OVRManager.display.RecenterPose();
        ResetAllAnchors();
    }

    /// <summary>
    /// Erases all anchors.
    /// 1. Destroys components of manually tracked anchors.
    /// 2. Calls Building Block's EraseAllAnchors for any others.
    /// </summary>
    public void ResetAllAnchors()
    {
        // 1. Clean up manually created anchors (Non-destructive to GameObject)
        // Use a temp list to avoid modification during iteration issues, or simple backwards loop
        for (int i = _created_anchors.Count - 1; i >= 0; i--)
        {
            var anchor = _created_anchors[i];
            if (anchor != null)
            {
                Destroy(anchor);
            }
        }
        _created_anchors.Clear();

        // 2. Clean up fallback anchors (Destructive to GameObject, handled by Building Block)
        if (anchor_core_building_block != null)
        {
            anchor_core_building_block.EraseAllAnchors();
        }
    }

    // ==================== FALLBACK/REFERENCE METHODS (Instantiate New Objects) ====================
    // These methods use the Building Block's InstantiateSpatialAnchor, which creates NEW objects.
    // Kept for reference - the AnchorTarget() method above is usually preferred.

    /// <summary>
    /// [FALLBACK] Uses Building Block to instantiate a new anchored object at target's pose.
    /// NOTE: This creates a NEW object, it doesn't anchor the existing one.
    /// </summary>
    [ContextMenu("Instantiate Anchor (Building Block)")]
    public void InstantiateAnchorAtTarget()
    {
        if (target_object == null)
        {
            Debug.LogError("AnchorManager: No target_object assigned.");
            return;
        }
        if (anchor_core_building_block == null)
        {
            Debug.LogError("AnchorManager: SpatialAnchorCoreBuildingBlock reference is not assigned.");
            return;
        }

        // NOTE: This INSTANTIATES a new prefab at the target's position
        // It does NOT anchor the existing target_object
        anchor_core_building_block.InstantiateSpatialAnchor(
            target_object,
            target_object.transform.position,
            target_object.transform.rotation
        );

        Debug.Log($"AnchorManager: [Building Block] Instantiated new anchor at {target_object.name}'s position.");
    }

    /// <summary>
    /// [FALLBACK] Clears existing anchors then instantiates a new one.
    /// </summary>
    [ContextMenu("Reset and Instantiate Anchor")]
    public void ResetAndInstantiateAnchor()
    {
        ResetAllAnchors();
        InstantiateAnchorAtTarget();
    }
}
