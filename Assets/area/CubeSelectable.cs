using UnityEngine;

public class CubeSelectable : MonoBehaviour
{
    public Material outlineMaterial;
    public Material originalMaterial;
    private Renderer myrenderer;

    public bool IsSelected { get; private set; }

    void Start()
    {
        myrenderer = GetComponent<Renderer>();
        originalMaterial = myrenderer.material;
        Initialize();
    }
        public void Initialize()
    {
        myrenderer = GetComponent<Renderer>();
        if (myrenderer != null && originalMaterial == null)
        {
            originalMaterial = myrenderer.material; 
        }
    }


    public void ToggleSelection()
    {
        if (IsSelected)
        {
            Deselect();
        }
        else
        {
            Select();
        }
    }

    public void Select()
    {
        myrenderer.material = outlineMaterial;
        IsSelected = true;
    }

    public void Deselect()
    {
        if (myrenderer != null && originalMaterial != null)
        {
            myrenderer.material = originalMaterial;
        }
        IsSelected = false;
    }
}
