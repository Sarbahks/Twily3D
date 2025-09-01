using UnityEngine;

[ExecuteAlways]
public class BindTopRadiusFromMesh : MonoBehaviour
{
    public Renderer targetRenderer;
    public string radiusProperty = "_TopRadius";

    void Reset()
    {
        targetRenderer = GetComponent<Renderer>();
    }

    void OnEnable() { Apply(); }
    void OnValidate() { Apply(); }
    void Update()
    {
        // Keep in sync if animated scaling
        if (!Application.isPlaying) Apply();
    }

    void Apply()
    {
        if (!targetRenderer) return;
        var mf = GetComponent<MeshFilter>();
        if (!mf || !mf.sharedMesh) return;

        var ext = mf.sharedMesh.bounds.extents;
        float rx = ext.x * transform.lossyScale.x;
        float rz = ext.z * transform.lossyScale.z;
        float radius = Mathf.Max(rx, rz);

        var mpb = new MaterialPropertyBlock();
        targetRenderer.GetPropertyBlock(mpb);
        mpb.SetFloat(radiusProperty, radius);
        targetRenderer.SetPropertyBlock(mpb);
    }
}
