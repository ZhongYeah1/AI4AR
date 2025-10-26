using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Script for dynamically adjusting sphere sizes to create equal retinal image sizes
/// Attach this script to the StimulusManager object
/// 
/// Features:
/// - S key: Toggle the effect on/off
/// - Uses similar triangles principle: r1/d1 = r2/d2
/// - Blue spheres are resized to appear same size as red sphere
/// - Red sphere remains unchanged (reference sphere)
/// </summary>
public class GenerateStimuli : MonoBehaviour
{
    [Header("Sphere References")]
    [SerializeField] private Transform redSphere;    // Medium sphere (reference)
    [SerializeField] private Transform blueSphere1;  // Small sphere (will be resized)
    [SerializeField] private Transform blueSphere2;  // Large sphere (will be resized)
    
    [Header("Camera Reference")]
    [SerializeField] private Transform centerEyeAnchor;
    
    [Header("Settings")]
    [SerializeField] private bool effectActive = false;
    [SerializeField] private float redSphereRadius = 0.5f; // Reference radius for calculations
    
    // Store original scales to restore when effect is disabled
    private Vector3 originalBlueSphere1Scale;
    private Vector3 originalBlueSphere2Scale;
    
    // Sphere positions (as specified in assignment)
    private readonly Vector3 redSpherePosition = new Vector3(0f, 0f, -2.3f);
    private readonly Vector3 blueSphere1Position = new Vector3(1f, 0f, 2.5f);    // Small sphere
    private readonly Vector3 blueSphere2Position = new Vector3(-3.5f, 0f, -1.75f); // Large sphere

    void Start()
    {
        InitializeSpheres();
        FindCameraReference();
        StoreOriginalScales();
    }

    void Update()
    {
        HandleInput();
        
        if (effectActive)
        {
            UpdateSphereScales();
        }
    }

    /// <summary>
    /// Initialize sphere positions and references
    /// </summary>
    private void InitializeSpheres()
    {
        // Find spheres if not assigned
        if (redSphere == null)
            redSphere = transform.Find("Medium");
        if (blueSphere1 == null)
            blueSphere1 = transform.Find("Small");
        if (blueSphere2 == null)
            blueSphere2 = transform.Find("Large");

        // Verify all spheres are found
        if (redSphere == null || blueSphere1 == null || blueSphere2 == null)
        {
            Debug.LogError("Not all spheres found! Make sure StimulusManager has children named 'Medium', 'Small', and 'Large'");
            return;
        }

        // Set sphere positions as specified in assignment
        redSphere.position = redSpherePosition;
        blueSphere1.position = blueSphere1Position;
        blueSphere2.position = blueSphere2Position;

        Debug.Log("Spheres initialized at specified positions");
    }

    /// <summary>
    /// Find the camera reference for distance calculations
    /// </summary>
    private void FindCameraReference()
    {
        if (centerEyeAnchor == null)
        {
            GameObject ovrCameraRig = GameObject.Find("OVRCameraRig");
            if (ovrCameraRig != null)
            {
                centerEyeAnchor = ovrCameraRig.transform.Find("TrackingSpace/CenterEyeAnchor");
            }
            
            if (centerEyeAnchor == null)
            {
                Debug.LogError("CenterEyeAnchor not found! Please assign it in the inspector.");
            }
        }
    }

    /// <summary>
    /// Store the original scales of blue spheres for restoration
    /// </summary>
    private void StoreOriginalScales()
    {
        if (blueSphere1 != null)
            originalBlueSphere1Scale = blueSphere1.localScale;
        if (blueSphere2 != null)
            originalBlueSphere2Scale = blueSphere2.localScale;
    }

    /// <summary>
    /// Handle user input for toggling the effect
    /// </summary>
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            ToggleEffect();
        }
    }

    /// <summary>
    /// Toggle the sphere scaling effect on/off
    /// </summary>
    private void ToggleEffect()
    {
        effectActive = !effectActive;

        if (effectActive)
        {
            Debug.Log("Generate Stimuli: Effect activated - blue spheres will appear same size as red sphere");
        }
        else
        {
            // Restore original scales when effect is disabled
            RestoreOriginalScales();
            Debug.Log("Generate Stimuli: Effect deactivated - spheres restored to original sizes");
        }
    }

    /// <summary>
    /// Restore the original scales of the blue spheres
    /// </summary>
    private void RestoreOriginalScales()
    {
        if (blueSphere1 != null)
            blueSphere1.localScale = originalBlueSphere1Scale;
        if (blueSphere2 != null)
            blueSphere2.localScale = originalBlueSphere2Scale;
    }

    /// <summary>
    /// Update the scales of blue spheres to create equal retinal image sizes
    /// Uses similar triangles: r1/d1 = r2/d2, so r2 = r1 * d2/d1
    /// </summary>
    private void UpdateSphereScales()
    {
        if (centerEyeAnchor == null || redSphere == null || blueSphere1 == null || blueSphere2 == null)
            return;

        Vector3 cameraPosition = centerEyeAnchor.position;

        // Calculate distances from camera to each sphere
        float distanceToRed = Vector3.Distance(cameraPosition, redSphere.position);
        float distanceToBlue1 = Vector3.Distance(cameraPosition, blueSphere1.position);
        float distanceToBlue2 = Vector3.Distance(cameraPosition, blueSphere2.position);

        // Prevent division by zero
        if (distanceToRed <= 0f || distanceToBlue1 <= 0f || distanceToBlue2 <= 0f)
            return;

        // Calculate the required radii for blue spheres to appear same size as red sphere
        // Using similar triangles: r_blue / d_blue = r_red / d_red
        // Therefore: r_blue = r_red * d_blue / d_red
        float requiredRadius1 = redSphereRadius * distanceToBlue1 / distanceToRed;
        float requiredRadius2 = redSphereRadius * distanceToBlue2 / distanceToRed;

        // Calculate scale factors (assuming original sphere radius is 0.5)
        float originalRadius = 0.5f;
        float scaleFactor1 = requiredRadius1 / originalRadius;
        float scaleFactor2 = requiredRadius2 / originalRadius;

        // Apply the new scales
        blueSphere1.localScale = originalBlueSphere1Scale * scaleFactor1;
        blueSphere2.localScale = originalBlueSphere2Scale * scaleFactor2;
    }

    /// <summary>
    /// Get the current state of the effect
    /// </summary>
    public bool IsEffectActive()
    {
        return effectActive;
    }

    /// <summary>
    /// Debug method to display current distances and scale factors
    /// </summary>
    void OnGUI()
    {
        if (!effectActive || centerEyeAnchor == null) return;

        // Display debug information in the top-left corner
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Generate Stimuli Debug Info:");
        
        if (redSphere != null && blueSphere1 != null && blueSphere2 != null)
        {
            Vector3 cameraPos = centerEyeAnchor.position;
            float distRed = Vector3.Distance(cameraPos, redSphere.position);
            float distBlue1 = Vector3.Distance(cameraPos, blueSphere1.position);
            float distBlue2 = Vector3.Distance(cameraPos, blueSphere2.position);
            
            GUILayout.Label($"Distance to Red: {distRed:F2}");
            GUILayout.Label($"Distance to Blue1: {distBlue1:F2}");
            GUILayout.Label($"Distance to Blue2: {distBlue2:F2}");
            GUILayout.Label($"Blue1 Scale: {blueSphere1.localScale.x:F2}");
            GUILayout.Label($"Blue2 Scale: {blueSphere2.localScale.x:F2}");
        }
        
        GUILayout.EndArea();
    }
}
