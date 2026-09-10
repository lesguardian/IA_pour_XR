using UnityEngine;

public class Stage4_ColorFilter : MonoBehaviour
{
    WebCamTexture cam;
    DebugTextureView rawView;
    DebugTextureView redView;
    Color32[] output;

    // Centre de teinte (0..1), tolérance autour du centre (0..0.5).
    // Par défaut centre = 0 => rouge, tolérance ~0.05 = ±9°.
    [Range(0f, 1f)]
    public float hueCenter = 0f;
    [Range(0f, 0.5f)]
    public float hueTolerance = 0.05f;

    // Seuils min pour saturation et valeur (0..1) pour éviter faux positifs.
    [Range(0f, 1f)]
    public float minSaturation = 0.4f;
    [Range(0f, 1f)]
    public float minValue = 0.2f;

    void Start()
    {
        cam = new WebCamTexture();
        cam.Play();

        rawView = DebugTextureView.Create("QuadA_Raw", new Vector3(-2.6f, 1, 0), new Vector2(4.5f, 2.7f));
        redView = DebugTextureView.Create("QuadB_RedFilter", new Vector3(2.6f, 1, 0), new Vector2(4.5f, 2.7f));
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

        for (int i = 0; i < pixels.Length; i++)
        {
            Color32 p32 = pixels[i];
            Color p = p32; // conversion implicite vers Color (float)

            // Convertir en HSV
            Color.RGBToHSV(p, out float hHue, out float s, out float v);

            // Calculer distance circulaire entre deux teintes
            float d = Mathf.Abs(hHue - hueCenter);
            d = Mathf.Min(d, 1f - d);

            bool inHue = d <= hueTolerance;
            bool inSatVal = s >= minSaturation && v >= minValue;

            if (inHue && inSatVal)
            {
                // Conserver la couleur d'origine (ou la renforcer)
                output[i] = new Color32(p32.r, p32.g, p32.b, 255);
            }
            else
            {
                // Sinon mettre en noir
                output[i] = new Color32(0, 0, 0, 255);
            }
        }

        rawView.Apply(pixels, w, h);
        redView.Apply(output, w, h);
    }
}
