using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script for toggling position and rotation tracking of the headset
/// Attach this script to the OVRCameraRig object
/// 
/// Features:
/// - R key: Toggle headset rotation tracking on/off
/// - P key: Toggle headset position tracking on/off
/// 
/// Note: This works by manipulating the parent transform to counteract the tracked movements
/// </summary>
public class ToggleTracking : MonoBehaviour
{
    [Header("Camera Components")]
    [SerializeField] private Transform centerEyeAnchor;
    
    [Header("Tracking State")]
    [SerializeField] private bool rotationTrackingEnabled = true;
    [SerializeField] private bool positionTrackingEnabled = true;
    
    // Store the last known values when tracking was disabled
    private Quaternion lastRotationBeforeDisable;
    private Vector3 lastPositionBeforeDisable;
    
    // Store the offset rotations and positions to apply
    private Quaternion rotationOffset = Quaternion.identity;
    private Vector3 positionOffset = Vector3.zero;
    
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
        
        // Initialize last known values
        if (centerEyeAnchor != null)
        {
            lastRotationBeforeDisable = centerEyeAnchor.rotation;
            lastPositionBeforeDisable = centerEyeAnchor.position;
        }
    }

    void Update()
    {
        HandleInput();
        ApplyTrackingOffsets();
    }

    /// <summary>
    /// Handle user input for toggling tracking
    /// </summary>
    private void HandleInput()
    {
        // R key: Toggle rotation tracking
        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleRotationTracking();
        }

        // P key: Toggle position tracking
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePositionTracking();
        }
    }

    /// <summary>
    /// Toggle rotation tracking on/off
    /// </summary>
    private void ToggleRotationTracking()
    {
        rotationTrackingEnabled = !rotationTrackingEnabled;
        
        if (centerEyeAnchor == null) return;

        if (!rotationTrackingEnabled)
        {
            // Disable rotation tracking
            lastRotationBeforeDisable = centerEyeAnchor.rotation;
            Debug.Log("Rotation tracking disabled");
        }
        else
        {
            // Enable rotation tracking
            // Calculate the rotation offset needed to maintain the current apparent rotation
            Quaternion currentTrackedRotation = centerEyeAnchor.rotation;
            rotationOffset = lastRotationBeforeDisable * Quaternion.Inverse(currentTrackedRotation);
            Debug.Log("Rotation tracking enabled");
        }
    }

    /// <summary>
    /// Toggle position tracking on/off
    /// </summary>
    private void TogglePositionTracking()
    {
        positionTrackingEnabled = !positionTrackingEnabled;
        
        if (centerEyeAnchor == null) return;

        if (!positionTrackingEnabled)
        {
            // Disable position tracking
            lastPositionBeforeDisable = centerEyeAnchor.position;
            Debug.Log("Position tracking disabled");
        }
        else
        {
            // Enable position tracking
            // Calculate the position offset needed to maintain the current apparent position
            Vector3 currentTrackedPosition = centerEyeAnchor.position;
            positionOffset = lastPositionBeforeDisable - currentTrackedPosition;
            Debug.Log("Position tracking enabled");
        }
    }

    /// <summary>
    /// Apply the calculated offsets to disable tracking effects
    /// </summary>
    private void ApplyTrackingOffsets()
    {
        if (centerEyeAnchor == null) return;

        // Apply rotation offset when rotation tracking is disabled
        if (!rotationTrackingEnabled)
        {
            // Make the OVRCameraRig rotate to counteract the head rotation
            // This makes it appear as if rotation tracking is frozen
            Quaternion currentHeadRotation = centerEyeAnchor.rotation;
            Quaternion targetRigRotation = lastRotationBeforeDisable * Quaternion.Inverse(currentHeadRotation);
            transform.rotation = targetRigRotation;
        }
        else if (rotationOffset != Quaternion.identity)
        {
            // Apply the calculated rotation offset when tracking is re-enabled
            transform.rotation = rotationOffset * transform.rotation;
            rotationOffset = Quaternion.identity; // Reset after applying
        }

        // Apply position offset when position tracking is disabled
        if (!positionTrackingEnabled)
        {
            // Make the OVRCameraRig move to counteract the head movement
            // This makes it appear as if position tracking is frozen
            Vector3 currentHeadPosition = centerEyeAnchor.localPosition;
            Vector3 headMovement = currentHeadPosition - Vector3.zero; // Movement from origin
            transform.position = -headMovement + lastPositionBeforeDisable;
        }
        else if (positionOffset != Vector3.zero)
        {
            // Apply the calculated position offset when tracking is re-enabled
            transform.position += positionOffset;
            positionOffset = Vector3.zero; // Reset after applying
        }
    }

    /// <summary>
    /// Get the current rotation tracking state
    /// </summary>
    public bool IsRotationTrackingEnabled()
    {
        return rotationTrackingEnabled;
    }

    /// <summary>
    /// Get the current position tracking state
    /// </summary>
    public bool IsPositionTrackingEnabled()
    {
        return positionTrackingEnabled;
    }
}
