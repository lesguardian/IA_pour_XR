using UnityEditor.Profiling;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Stage1_GrabImage : MonoBehaviour
{


    WebCamTexture cam;
    DebugTextureView rawView;
    float nextLogTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        cam = new WebCamTexture();
        cam.Play();
        rawView = DebugTextureView.Create("QuadA_Raw", new Vector3(0,1,0), new Vector2(5,3));
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!cam.didUpdateThisFrame)
        {
            return;
        }

        // 1. Pull the frame as an array of (R,G,B,A) bytes.
        Color32[] pixels = cam.GetPixels32();
        int w = cam.width, h = cam.height;

        // 2. Show it on the Quad.
        rawView.Apply(pixels, w, h);


        // 3. Once per second, print one pixel so students see "it's just numbers".
        if(Time.time>= nextLogTime) {
            Color32 p = pixels[(h/2) * w + (w/2)];
            Debug.Log($"Center pixel: R={p.r}, G={p.g}, B={p.b}");
            nextLogTime = Time.time + 1f; // Log every second
        }
    }
}
