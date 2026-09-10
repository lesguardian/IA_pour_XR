using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Stage3_Threshold : MonoBehaviour
{
    WebCamTexture cam;
    DebugTextureView rawView;
    DebugTextureView threshView;
    DebugTextureView grayView;
    Color32[] output;

    // Seuil adjustable depuis l'inspecteur (0..255). Par défaut milieu.
    public int threshold = 128;

    void Start()
    {
        cam = new WebCamTexture();
        cam.Play();

        rawView = DebugTextureView.Create("QuadA_Raw", new Vector3(-2.6f, 1, 0), new Vector2(4.5f, 2.7f));
        threshView = DebugTextureView.Create("QuadB_Threshold", new Vector3(2.6f, 1, 0), new Vector2(4.5f, 2.7f));
        grayView = DebugTextureView.Create("QuadB_Gray", new Vector3(2.6f, 1, 0), new Vector2(4.5f, 2.7f));
    }

    void Update()
    {
        if (!cam.didUpdateThisFrame) return;

        Color32[] pixels = cam.GetPixels32();
        int w = cam.width, h = cam.height;

        if (output == null || output.Length != pixels.Length)
        {
            output = new Color32[pixels.Length];
        }

        // Calculer la luminance et appliquer le seuil inversé :
        // si luminance < threshold => blanc, sinon => noir
        //2. For every pixel: gray = 0.299*R + 0.587*G + 0.114*B (luminance)
        for (int i = 0; i < pixels.Length; i++)
        {
            Color32 p = pixels[i];
            byte g = (byte)(0.299f * p.r + 0.587f * p.g + 0.114f * p.b);
            output[i] = new Color32(g, g, g, 255);
        }
        grayView.Apply(output, w, h);

        for (int i = 0; i < pixels.Length; i++)
        {
            Color32 p = pixels[i];
            byte lum = (byte)(0.299f * p.r + 0.587f * p.g + 0.114f * p.b);

            if (lum < threshold)
                output[i] = new Color32(255, 255, 255, 255);
            else
                output[i] = new Color32(0, 0, 0, 255);
        }

        rawView.Apply(pixels, w, h);
        threshView.Apply(output, w, h);
    }
}
