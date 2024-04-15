using System;
using UnityEngine;
using Fusion;

/// <summary>
/// Controls the behavior of a target in the game, including its color changes when hit.
/// This class is synchronized over the network to ensure that all players see the same target state.
/// </summary>
public class TargetController : NetworkBehaviour
{
    // This property holds the color of the target. Changes to this property are network-synchronized.
    [Networked, OnChangedRender(nameof(ColorChanged))]
    public Color TargetColor { get; set; }

    /// <summary>
    /// Called when the network object is spawned. Initializes the target's color to white.
    /// </summary>
    public override void Spawned()
    {
        TargetColor = Color.white;  // Default color is white when spawned.
    }

    /// <summary>
    /// Called when TargetColor changes. Updates the color of the target's material to reflect the new color.
    /// This method is specified as the callback for changes to the TargetColor property.
    /// </summary>
    public void ColorChanged()
    {
        // Fetches the Renderer component from this GameObject and updates its material color.
        Renderer targetRenderer = GetComponent<Renderer>();
        if (targetRenderer != null)
        {
            targetRenderer.material.color = TargetColor;
        }
        else
        {
            Debug.LogWarning("Renderer not found on the target object.");
        }
    }
}
