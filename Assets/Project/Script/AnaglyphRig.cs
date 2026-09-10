using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class AnaglyphRig : MonoBehaviour
{
    [Header("Réglages stéréo")]
    public float eyeSeparation = 0.03f;

    [Header("Références")]
    public RawImage displayImage;
    public Shader anaglyphShader;

    Camera camLeft;
    Camera camRight;
    RenderTexture rtLeft;
    RenderTexture rtRight;
    Material anaglyphMat;

    bool initialized = false;

    void Awake()
    {
        if (initialized) return; // sécurité anti-doublon
        initialized = true;

        camLeft = GetComponent<Camera>();

        // Créer un NOUVEL objet vide pour l'œil droit (pas de clone du GameObject entier)
        GameObject rightGO = new GameObject("Main Camera Right (Auto)");
        rightGO.transform.SetParent(camLeft.transform.parent);
        rightGO.transform.position = camLeft.transform.position;
        rightGO.transform.rotation = camLeft.transform.rotation;
        rightGO.transform.localScale = camLeft.transform.localScale;

        camRight = rightGO.AddComponent<Camera>();
        camRight.CopyFrom(camLeft); // copie FOV, clipping planes, culling mask, etc.

        // Décaler les deux caméras symétriquement sur leur axe local X
        camLeft.transform.localPosition -= camLeft.transform.right * (eyeSeparation / 2f);
        camRight.transform.localPosition += camRight.transform.right * (eyeSeparation / 2f);

        // Créer les RenderTextures à la résolution de l'écran
        rtLeft = new RenderTexture(Screen.width, Screen.height, 24);
        rtRight = new RenderTexture(Screen.width, Screen.height, 24);

        camLeft.targetTexture = rtLeft;
        camRight.targetTexture = rtRight;

        // Créer le matériau anaglyphe
        anaglyphMat = new Material(anaglyphShader);
        anaglyphMat.SetTexture("_LeftTex", rtLeft);
        anaglyphMat.SetTexture("_RightTex", rtRight);

        if (displayImage != null)
        {
            displayImage.material = anaglyphMat;
            displayImage.texture = rtLeft;
        }
    }

    void OnDestroy()
    {
        if (rtLeft != null) rtLeft.Release();
        if (rtRight != null) rtRight.Release();
    }
}