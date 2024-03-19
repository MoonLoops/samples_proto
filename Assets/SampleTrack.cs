/**
 * Track FMOD script - visualizes an FMOD Event and creates a "Track" component for playback. 
 **/

using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class SampleTrack : MonoBehaviour
{
    public int width = 1024;
    public int height = 256;
    public Color background = Color.black;
    public Color foreground = Color.yellow;

    public GameObject arrow = null;
    public Camera cam = null;

    ///////////// FMOD DSP stuff
    public FMODUnity.EventReference _sampleInstancePath;
    public int _windowSize = 512;
    public FMOD.DSP_FFT_WINDOW _windowShape = FMOD.DSP_FFT_WINDOW.RECT;

    private FMOD.Studio.EventInstance _sampleInstance;
    private FMOD.Channel _channel;
    private FMOD.ChannelGroup _channelGroup;
    private FMOD.DSP _dsp;
    private FMOD.DSP_PARAMETER_FFT _fftparam;
    private FMOD.DSP _fft;

    // Private sprite and waveform fields.
    private SpriteRenderer sprend = null;
    private int samplesize;
    private byte[] _samples = null;
    private float[] waveform = null;
    private float arrowoffsetx;
    private float originOffset;
    private float arrowOriginOffset;

    float delta = 0.001f;


    private void Start()
    {
        // Initialize the sprite renderer.
        sprend = this.GetComponent<SpriteRenderer>();

        // Prepare FMOD event, sets _sampleInstance.
        PrepareFMODEventInstance();

        // Get the waveform and add it to the sprite renderer.
        Texture2D texwav = GetWaveformFMOD();
        Rect rect = new Rect(Vector2.zero, new Vector2(width, height));
        sprend.sprite = Sprite.Create(texwav, rect, Vector2.zero);


        // Set cursor to origin.
        CursorToOrigin();
        originOffset = Math.Abs(arrow.transform.position.x);
        arrowOriginOffset = originOffset + arrowoffsetx;

        // Adjust the waveform sprite.
        sprend.transform.Translate(Vector3.left * (sprend.size.x / 2f));

    }

    private void Update()
    {
        UpdateCursorPosition();


        ////////////////////////////////
        // INPUT CHECKS
        ////////////////////////////////

        // Check for SPACE input to toggle play/pause playback.
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePlayback();
        }
        // Check for S/s key input to stop playback.
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            StopPlayback();

            // Reset cursor position ...
            CursorToOrigin();

        }
        // Check for X key for pitch up.
        else if (Input.GetKey(KeyCode.UpArrow))
        {

            AdjustPitch(delta);
        }
        // Check for  key for pitch up.
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            AdjustPitch(-delta);
        }        
       
    }

    // Sets the cursor to its origin position.
    void CursorToOrigin()
    {
        // Set event timeline position to 0.
        _sampleInstance.setTimelinePosition(0);

        // Adjust arrow and waveform sprite.
        arrow.transform.position = new Vector3(0f, 0f, 1f);
        arrow.transform.Translate(Vector3.left * (sprend.size.x / 2f));
        arrowoffsetx = -(arrow.GetComponent<SpriteRenderer>().size.x / 2f);
    }

    // Updates the cursor while the event is playing.
    void UpdateCursorPosition()
    {
        
        // Get current position and event length
        _sampleInstance.getTimelinePosition(out int currentPosition); // both in milliseconds
        _sampleInstance.getDescription(out FMOD.Studio.EventDescription eventDescription);
        eventDescription.getLength(out int eventLength); // both in milliseconds

        // Convert milliseconds to seconds for more accurate calculation
        float currPossSeconds = (float)currentPosition / 1000;
        float eventLengthSeconds = (float)eventLength / 1000;

        // Calculate the offset based on the current position and event length
        float xoffset = (currPossSeconds / eventLengthSeconds) * sprend.size.x;
        xoffset -= originOffset; // originOffset is positive.
        //Debug.Log("xoffset: " + xoffset);

        // Update arrow position
        arrow.transform.position = new Vector3(xoffset, arrow.transform.position.y, arrow.transform.position.z);
    }

    ////////////////////// Playback Functions //////////////////////


    // Helper function to toggle the play/pause state of event after pressing space bar.
    void TogglePlayback()
    {
        // Check the event is valid.
        if (_sampleInstance.isValid())
        {
            // If event hasn't started, start the event.
            if (IsStopped())
            {
                Debug.Log("Starting event... \n");

                _sampleInstance.setPaused(false);
                _sampleInstance.start();

                // Set the channel group to the event instance group instead of master.
                _sampleInstance.getChannelGroup(out _channelGroup);
                _channelGroup.addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, _fft);
            }
            // If event is playing, pause the event.
            else if (IsPlaying())
            {
                Debug.Log("Toggling the pause state. \n");

                _sampleInstance.getPaused(out Boolean paused);
                _sampleInstance.setPaused(!paused);
            }
        }
    }

    // Helper function to stop event playback altogether after S/s is pressed.
    void StopPlayback()
    {
        // Check the event is valid.
        if (_sampleInstance.isValid())
        {
            // If the event is playing, stop the playback.
            if (IsPlaying())
            {
                Debug.Log("Stopping the event... \n");

                _sampleInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                //_sample.release(); // Don't do this here. Won't allow to play again.  
            }
            
        }
    }


    // Helper function to tell whether the event playback is playing.
    Boolean IsPlaying()
    {

        return PlaybackState() == FMOD.Studio.PLAYBACK_STATE.PLAYING;
    }

    // Helper function to tell whether the event playback is stopped.
    Boolean IsStopped()
    {

        return PlaybackState() == FMOD.Studio.PLAYBACK_STATE.STOPPED || PlaybackState() == FMOD.Studio.PLAYBACK_STATE.STOPPING;
    }

    Boolean IsPaused()
    {
        _sampleInstance.getPaused(out Boolean paused);

        return paused == true;
    }

    // Helper function to get the playback state of the event.
    FMOD.Studio.PLAYBACK_STATE PlaybackState()
    {
        FMOD.Studio.PLAYBACK_STATE pS;
        _sampleInstance.getPlaybackState(out pS);
        return pS;
    }


    // Helper function to pitch event up by 0.1f
    void AdjustPitch(float delta)
    {
        _sampleInstance.getPitch(out float currPitch);
        _sampleInstance.setPitch(currPitch + delta);

    }

    float GetPitch()
    {
        _sampleInstance.getPitch(out float currPitch);
        Debug.Log("Current Pitch: " + currPitch);
        return currPitch;
        
    }
    ////////////////////// Event Prepare and Waveform //////////////////////


    // Prepare FMOD Event Instance.
    private void PrepareFMODEventInstance()
    {
        // Create the event instance from the event path, add 3D sound and start.
        _sampleInstance = FMODUnity.RuntimeManager.CreateInstance(_sampleInstancePath);
        _sampleInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject.transform));

        // Create the FFT dsp, set window type, and window size.
        FMODUnity.RuntimeManager.CoreSystem.createDSPByType(FMOD.DSP_TYPE.FFT, out _dsp);
        _dsp.setParameterInt((int)FMOD.DSP_FFT.WINDOWTYPE, (int)_windowShape);
        _dsp.setParameterInt((int)FMOD.DSP_FFT.WINDOWSIZE, _windowSize * 2);

    }

    
    private Texture2D GetWaveformFMOD()
    {
        int halfheight = height / 2;
        float heightscale = (float)halfheight * 0.0025f;

        // get the sound data
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        waveform = new float[width];

        // get samples from the helper function.
        //_samples = 
        samplesize = _windowSize; // @TODO change this.

        // map the sound data to texture
        // 1 - clear
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tex.SetPixel(x, y, background);
            }
        }


        tex.Apply();

        // Debug log to check if the texture is being created and modified as expected
        UnityEngine.Debug.Log("Waveform texture created: " + tex.width + "x" + tex.height);

        return tex;

    }


 

}
