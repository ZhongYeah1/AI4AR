using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script for handling camera rotation flipping and position resetting
/// Attach this script to the OVRCameraRig object
/// 
/// Features:
/// - F key: Flip the camera 180 degrees on Y-axis (turn around)
/// - Tab key: Reset camera position to origin (0,0,0)
/// </summary>
public class CameraResetFlipper : MonoBehaviour
{
    [Header("Camera Components")]
    [SerializeField] private Transform centerEyeAnchor;
    
    void Start()
    {
        // Find the CenterEyeAnchor if not assigned
        if (centerEyeAnchor == null)
        {
            centerEyeAnchor = transform.Find("TrackingSpace/CenterEyeAnchor");
            if (centerEyeAnchor == null)
            {
                Debug.LogError("CenterEyeAnchor not found! Make sure this script is attached to OVRCameraRig.");
            }
        }
    }

    void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// Handle user input for camera operations
    /// </summary>
    private void HandleInput()
    {
        // F key: Flip camera 180 degrees on Y-axis
        if (Input.GetKeyDown(KeyCode.F))
        {
            FlipCamera();
        }

        // Tab key: Reset camera position to origin
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ResetCameraPosition();
        }
    }

    /// <summary>
    /// Flip the camera 180 degrees around the Y-axis (turn around)
    /// Preserves pitch and roll, only rotates yaw
    /// </summary>
    private void FlipCamera()
    {
        // Rotate the OVRCameraRig 180 degrees around Y-axis
        transform.Rotate(0, 180, 0);
        
        Debug.Log("Camera flipped 180 degrees");
    }

    /// <summary>
    /// Reset the camera position to the origin (0,0,0) in global coordinates
    /// This works by offsetting the OVRCameraRig position to counteract the head tracking
    /// </summary>
    private void ResetCameraPosition()
    {
        if (centerEyeAnchor == null)
        {
            Debug.LogError("CenterEyeAnchor is not assigned!");
            return;
        }

        // Calculate the offset needed to bring the camera to origin
        // The CenterEyeAnchor position represents where the head is relative to the tracking space
        // We need to move the OVRCameraRig to offset this tracked position
        Vector3 headPosition = centerEyeAnchor.localPosition;
        
        // Set the OVRCameraRig position to negative of the head position
        // This effectively places the head at (0,0,0) in world space
        transform.position = -headPosition;
        
        Debug.Log($"Camera position reset to origin. Head offset: {headPosition}");
    }
}
