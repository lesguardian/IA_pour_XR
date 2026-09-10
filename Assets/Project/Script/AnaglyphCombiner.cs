using UnityEngine;

public class AnaglyphCombiner : MonoBehaviour
{
    public RenderTexture leftEye;
    public RenderTexture rightEye;
    public Material anaglyphMaterial;

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        anaglyphMaterial.SetTexture("_LeftTex", leftEye);
        anaglyphMaterial.SetTexture("_RightTex", rightEye);
        Graphics.Blit(src, dst, anaglyphMaterial);
    }
}