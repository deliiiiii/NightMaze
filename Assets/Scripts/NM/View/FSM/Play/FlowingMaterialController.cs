using UnityEngine;

public class FlowingMaterialController : MonoBehaviour
{
    static readonly int scrollXSpeed = Shader.PropertyToID("_ScrollXSpeed");
    static readonly int scrollYSpeed = Shader.PropertyToID("_ScrollYSpeed");
    static readonly int emissionColor = Shader.PropertyToID("_EmissionColor");
    Renderer targetRenderer;
    Material materialInstance;

    public float ScrollSpeedX = 2.0f;
    public float ScrollSpeedY = 0.0f;
    public Color EmissionColor = Color.cyan;

    public Vector2 CurrentFlowSpeed => new Vector2(ScrollSpeedX, ScrollSpeedY);

    void Start()
    {
        targetRenderer = GetComponent<Renderer>();
        if (targetRenderer != null)
        {
            materialInstance = targetRenderer.material;
        }
    }

    void Update()
    {
        if (materialInstance != null)
        {
            materialInstance.SetFloat(scrollXSpeed, ScrollSpeedX);
            materialInstance.SetFloat(scrollYSpeed, ScrollSpeedY);
            materialInstance.SetColor(emissionColor, EmissionColor);
        }
    }
    
    void OnDestroy()
    {
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
    }
}