using System;
using UnityEngine;
using System.Runtime.InteropServices;

class TargetFMOD : MonoBehaviour

{

    ///////////// FMOD Event stuff
    public FMODUnity.EventReference _eventPath;

    private FMOD.Studio.EventInstance _event;
    private FMOD.Channel _channel;
    private FMOD.ChannelGroup _channelGroup;

    public FMOD.DSP_FFT_WINDOW _windowShape = FMOD.DSP_FFT_WINDOW.RECT;

    private FMOD.DSP _fft;

    LineRenderer lineRenderer;
    const int sampleSize = 84;



    void Start()
    {
        // Create the line renderer.
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = sampleSize;
        lineRenderer.startWidth = .1f;
        lineRenderer.endWidth = .1f;

        // Set event instance based on event path.
        _event = FMODUnity.RuntimeManager.CreateInstance(_eventPath);

        // Set the DSP and FFT.
        FMODUnity.RuntimeManager.CoreSystem.createDSPByType(FMOD.DSP_TYPE.FFT, out _fft);
        _fft.setParameterInt((int)FMOD.DSP_FFT.WINDOWTYPE, (int)_windowShape);
        _fft.setParameterInt((int)FMOD.DSP_FFT.WINDOWSIZE, sampleSize * 2);
    }
    

    void Update()
    {
        

        ////////////////////////////////
        // INPUT CHECKS
        ////////////////////////////////

        // Check for SPACE input to toggle play/pause playback.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePlayback();   
        }
        // Check for S/s key input to stop playback.
        else if (Input.GetKeyDown(KeyCode.S))
        {
            StopPlayback();
        }
        

        // If event is playing, and is not paused, update the line renderer based on FFT data.
        _event.getPaused(out Boolean paused);
        if (IsPlaying() && paused == false || IsStopped())
        {
            DrawLineRenderer();
        }


    }

    // Helper function to toggle the play/pause state of event after pressing space bar.
    void TogglePlayback()
    {
        // Check the event is valid.
        if (_event.isValid())
        {
            // If event hasn't started, start the event.
            if (IsStopped())
            {
                Debug.Log("Starting event... \n");

                _event.setPaused(false);
                _event.start();

                // Set the channel group to the event instance group instead of master.
                _event.getChannelGroup(out _channelGroup);
                _channelGroup.addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.HEAD, _fft);
            }
            // If event is playing, pause the event.
            else if ( IsPlaying() )
            {
                Debug.Log("Toggling the pause state. \n");

                _event.getPaused(out Boolean paused);
                _event.setPaused(!paused);
            }
        }
    }

    // Helper function to stop event playback altogether after S/s is pressed.
    void StopPlayback()
    {
        // Check the event is valid.
        if(_event.isValid() )
        {
            // If the event is playing, stop the playback.
            if( IsPlaying())
            {
                Debug.Log("Stopping the event... \n");

                _event.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                //_event.release(); // Don't do this here. Won't allow to play again.

                // Reset line renderer.
                Vector3 pos = transform.position;
                for (int i = 0; i < sampleSize; i++)
                {
                    pos.x += (WIDTH / sampleSize);

                    pos.y = transform.position.y; // offset to have line renderer with transform.
                    lineRenderer.SetPosition(i, pos);
                }
                
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

    // Helper function to get the playback state of the event.
    FMOD.Studio.PLAYBACK_STATE PlaybackState()
    {
        FMOD.Studio.PLAYBACK_STATE pS;
        _event.getPlaybackState(out pS);
        return pS;
    }


    
    const float WIDTH = 3.0f;
    const float HEIGHT = 0.01f;
    // Draws the line renderer of the target event.
    void DrawLineRenderer()
    {
        IntPtr unmanagedData;
        uint length;
        _fft.getParameterData((int)FMOD.DSP_FFT.SPECTRUMDATA, out unmanagedData, out length);
        FMOD.DSP_PARAMETER_FFT fftData = (FMOD.DSP_PARAMETER_FFT)Marshal.PtrToStructure(unmanagedData, typeof(FMOD.DSP_PARAMETER_FFT));
        var spectrum = fftData.spectrum;

        if (fftData.numchannels > 0)
        {
            // Set initial position to transform position of object.
            var pos = transform.position;

            for (int i = 0; i < sampleSize; ++i)
            {
                pos.x += (WIDTH / sampleSize);

                float level = lin2dB(spectrum[0][i]);
                pos.y = (level - 300) * HEIGHT; // offset to have line renderer with transform.

                lineRenderer.SetPosition(i, pos);
            }
            
        }
    }

    

    // Helper function to help clamp spectrum data values.
    float lin2dB(float linear)
    {
        return Mathf.Clamp(Mathf.Log10(linear) * 15.0f, -100.0f, 0.0f);
    }
}
