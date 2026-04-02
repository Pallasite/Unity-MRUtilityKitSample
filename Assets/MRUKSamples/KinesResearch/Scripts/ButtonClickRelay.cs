using UnityEngine;

/// <summary>
/// Lightweight component that relays click/poke events to a target method.
/// Added by KinesResearchSceneBuilder to wire button events at edit-time.
/// At runtime, call Invoke() to trigger the stored method on the target component.
/// Compatible with both ISDK PokeInteractable events and simple collider-based input.
/// </summary>
public class ButtonClickRelay : MonoBehaviour
{
    [Tooltip("The component that owns the method to call.")]
    public Component targetObject;

    [Tooltip("The name of the public void method to invoke on targetObject.")]
    public string methodName;

    /// <summary>
    /// Invokes the stored method on the target component via reflection.
    /// Wire this to PokeInteractable.WhenSelect or InteractableUnityEventWrapper.
    /// </summary>
    public void Invoke()
    {
        if (targetObject == null)
        {
            Debug.LogWarning($"ButtonClickRelay on {name}: targetObject is null.");
            return;
        }

        if (string.IsNullOrEmpty(methodName))
        {
            Debug.LogWarning($"ButtonClickRelay on {name}: methodName is empty.");
            return;
        }

        var method = targetObject.GetType().GetMethod(methodName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

        if (method != null)
        {
            method.Invoke(targetObject, null);
        }
        else
        {
            Debug.LogError($"ButtonClickRelay on {name}: Method '{methodName}' not found on {targetObject.GetType().Name}.");
        }
    }

    /// <summary>
    /// Visual feedback: change button color briefly on click.
    /// Call from the same event as Invoke().
    /// </summary>
    public void FlashButton()
    {
        var visual = transform.Find("ButtonVisual");
        if (visual != null)
        {
            var renderer = visual.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                var originalColor = renderer.material.color;
                renderer.material.color = Color.white;
                StartCoroutine(ResetColor(renderer, originalColor, 0.15f));
            }
        }
    }

    private System.Collections.IEnumerator ResetColor(MeshRenderer renderer, Color original, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (renderer != null)
        {
            renderer.material.color = original;
        }
    }
}
