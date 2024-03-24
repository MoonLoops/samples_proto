// SAMPLE TRACK COMPONENT
// Sample Track - visualizes an FMOD Event and creates a "Track" component sprite.

using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class SampleTrack : MonoBehaviour
{
    [SerializeField]
    private int timelineWidth; // Represents a width that is equal to the reference length (in milliseconds).
    [SerializeField]
    private int timelineHeight;

    private float widthPerMS; // Set by helper function.
    public Color background = Color.black;
    public Color foreground = Color.yellow;

    public Camera cam = null;
    
    private FMOD.Studio.EventInstance sampleEvent;
    private Vector3 origin;


    ///////////// FMOD References 
    

    // Private sprite and waveform fields.
    private SpriteRenderer sprend = null;
    private int samplesize;
    //private byte[] _samples = null;
    private float[] waveform = null;
    private float originOffset;


    private void Start()
    {
        
        // Initialize the sprite renderer.
        sprend = this.GetComponent<SpriteRenderer>();

        // Starts at 0.0f until set by reference length of target event.
        widthPerMS = 0.0f;       
    }

    private void Update()
    {
        
    }

    // Helper function that sets the origin vector.
    public void SetOriginVector(float x, float y, float z)
    {
        origin = new Vector3(x, y, z);
    }

    // Helper function that sets the timelineHeight from parent component.
    public void SetTimelineWidth(int width)
    {
        timelineWidth = width;
    }

    // Helper function that sets the timelineHeight from parent component.
    public void SetTimelineHeight(int height)
    {
        timelineHeight = height;
    }


    // Helper function to set the sample event reference 
    public void SetSampleEvent(FMOD.Studio.EventInstance eventInstance)
    {
        sampleEvent = eventInstance;
    }

    // Helper function to prepare the sample track "waveform" for this an event.
    public void PrepareSampleTrack(int referenceLength = 0)
    {       
        // Continue if the reference length is valid, a.k.a. greater than 0.
        if(referenceLength > 0)
        {   
            // Reference length is in milliseconds, twice the length of target song.

            // Sample Texture Width is length of sample (ms) * widthPerMS
            int sampleTextureWidth = GetSampleTextureWidth();

            // Get the waveform and add it to the sprite renderer.
            Texture2D texwav = GetWaveformFMOD(sampleTextureWidth);
            // Width should be set from the event length, get it from the description.
            Rect rect = new Rect(Vector2.zero, new Vector2(sampleTextureWidth, timelineHeight));
            sprend.sprite = Sprite.Create(texwav, rect, Vector2.zero);

            // Adjust the waveform sprite position.
            transform.position = new Vector3(origin.x, transform.position.y, origin.z);
        }
        
    }

    // Helper function to get the description of the sampleEvent.
    public FMOD.Studio.EventDescription GetSampleDescription() 
    {
        sampleEvent.getDescription(out FMOD.Studio.EventDescription sampleDesc);
        return sampleDesc;
    }

    // Helper function to get the length of the sample event (in milliseconds).
    public int GetSampleLength() 
    {
        FMOD.Studio.EventDescription sampleDesc = GetSampleDescription();
        sampleDesc.getLength(out int sampleLength);

        return sampleLength;
    }

    // Get the width per millisecond.
    public float GetWidthPerMillisecond()
    {
        return widthPerMS;
    }

    // Set the width per millisecond.
    public void SetWidthPerMillisecond(int referenceLength = 0)
    {
        // Proceed if referenceLength is greater than 0.
        if(referenceLength > 0) 
        {
            widthPerMS = ((float)timelineWidth / (float)referenceLength);
            
            Debug.Log("Width [int] : " + timelineWidth);
            Debug.Log("Reference length (ms) :" + referenceLength);
            Debug.Log("Width per ms [float] : " + widthPerMS);
        }
    }

    // Helper function to get the width of the sample texture.
    public int GetSampleTextureWidth() 
    {   
        // Get the length of the event in milliseconds.
        int sampleLength = GetSampleLength();

        // Width of texture should be converted to (int)(sampleLength (ms) * widthPerMS)
        int sampleTextureWidth = (int)(sampleLength * widthPerMS);

        Debug.Log("Sample length (ms) : " + sampleLength);
        Debug.Log("Sample Texture Width [int] : " + sampleTextureWidth);


        return sampleTextureWidth;
    }

    // Creates our representation of a wave form for this sample event.
    public Texture2D GetWaveformFMOD(int sampleTextureWidth)
    {
        int halfheight = timelineHeight / 2;
        float heightscale = (float)halfheight * 0.0025f;

        // get the sound data
        Texture2D tex = new Texture2D(sampleTextureWidth, timelineHeight, TextureFormat.RGBA32, false);
        waveform = new float[sampleTextureWidth];

        // get samples from the helper function.
        samplesize = sampleTextureWidth; // @TODO change this.

        // map the sound data to texture
        // 1 - clear
        for (int x = 0; x < sampleTextureWidth; x++)
        {
            for (int y = 0; y < timelineHeight; y++)
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
