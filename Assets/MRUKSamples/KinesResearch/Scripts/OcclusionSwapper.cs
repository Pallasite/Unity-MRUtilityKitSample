using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Automatically swaps between occlusion and non-occlusion materials based on player distance.
/// The depth occlusion material shows visual errors when the player is too far away due to
/// depth map inaccuracy at range. This component handles swapping automatically.
///
/// Use static methods (e.g., OcclusionSwapper.SetAllOcclusionDistance()) to control all instances.
/// </summary>
public class OcclusionSwapper : MonoBehaviour
{
    // ==================== STATIC REGISTRY ====================
    // All active OcclusionSwapper instances register here for bulk control

    private static HashSet<OcclusionSwapper> _all_instances = new HashSet<OcclusionSwapper>();

    /// <summary>
    /// Get all active OcclusionSwapper instances in the scene.
    /// </summary>
    public static IReadOnlyCollection<OcclusionSwapper> AllInstances => _all_instances;

    /// <summary>
    /// Set the occlusion distance for ALL OcclusionSwappers in the scene.
    /// </summary>
    public static void SetAllOcclusionDistance(float distance)
    {
        foreach (var swapper in _all_instances)
        {
            swapper.occlusion_distance = distance;
        }
    }

    /// <summary>
    /// Enable or disable auto-swap for ALL OcclusionSwappers in the scene.
    /// </summary>
    public static void SetAllAutoSwapEnabled(bool enabled)
    {
        foreach (var swapper in _all_instances)
        {
            swapper.auto_swap_enabled = enabled;
        }
    }

    /// <summary>
    /// Force all OcclusionSwappers to a specific occlusion state.
    /// </summary>
    public static void SetAllOcclusion(bool occlude)
    {
        foreach (var swapper in _all_instances)
        {
            swapper.SetObstacleOcclusion(occlude);
        }
    }

    // ==================== INSTANCE FIELDS ====================

    [Header("Materials")]
    [Tooltip("Material with depth occlusion enabled - used when player is close")]
    public Material depth_occlusion_material;

    [Tooltip("Material without depth occlusion - used when player is far")]
    public Material no_occlusion_material;

    [Header("Distance Settings")]
    [Tooltip("Distance at which to swap to occlusion material (player closer than this = use occlusion)")]
    public float occlusion_distance = 1f;

    [Header("Runtime Control")]
    [Tooltip("Enable/disable automatic distance-based swapping")]
    public bool auto_swap_enabled = true;

    // Cached references for performance
    private Transform _player;
    private Renderer _renderer;
    private bool _is_occluding = false;

    // ==================== LIFECYCLE ====================

    private void Awake()
    {
        // Register this instance
        _all_instances.Add(this);

        _renderer = GetComponent<Renderer>();
        if (_renderer == null)
        {
            Debug.LogError("OcclusionSwapper: No Renderer found on this GameObject.");
        }
    }

    private void OnDestroy()
    {
        // Unregister when destroyed
        _all_instances.Remove(this);
    }

    private void Start()
    {
        // Cache player reference (main camera = player head)
        if (Camera.main != null)
        {
            _player = Camera.main.transform;
        }
        else
        {
            Debug.LogWarning("OcclusionSwapper: No main camera found. Auto-swap will not work.");
        }

        // Start with non-occlusion material (player assumed far away initially)
        SetObstacleOcclusion(false);
    }

    private void Update()
    {
        if (!auto_swap_enabled || _player == null || _renderer == null) return;

        // Calculate XZ distance (ignore height difference)
        Vector3 player_xz = new Vector3(_player.position.x, transform.position.y, _player.position.z);
        float distance = Vector3.Distance(player_xz, transform.position);

        // Swap material based on distance
        bool should_occlude = distance <= occlusion_distance;

        // Only swap if state changed (avoid setting material every frame)
        if (should_occlude != _is_occluding)
        {
            SetObstacleOcclusion(should_occlude);
        }
    }

    // ==================== PUBLIC INSTANCE METHODS ====================

    /// <summary>
    /// Manually set the occlusion state. Also updates internal tracking.
    /// </summary>
    /// <param name="occlude">True to use depth occlusion material, false for no occlusion</param>
    public void SetObstacleOcclusion(bool occlude)
    {
        if (_renderer == null) return;

        _is_occluding = occlude;
        _renderer.material = occlude ? depth_occlusion_material : no_occlusion_material;
    }

    /// <summary>
    /// Enable or disable automatic distance-based material swapping.
    /// </summary>
    public void SetAutoSwapEnabled(bool enabled)
    {
        auto_swap_enabled = enabled;
    }
}
