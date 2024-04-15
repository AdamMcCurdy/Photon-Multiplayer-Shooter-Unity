using System;
using UnityEngine;
using Fusion;

public class PlayerController : NetworkBehaviour
{
    // Public Fields - Exposed in Unity for customization
    public GameObject CamPivotElevation;
    public GameObject CamPivotPan;
    public float JumpHeight = 1f;
    public float Speed = 5f;
    public float MouseSpeedX = 500f;
    public float MouseSpeedY = 500f;
    public float RotationSpeed = 75f;
    public float Gravity = -19.62f;
    public float ResetRotationSpeed = 5f;
    public float BackDamper = 2f;
    public float SideDamper = 1.5f;
    public float RotationDamper = 2f;
    public float CamMaxRange = 50f;
    public float CamZoomSpeed = 500f;
    public float CamPanSpeedDamper = 5000f;
    public Color PlayerColor;

    public Camera cam;  // Main camera associated with the player.

    // Private Fields
    private CharacterController controller;
    private Vector3 velocity;  // Current movement velocity.
    private float camElevationRate = 0f;
    private float camPanRate = 0f;

    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            AttachCamera();
            controller = GetComponent<CharacterController>();
        }
    }

    /// <summary>
    /// Enable and tag the main camera once the player has spawned.
    /// </summary>
    private void AttachCamera()
    {
        cam.enabled = true;
        cam.tag = "MainCamera";
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
        {
            return;
        }

        HandleMovement();
        HandleMouseRotation();
        HandleCameraRotation();
        HandleGravity();
    }

    /// <summary>
    /// Handles player movement using keyboard input.
    /// </summary>
    private void HandleMovement()
    {
        Vector3 move = transform.forward * Input.GetAxis("Vertical") +
                       transform.right * Input.GetAxis("Horizontal");
        move *= Runner.DeltaTime * Speed;

        var normalizedMovement = move.normalized * Speed + velocity;
        controller.Move(normalizedMovement * Runner.DeltaTime);
    }

    /// <summary>
    /// Handles mouse-based rotation affecting camera and player rotation.
    /// </summary>
    private void HandleMouseRotation()
    {
        var x = Input.GetAxis("Mouse X");
        var y = Input.GetAxis("Mouse Y");

        camElevationRate = Mathf.Lerp(camElevationRate, -y * MouseSpeedY * Runner.DeltaTime, 0.1f);
        camPanRate = Mathf.Lerp(camPanRate, x * MouseSpeedX * Runner.DeltaTime, 0.1f);

        CamPivotElevation.transform.Rotate(Vector3.right, camElevationRate);
        transform.Rotate(Vector3.up, camPanRate);
    }

    /// <summary>
    /// Locks the cursor and handles additional camera rotations.
    /// </summary>
    private void HandleCameraRotation()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        var rotSpeed = RotationSpeed / RotationDamper;
        transform.Rotate(Vector3.up, Input.GetAxis("Horizontal") * rotSpeed * Runner.DeltaTime);
    }

    /// <summary>
    /// Applies gravity to the player's vertical movement.
    /// </summary>
    private void HandleGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = 0f;  // Reset the downward velocity when grounded.
        }

        velocity.y += Gravity * Runner.DeltaTime;  // Apply gravity to downward velocity.
    }
}