using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script for making a cube either match or mirror the user's movements
/// Attach this script to the MirrorHead object
/// 
/// Features:
/// - M key (first press): Make cube match user movements (cube looks away from user)
/// - M key (second press): Make cube mirror user movements (cube faces the user like a mirror)
/// - Handles both positional and rotational movements
/// </summary>
public class VRMirror : MonoBehaviour
{
    [Header("Camera Reference")]
    [SerializeField] private Transform centerEyeAnchor;
    
    [Header("Mirror Settings")]
    [SerializeField] private Vector3 cubeForwardOffset = new Vector3(0, 90, 0); // Cube's forward axis is 90° offset
    
    // Mirror modes
    private enum MirrorMode
    {
        Disabled,   // No mirroring
        Matching,   // Cube matches user movements (looks same direction)
        Mirroring   // Cube mirrors user movements (faces the user)
    }
    
    private MirrorMode currentMode = MirrorMode.Disabled;
    
    void Start()
    {
        // Find the CenterEyeAnchor if not assigned
        if (centerEyeAnchor == null)
        {
            // Try to find it in the typical OVR hierarchy
            GameObject ovrCameraRig = GameObject.Find("OVRCameraRig");
            if (ovrCameraRig != null)
            {
                centerEyeAnchor = ovrCameraRig.transform.Find("TrackingSpace/CenterEyeAnchor");
            }
            
            if (centerEyeAnchor == null)
            {
                Debug.LogError("CenterEyeAnchor not found! Please assign it in the inspector or ensure OVRCameraRig is in the scene.");
            }
        }
    }

    void Update()
    {
        HandleInput();
        UpdateCubeTransform();
    }

    /// <summary>
    /// Handle user input for toggling mirror modes
    /// </summary>
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMirrorMode();
        }
    }

    /// <summary>
    /// Toggle between different mirror modes
    /// </summary>
    private void ToggleMirrorMode()
    {
        switch (currentMode)
        {
            case MirrorMode.Disabled:
                currentMode = MirrorMode.Matching;
                Debug.Log("VR Mirror: Matching mode enabled - cube matches user movements");
                break;
            case MirrorMode.Matching:
                currentMode = MirrorMode.Mirroring;
                Debug.Log("VR Mirror: Mirroring mode enabled - cube mirrors user movements");
                break;
            case MirrorMode.Mirroring:
                currentMode = MirrorMode.Disabled;
                Debug.Log("VR Mirror: Disabled");
                break;
        }
    }

    /// <summary>
    /// Update the cube's transform based on the current mirror mode
    /// </summary>
    private void UpdateCubeTransform()
    {
        if (centerEyeAnchor == null || currentMode == MirrorMode.Disabled)
            return;

        Vector3 cameraPosition = centerEyeAnchor.position;
        Quaternion cameraRotation = centerEyeAnchor.rotation;

        switch (currentMode)
        {
            case MirrorMode.Matching:
                UpdateMatchingMode(cameraPosition, cameraRotation);
                break;
            case MirrorMode.Mirroring:
                UpdateMirroringMode(cameraPosition, cameraRotation);
                break;
        }
    }

    /// <summary>
    /// Update cube to match user movements (cube looks same direction as user)
    /// The cube should look like the camera is behind it
    /// </summary>
    private void UpdateMatchingMode(Vector3 cameraPosition, Quaternion cameraRotation)
    {
        // Position: cube matches camera position
        transform.position = cameraPosition;
        
        // Rotation: cube looks in the same direction as camera
        // Apply the forward offset to account for cube's orientation
        Quaternion offsetRotation = Quaternion.Euler(cubeForwardOffset);
        transform.rotation = cameraRotation * offsetRotation;
    }

    /// <summary>
    /// Update cube to mirror user movements (cube faces the user like a mirror)
    /// The cube should face the user so they can see the cube's face
    /// </summary>
    private void UpdateMirroringMode(Vector3 cameraPosition, Quaternion cameraRotation)
    {
        // Position: cube mirrors camera position (same as matching for this implementation)
        transform.position = cameraPosition;
        
        // Rotation: cube faces the camera (mirrors the rotation)
        // To mirror rotation, we need to invert around the Y axis and apply offset
        Vector3 eulerAngles = cameraRotation.eulerAngles;
        
        // Mirror the Y rotation (yaw) - add 180 degrees to face the camera
        eulerAngles.y += 180f;
        
        // Apply the mirrored rotation with cube offset
        Quaternion mirroredRotation = Quaternion.Euler(eulerAngles);
        Quaternion offsetRotation = Quaternion.Euler(cubeForwardOffset);
        transform.rotation = mirroredRotation * offsetRotation;
    }
}
