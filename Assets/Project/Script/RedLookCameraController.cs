using System.Collections;
using UnityEngine;

public class RedLookCameraController : MonoBehaviour
{
    [Header("Cible de l'orbite")]
    public Transform target;           // Assigner "Clone" ici
    public float orbitRadius = 5f;      // Distance au sujet (calculée auto au Start si laissé à 0)
    public float heightOffset = 1.5f;   // Hauteur du point regardé sur la cible (ex: niveau de la tête)

    WebCamTexture cam;
    Color32[] pixels;

    [Header("Amplitude du mouvement")]
    public float maxYawAngle = 45f;
    public float maxPitchAngle = 25f;
    public float deadZone = 0.03f;

    [Header("Lissage")]
    public float yawSpeed = 8f;
    public float pitchSpeed = 8f;

    [Header("Détection rouge (HSV)")]
    [Range(0f, 1f)] public float hueCenter = 0f;
    [Range(0f, 0.5f)] public float hueTolerance = 0.07f;
    [Range(0f, 1f)] public float minSaturation = 0.4f;
    [Range(0f, 1f)] public float minValue = 0.15f;

    public bool flipY = true;
    public bool mirrorX = true;
    public bool holdLastWhenLost = true;
    public int pixelStride = 2;

    float currentYaw = 0f;
    float currentPitch = 0f;
    float baseYaw = 0f; // angle de départ (position actuelle du rig autour de la cible)

    void Start()
    {
        string targetDeviceName = "HD User Facing";
        WebCamDevice[] devices = WebCamTexture.devices;

        WebCamDevice? chosen = null;
        foreach (var d in devices)
        {
            if (d.name == targetDeviceName)
            {
                chosen = d;
                break;
            }
        }

        if (chosen.HasValue)
        {
            cam = new WebCamTexture(chosen.Value.name, 640, 480, 30);
        }
        else
        {
            //Debug.LogWarning($"Webcam '{targetDeviceName}' introuvable, utilisation de la première disponible.");
            cam = new WebCamTexture();
        }

        cam.Play();
        StartCoroutine(CheckCamStarted());

        if (target != null)
        {
            Vector3 flatOffset = transform.position - target.position;
            flatOffset.y = 0f;
            if (orbitRadius <= 0f) orbitRadius = flatOffset.magnitude;
            baseYaw = Mathf.Atan2(flatOffset.x, flatOffset.z) * Mathf.Rad2Deg;
        }
    }

    IEnumerator CheckCamStarted()
    {
        float timeout = 5f;
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            if (cam.width > 16) // une vraie image a une largeur correcte ; tant que non initialisée, souvent 16x16
            {
                //Debug.Log($"Webcam démarrée : {cam.deviceName}, résolution {cam.width}x{cam.height}");
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        //Debug.LogError("La webcam n'a jamais démarré correctement après 5 secondes.");
    }

    void Update()
    {
        //if (target == null) { Debug.LogWarning("target est NULL"); return; }
        //if (cam == null) { Debug.LogWarning("cam est NULL"); return; }
        //if (!cam.didUpdateThisFrame) return; // pas de log ici, sinon ça spam à chaque frame

        //Debug.Log($"Frame webcam reçue, count va être calculé...");

        if (target == null || cam == null || !cam.didUpdateThisFrame) return;

        pixels = cam.GetPixels32();
        int w = cam.width, h = cam.height;
        if (pixels == null || pixels.Length == 0) return;

        long sumX = 0, sumY = 0, count = 0;
        

        for (int i = 0; i < pixels.Length; i += pixelStride)
        {
            Color p = pixels[i];
            Color.RGBToHSV(p, out float hh, out float s, out float v);

            float d = Mathf.Abs(hh - hueCenter);
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

        float targetYaw, targetPitch;
        //Debug.Log($"count={count} ux={(count > 0 ? ((float)sumX / count / w) : -1)}");
        if (count > 0)
        {
            float cx = (float)sumX / count;
            float cy = (float)sumY / count;

            float ux = (cx / w) * 2f - 1f;
            float uy = (cy / h) * 2f - 1f;

            if (mirrorX) ux = -ux;
            if (flipY) uy = -uy;

            if (Mathf.Abs(ux) < deadZone) ux = 0f;
            if (Mathf.Abs(uy) < deadZone) uy = 0f;

            targetYaw = ux * maxYawAngle;
            targetPitch = uy * maxPitchAngle;
        }
        else
        {
            targetYaw = holdLastWhenLost ? currentYaw : 0f;
            targetPitch = holdLastWhenLost ? currentPitch : 0f;
        }

        float dt = Mathf.Min(Time.deltaTime, 0.05f);
        currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, dt * yawSpeed);
        currentPitch = Mathf.LerpAngle(currentPitch, targetPitch, dt * pitchSpeed);

        // Calcul de la position sur la sphère autour de la cible
        float totalYaw = baseYaw + currentYaw;
        Quaternion orbitRot = Quaternion.Euler(currentPitch, totalYaw, 0f);
        Vector3 offset = orbitRot * (Vector3.back * orbitRadius);

        Vector3 pivotPoint = target.position + Vector3.up * heightOffset;
        transform.position = pivotPoint + offset;
        transform.LookAt(pivotPoint, Vector3.up);
    }

    void OnDestroy()
    {
        if (cam != null && cam.isPlaying) cam.Stop();
    }
}