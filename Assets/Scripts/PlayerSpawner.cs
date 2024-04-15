using Fusion;
using UnityEngine;

/// <summary>
/// Handles the spawning of player entities in the game when new players join.
/// This class is part of the game's network simulation.
/// </summary>
public class PlayerSpawner : SimulationBehaviour, IPlayerJoined
{
    public GameObject PlayerPrefab; // The prefab for the player object to spawn.

    /// <summary>
    /// Called when a player joins the game session. This method checks if the joining player
    /// is the local player and if so, spawns a player instance at a predefined location.
    /// </summary>
    /// <param name="player">Reference to the joining player.</param>
    public void PlayerJoined(PlayerRef player)
    {
        // Check if the joining player is the local player.
        if (player == Runner.LocalPlayer)
        {
            // Spawn the player prefab at position (0, 1, 0) with no rotation.
            Runner.Spawn(PlayerPrefab, new Vector3(0, 1, 0), Quaternion.identity);
        }
    }
}
