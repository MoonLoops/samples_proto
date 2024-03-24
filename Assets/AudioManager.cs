// AUDIO MANAGER COMPONENT
// Audio Manager class that controls the target song and the sample tracks for a level.

using UnityEngine;
using FMOD.Studio;
using System;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public TargetFMOD targetFMOD;
    public NestedSampleEvent parentSampleEvent;
    
    // Width of each timeline or "track" for reference.
    public int timelineWidth = 1024;
    public int timelineHeight = 256;
    public int canvasHeight; 

    private Boolean canvasInitialized = false;
    private int targetLength; // The length of the target event in milliseconds, does not change.

    float pitchDelta = 0.001f;

    void Start() 
    {   
        targetLength = 0;
        canvasHeight = timelineHeight*3; // For sake of reference, say the canvas is 3 times the track height.
        
    }

    void Update()
    {   
        // If targetLength is 0, set when target event is ready.
        if( targetLength == 0 && targetFMOD.TargetEventReady() ) {
            targetLength = targetFMOD.GetTargetLength();

            // Set the reference length as 2x the targetLength, to allow room for stretch and shrink.
            parentSampleEvent.SetReferenceLength(targetLength * 2);

            Debug.Log("Setting Ref Length: " + (targetLength*2));
        }
        // Initialize the canvas and other timeline references of sample tracks if needed.
        if (!canvasInitialized) 
        {
            // Set timelines for parent event and each timeline of the parent event's sample tracks.
            parentSampleEvent.SetTimelines(timelineWidth, timelineHeight);

            // set canvas init to true.
            canvasInitialized = true;
            Debug.Log("Setting timelines (w,h): (" + timelineWidth + ", " + timelineHeight + ")");
            Debug.Log("Canvas Initilized (bool) : " + canvasInitialized);


        }
        
        // Check for Target Song inputs.
        CheckTargetInputs();

        // Check for Nested Sample Event inputs.
        CheckSampleInputs();
        
    }

    public void CheckTargetInputs() 
    {
        // Check for SPACE input to toggle play/pause playback.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            targetFMOD.TogglePlayback();
        }
        // Check for S/s key input to stop playback.
        else if (Input.GetKeyDown(KeyCode.S))
        {
            targetFMOD.StopPlayback();
        }
    }

    public void CheckSampleInputs() 
    {
       
        // Check for SPACE input to toggle play/pause playback.
        if (Input.GetKeyDown(KeyCode.P))
        {
            parentSampleEvent.TogglePlayback();
        }
        // Check for S/s key input to stop playback.
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            parentSampleEvent.StopPlayback();

            // Reset cursor position ...
            parentSampleEvent.CursorToOrigin();

        }
        // Check for X key for pitch up.
        else if (Input.GetKey(KeyCode.UpArrow))
        {

            //parentSampleEvent.AdjustPitch(pitchDelta);
        }
        // Check for  key for pitch up.
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            //parentSampleEvent.AdjustPitch(-pitchDelta);
        }       
    }

    

    

}
