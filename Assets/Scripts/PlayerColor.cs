using Fusion;
using UnityEngine;

public class PlayerColor : NetworkBehaviour
{
    public MeshRenderer MeshRenderer; // Reference to the MeshRenderer to change its color.

    [Networked, OnChangedRender(nameof(ColorChanged))]
    public Color NetworkedColor { get; set; } // The color synchronized over the network.

    public override void Spawned()
    {
        // Generate and set a random color if this instance has state authority.
        if (HasStateAuthority)
        {
            SetRandomColor();
        }
    }

    void Update()
    {
        // Change color when the E key is pressed and this instance has state authority.
        if (HasStateAuthority && Input.GetKeyDown(KeyCode.E))
        {
            SetRandomColor();
        }
    }

    /// <summary>
    /// Updates the color of the MeshRenderer and PlayerController to the NetworkedColor.
    /// This method is called automatically whenever NetworkedColor changes.
    /// </summary>
    void ColorChanged()
    {
        // Apply the new color to the player's mesh.
        MeshRenderer.material.color = NetworkedColor;

        // Update the player controller's color to the new networked color.
        GetComponent<PlayerController>().PlayerColor = NetworkedColor;
    }

    /// <summary>
    /// Sets NetworkedColor to a new random color with full opacity.
    /// </summary>
    private void SetRandomColor()
    {
        NetworkedColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1f);
    }
}
