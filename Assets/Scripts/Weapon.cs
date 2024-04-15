using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Weapon : NetworkBehaviour
{
    [SerializeField]
    private LayerMask _hitMask; // Layer mask to define what objects the weapon can hit.

    /// <summary>
    /// Fire the weapon. This function handles the logic of shooting a projectile(raycast),
    /// determining if it hits anything, and applying effects based on the hit.
    /// </summary>
    public void Fire()
    {
        // Default position for where a hit is detected.
        var hitPosition = Vector3.zero;
        
        // Configure hit options to include physical interactions and ignore input authority.
        var hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority;

        // Perform a raycast to simulate a hitscan projectile. This is an instant check along a line
        // from the weapon's current position forward up to 100 units. If it hits something that
        // matches the _hitMask, it processes further.
        if (Runner.LagCompensation.Raycast(transform.position, transform.forward, 100f,
                Object.InputAuthority, out var hit, _hitMask, hitOptions))
        {
            // Check if the raycast hit a collider with a rigidbody attached.
            if (hit.Collider != null && hit.Collider.attachedRigidbody != null)
            {
                // Attempt to get the TargetController from the hit object.
                var targetController = hit.Collider.gameObject.GetComponent<TargetController>();
                
                if (targetController != null)
                {
                    // If a TargetController is found, set its color to the player's color.
                    // This assumes the weapon is part of a player object that has a PlayerController.
                    var playerController = transform.root.GetComponent<PlayerController>();
                    targetController.TargetColor = playerController.PlayerColor;
                }
            }
        }
    }
}
