using UnityEngine;

public class Actor_ChangeMaterial : MonoBehaviour
{
    public Renderer TargetRenderer;
    public Material HighlightMaterial;

    private Material originalMaterial;

    private void Awake()
    {
        if (TargetRenderer != null)
            originalMaterial = TargetRenderer.material;
    }

    public void Highlight(GameObject sender)
    {
        if (TargetRenderer != null && HighlightMaterial != null)
            TargetRenderer.material = HighlightMaterial;
    }

    public void Unhighlight(GameObject sender)
    {
        if (TargetRenderer != null)
            TargetRenderer.material = originalMaterial;
    }
}