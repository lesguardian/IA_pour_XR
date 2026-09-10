using UnityEngine;

public class FaceTracker : MonoBehaviour
{
    WebCamTexture cam;
    Color32[] pixels;

    [Header("Cibles")]
    public Transform subject;      // Rotation yaw du corps
    public Transform headBone;     // Rotation pitch/yaw fine de la tête

    [Header("Amplitude analogique")]
    public float maxYawAngle = 60f;    // Rotation horizontale max (deg)
    public float maxPitchAngle = 30f;  // Rotation verticale max (deg)
    public float deadZone = 0.03f;     // Zone morte au centre pour éviter le tremblement

    [Header("Profondeur (taille du blob)")]
    public bool useDepth = true;
    public float minBlobRatio = 0.002f; // ratio de pixels rouges = "loin"
    public float maxBlobRatio = 0.08f;  // ratio de pixels rouges = "proche"
    public float depthLeanAngle = 10f;  // inclinaison du buste selon la proximité

    [Header("Lissage")]
    public float yawSpeed = 8f;
    public float pitchSpeed = 8f;

    [Header("Détection couleur rouge (HSV)")]
    [Range(0f, 1f)] public float hueCenter = 0f;
    [Range(0f, 0.5f)] public float hueTolerance = 0.07f;
    [Range(0f, 1f)] public float minSaturation = 0.4f;
    [Range(0f, 1f)] public float minValue = 0.15f;

    public bool flipY = true;
    public bool mirrorX = true; // webcam non-miroir par défaut -> à tester
    public bool holdLastWhenLost = true;

    // Etat analogique courant (lissé)
    float currentYaw = 0f;
    float currentPitch = 0f;
    float currentDepth01 = 0f; // 0 = loin, 1 = proche

    // Rotations de base (pour repartir d'une orientation neutre)
    Quaternion subjectBaseRotation;
    Quaternion headBaseRotation;

    void Start()
    {
        cam = new WebCamTexture();
        cam.Play();

        if (subject != null) subjectBaseRotation = subject.rotation;
        if (headBone != null) headBaseRotation = headBone.rotation;
    }

    void Update()
    {
        if (subject == null || cam == null || !cam.didUpdateThisFrame) return;

        pixels = cam.GetPixels32();
        int w = cam.width, h = cam.height;
        if (pixels == null || pixels.Length == 0) return;

        long sumX = 0, sumY = 0, count = 0;

        for (int i = 0; i < pixels.Length; i++)
        {
            Color p = pixels[i];
            Color.RGBToHSV(p, out float hHue, out float s, out float v);

            float d = Mathf.Abs(hHue - hueCenter);
            d = Mathf.Min(d, 1f - d);

            if (d <= hueTolerance && s >= minSaturation && v >= minValue)
            {
                int px = i % w;
                int py = i / w;
                sumX += px;
                sumY += py;
                count++;
            }
        }

        float targetYaw, targetPitch, targetDepth01;

        if (count > 0)
        {
            float cx = (float)sumX / count;
            float cy = (float)sumY / count;

            // Normaliser en -1..1 par rapport au centre de l'image
            float ux = (cx / w) * 2f - 1f;
            float uy = (cy / h) * 2f - 1f;

            if (mirrorX) ux = -ux;
            if (flipY) uy = -uy;

            // Zone morte
            if (Mathf.Abs(ux) < deadZone) ux = 0f;
            if (Mathf.Abs(uy) < deadZone) uy = 0f;

            targetYaw = ux * maxYawAngle;
            targetPitch = uy * maxPitchAngle;

            // Profondeur estimée via la taille du blob (ratio de pixels rouges)
            float blobRatio = (float)count / (w * h);
            targetDepth01 = Mathf.InverseLerp(minBlobRatio, maxBlobRatio, blobRatio);
            targetDepth01 = Mathf.Clamp01(targetDepth01);
        }
        else
        {
            if (!holdLastWhenLost)
            {
                targetYaw = 0f;
                targetPitch = 0f;
                targetDepth01 = 0f;
            }
            else
            {
                targetYaw = currentYaw;
                targetPitch = currentPitch;
                targetDepth01 = currentDepth01;
            }
        }

        // Lissage analogique (interpolation continue, pas de saut)
        currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, Time.deltaTime * yawSpeed);
        currentPitch = Mathf.LerpAngle(currentPitch, targetPitch, Time.deltaTime * pitchSpeed);
        currentDepth01 = Mathf.Lerp(currentDepth01, targetDepth01, Time.deltaTime * yawSpeed);

        // Applique la rotation yaw au corps
        subject.rotation = subjectBaseRotation * Quaternion.Euler(0f, currentYaw, 0f);

        // Applique yaw + pitch fin à la tête, plus un léger lean selon la profondeur
        if (headBone != null)
        {
            float lean = useDepth ? currentDepth01 * depthLeanAngle : 0f;
            headBone.rotation = headBaseRotation * Quaternion.Euler(currentPitch - lean, currentYaw * 0.3f, 0f);
        }
    }
}