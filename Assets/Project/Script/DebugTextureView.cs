using UnityEngine;

// Attach to a Quad. The stage scripts push processed pixels into this each frame
// so the Quad shows what the filter is "seeing". Keeps the algorithm scripts focused.
[RequireComponent(typeof(Renderer))]
public class DebugTextureView : MonoBehaviour
{
    Texture2D tex;

    public void Apply(Color32[] pixels, int width, int height)
    {
        if (tex == null || tex.width != width || tex.height != height)
        {
            tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            GetComponent<Renderer>().material.mainTexture = tex;
        }
        tex.SetPixels32(pixels);
        tex.Apply();
    }

    // Spawn a Quad with an unlit material and a DebugTextureView. Used by each
    // stage script in Start() so the scenes can be just a single GameObject.
    public static DebugTextureView Create(string name, Vector3 position, Vector2 size)
    {
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.name = name;
        quad.transform.position = position;
        quad.transform.localScale = new Vector3(size.x, size.y, 1f);
        Shader sh = Shader.Find("Universal Render Pipeline/Unlit");
        if (sh == null) sh = Shader.Find("Unlit/Texture");
        quad.GetComponent<Renderer>().material = new Material(sh);
        return quad.AddComponent<DebugTextureView>();
    }
}