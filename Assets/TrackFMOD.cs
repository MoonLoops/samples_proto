/**
 * Track FMOD script - visualizes an FMOD Event and creates a "Track" component for playback. 
 **/

using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class TrackFMOD : MonoBehaviour
{
    public int width = 1024;
    public int height = 256;
    public Color background = Color.black;
    public Color foreground = Color.yellow;
    
    public GameObject arrow = null;
    public Camera cam = null;

    ///////////// FMOD DSP stuff
    public FMODUnity.EventReference _eventPath;
    public int _windowSize = 512;
    public FMOD.DSP_FFT_WINDOW _windowShape = FMOD.DSP_FFT_WINDOW.RECT;

    private FMOD.Studio.EventInstance _event;
    private FMOD.Channel _channel;
    private FMOD.ChannelGroup _channelGroup;
    private FMOD.DSP _dsp;
    private FMOD.DSP_PARAMETER_FFT _fftparam;

    ///////////// FMOD sound
    private FMOD.Sound _sound;

    // Private sprite and waveform fields.
    private SpriteRenderer sprend = null;
    private int samplesize;
    private byte[] samples = null;
    private float[] waveform = null;
    private float arrowoffsetx;

    public float[] _samples;

    private void Start()
    {
        // Initialize the sprite renderer.
        sprend = this.GetComponent<SpriteRenderer>();

        //Prepare FMOD event, sets _event.
        PrepareFMODEventInstance();

        _samples = new float[_windowSize];

        // Get the waveform and add it to the sprite renderer.
        Texture2D texwav = GetWaveformFMOD();
        Rect rect = new Rect(Vector2.zero, new Vector2(width, height));
        sprend.sprite = Sprite.Create(texwav, rect, Vector2.zero);

        _sound.setMode(FMOD.MODE.LOOP_NORMAL);

        // Get the master channel group
        //FMODUnity.RuntimeManager.CoreSystem.getMasterChannelGroup(out _channelGroup);

        // Play the sound on the master channel group
        FMOD.RESULT result = FMODUnity.RuntimeManager.CoreSystem.playSound(_sound, _channelGroup, false, out _channel);
        if (result != FMOD.RESULT.OK)
        {
            UnityEngine.Debug.LogError("Failed to play sound: " + result);
        }


        // Adjust arrow and waveform sprite.
        arrow.transform.position = new Vector3(0f, 0f, 1f);
        arrow.transform.Translate(Vector3.left * (sprend.size.x / 2f));
        arrowoffsetx = -(arrow.GetComponent<SpriteRenderer>().size.x / 2f);

        sprend.transform.Translate(Vector3.left * (sprend.size.x / 2f));

    }

    private void Update()
    {

    }

    // Prepare FMOD Event Instance.
    private void PrepareFMODEventInstance()
    {
        // Create the event instance from the event path, add 3D sound and start.
        _event = FMODUnity.RuntimeManager.CreateInstance(_eventPath);
        _event.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject.transform));
        //_event.start();

        // Create the FFT dsp, set window type, and window size.
        FMODUnity.RuntimeManager.CoreSystem.createDSPByType(FMOD.DSP_TYPE.FFT, out _dsp);
        _dsp.setParameterInt((int)FMOD.DSP_FFT.WINDOWTYPE, (int)_windowShape);
        _dsp.setParameterInt((int)FMOD.DSP_FFT.WINDOWSIZE, _windowSize * 2);

        // Get the channel group from the event and add to DSP.
        _event.getChannelGroup(out _channelGroup);
        _channelGroup.addDSP(0, _dsp);
    }


    private Texture2D GetWaveformFMOD()
    {
        int halfheight = height / 2;
        float heightscale = (float)halfheight * 0.0025f;

        // get the sound data
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        waveform = new float[width];

        // get samples from the helper function.
        samples = GetSampleData("Assets/music 1.mp3");
        samplesize = samples.Length;


        // Debug log to check if the is valid and has data
        UnityEngine.Debug.Log("Samples: " + samples);

        int packsize = (samplesize / width);
        for (int w = 0; w < width; w++)
        {
            waveform[w] = Mathf.Abs(samples[w * packsize]);
        }

        // Debug log to check the dimensions and content of the waveform array
        UnityEngine.Debug.Log("Waveform array length: " + waveform.Length);

        // map the sound data to texture
        // 1 - clear
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tex.SetPixel(x, y, background);
            }
        }

        // 2 - plot
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < waveform[x] * heightscale; y++)
            {
                tex.SetPixel(x, halfheight + y, foreground);
                tex.SetPixel(x, halfheight - y, foreground);
            }
        }

        tex.Apply();

        // Debug log to check if the texture is being created and modified as expected
        UnityEngine.Debug.Log("Waveform texture created: " + tex.width + "x" + tex.height);

        return tex;

    }


    /// <summary>
    /// Return sample data in a byte array from an audio source using its file path 
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private byte[] GetSampleData(string filePath)
    {
        // Very useful tool for debugging FMOD function calls
        FMOD.RESULT result;

        // Creating the sound using the file path of the audio source 
        // Make sure to create the sound using the MODE.CREATESAMEPLE | MDOE.OPENONLY so the sample data can be retrieved
        result = FMODUnity.RuntimeManager.CoreSystem.createSound(filePath, FMOD.MODE.CREATESAMPLE | FMOD.MODE.OPENONLY, out _sound);

        // Debug the results of the FMOD function call to make sure it got called properly
        if (result != FMOD.RESULT.OK)
        {
            UnityEngine.Debug.Log("Failed to create sound with the result of: " + result);
            return null;
        }

        // Retrieving the length of the sound in milliseconds to size the arrays correctly
        result = _sound.getLength(out uint length, FMOD.TIMEUNIT.MS);

        if (result != FMOD.RESULT.OK)
        {
            UnityEngine.Debug.Log("Failed to retrieve the length of the sound with result: " + result);
            return null;
        }
    
        // Creating the return array which will have the sample data is a readable variable type 
        // Using the length of the sound to create it to the right size 
        byte[] byteArray = new byte[(int)length];

        // Retrieving the sample data to the pointer using the full length of the sound 
        result = _sound.readData(byteArray);

        if (result != FMOD.RESULT.OK)
        {
            UnityEngine.Debug.Log("Failed to retrieve data from sound: " + result);
            return null;
        }

        UnityEngine.Debug.Log("Returning byte array of samples, result: " + result);

        //Returning the array populated with the sample data to be used
        return byteArray;
    }


}
