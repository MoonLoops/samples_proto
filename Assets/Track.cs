using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Sprite))]
public class Track : MonoBehaviour
{
    public int width = 1024;
    public int height = 64;
    public Color background = Color.black;
    public Color foreground = Color.yellow;
    public GameObject arrow = null;
    public Camera cam = null;

    private AudioSource aud = null;
    private SpriteRenderer sprend = null;
    private int samplesize;
    private float[] samples = null;
    private float[] waveform = null;
    private float arrowoffsetx;

    private void Start()
    {
        // Reference components on the gameobject
        aud = this.GetComponent<AudioSource>();
        sprend = this.GetComponent<SpriteRenderer>();

        Texture2D texwav = GetWaveform();
        Rect rect = new Rect(Vector2.zero, new Vector2(width, height));
        sprend.sprite = Sprite.Create(texwav, rect, Vector2.zero);

        arrow.transform.position = new Vector3(0f, 0f);
        arrowoffsetx = -(arrow.GetComponent<SpriteRenderer>().size.x / 2f);

        cam.transform.position = new Vector3(0f, 0f, -1f);
        cam.transform.Translate(Vector3.right * (sprend.size.x / 2f));
    }
    private void Update()
    {
        // move the arrow
        float xoffset = (aud.time / aud.clip.length) * sprend.size.x;
        arrow.transform.position = new Vector3(xoffset + arrowoffsetx, 0);
    }

    private Texture2D GetWaveform()
    {
        int halfheight = height / 2;
        float heightscale = (float)height * 0.75f;

        // get the sound data
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        waveform = new float[width];

        samplesize = aud.clip.samples * aud.clip.channels;
        samples = new float[samplesize];
        aud.clip.GetData(samples, 0);

        // Debug log to check if the AudioClip is valid and has data
        Debug.Log("AudioClip length: " + aud.clip.length + ", channels: " + aud.clip.channels + ", samples: " + aud.clip.samples);

        int packsize = (samplesize / width);
        for (int w = 0; w < width; w++)
        {
            waveform[w] = Mathf.Abs(samples[w * packsize]);
        }

        // Debug log to check the dimensions and content of the waveform array
        Debug.Log("Waveform array length: " + waveform.Length);

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
        Debug.Log("Waveform texture created: " + tex.width + "x" + tex.height);

        return tex;
    }

}
