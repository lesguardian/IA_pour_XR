using UnityEngine;

public class Stage2_Grayscale : MonoBehaviour
{

    WebCamTexture cam;
    DebugTextureView rawView;
    DebugTextureView grayView;
    Color32[] output;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = new WebCamTexture();
        cam.Play();
        rawView = DebugTextureView.Create("QuadA_Raw", new Vector3(-2.6f,1,0), new Vector2(4.5f,2.7f));
        grayView = DebugTextureView.Create("QuadB_Gray", new Vector3(2.6f,1,0), new Vector2(4.5f,2.7f));
    }

    // Update is called once per frame
    void Update()
    {
        if(!cam.didUpdateThisFrame)return;

        //1. Get the frame.
        Color32[] pixels = cam.GetPixels32();
        int w = cam.width, h = cam.height;
        if(output == null || output.Length != pixels.Length)
        {
            output = new Color32[pixels.Length];
        }

        //2. For every pixel: gray = 0.299*R + 0.587*G + 0.114*B (luminance)
        for(int i = 0; i < pixels.Length; i++)
        {
            Color32 p = pixels[i];
            byte g = (byte)(0.299f * p.r + 0.587f * p.g + 0.114f * p.b);
            output[i] = new Color32(g, g, g, 255);
        }

        //3. Show it on the Quads.
        rawView.Apply(pixels, w, h);
        grayView.Apply(output, w, h);
    }
}
