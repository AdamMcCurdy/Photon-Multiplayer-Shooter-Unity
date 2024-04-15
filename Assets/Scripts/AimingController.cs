using System;
using UnityEngine;
using Fusion;

public class AimingController : NetworkBehaviour
{
    // Public fields that can be set in the Unity editor.
    public bool showLine = false; // Whether to display the aiming line.
    public GameObject Shoulder; // Reference to the shoulder GameObject for aiming.
    public GameObject Gun; // Reference to the gun GameObject.
    public Vector3 AimPoint; // Point where the gun is aiming.
    private LineRenderer lineRenderer; // Line renderer component for visualizing the aim.
    private TargetController targetController; // Controller for handling target interactions.
    private Color color; // Color used for the aiming line and other UI elements.

    // Called when the object is spawned on the network.
    public override void Spawned()
    {
        if (showLine)
        {
            SetupLineRenderer(); // Set up the line renderer if it's needed.
        }
    }

    void Update()
    {
        HandleInput(); // Handle user input each frame.
    }

    // Fixed update method called for network updates.
    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority) // Check if the local client has authority to update state.
        {
            UpdateAiming(); // Update the aiming mechanism.
        }
    }

    private void HandleInput()
    {
        if (HasStateAuthority)
        {
            CheckReleaseInput(); // Check for release inputs like key up or mouse button up.
            CheckPressInput(); // Check for press inputs like key down or mouse button down.
            CheckHoldInput(); // Check for continuous hold inputs.
        }
    }

    private void CheckReleaseInput()
    {
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetMouseButtonUp(0))
        {
            if (showLine)
            {
                // Hide the aiming line when the input is released.
                lineRenderer.startWidth = 0;
                lineRenderer.endWidth = 0;
            }
        }
    }

    private void CheckPressInput()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (targetController != null)
            {
                // Update the target's color to match the player's color upon pressing the input.
                color = GetComponent<PlayerController>().PlayerColor;
                targetController.TargetColor = color;
                Debug.Log("Hit with color: " + color); // Log the color hit for debugging.
            }
        }
    }

    private void CheckHoldInput()
    {
        if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
        {
            if (showLine)
            {
                // Continuously update the line's position and dimensions while the input is held.
                lineRenderer.SetPosition(0, Gun.transform.position + Gun.transform.forward * 0.25f);
                lineRenderer.SetPosition(1, AimPoint);
                lineRenderer.startWidth = 0.1f;
                lineRenderer.endWidth = 0.1f;
            }
        }
    }

    private void UpdateAiming()
    {
        AimPoint = FindAimPoint(); // Update the aiming point.
        Shoulder.transform.LookAt(AimPoint); // Make the shoulder look at the aim point.
    }

    private Vector3 FindAimPoint()
    {
        // Generate a ray from the camera through the center of the screen.
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Target"))
            {
                // Assign the target controller if the hit object is a target.
                targetController = hit.collider.GetComponent<TargetController>();
            }
            return hit.point; // Return the exact point hit by the ray.
        }
        else
        {
            return ray.GetPoint(100); // Default far point if no hit occurs.
        }
    }

    private void SetupLineRenderer()
    {
        // Initialize the line renderer component on the gun, or add one if it doesn't exist.
        lineRenderer = Gun.GetComponent<LineRenderer>() ?? Gun.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2; // Set the number of points to two for a simple line.
        lineRenderer.material.color = color; // Set the color of the line.
    }
}
