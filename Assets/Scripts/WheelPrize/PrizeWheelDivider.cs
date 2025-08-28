using UnityEngine;

public class PrizeWheelDivider : MonoBehaviour
{
    [Header("Divider Settings")]
    public LineRenderer lineRenderer;
    public float innerRadius = 0.1f;
    public float outerRadius = 0.5f;
    public float lineWidth = 0.02f;
    public Color lineColor = Color.black;
    
    private float angle;
    
    public void SetupDivider(float dividerAngle)
    {
        angle = dividerAngle;
        
        // LineRenderer yoksa oluştur
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }
        
        // LineRenderer ayarları
        lineRenderer.material = CreateLineMaterial();
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = false;
        lineRenderer.sortingOrder = 1; // Çarkın üstünde görünsün
        
        UpdateLinePosition();
    }
    
    void UpdateLinePosition()
    {
        // Açıyı radyana çevir
        float radians = angle * Mathf.Deg2Rad;
        
        // İç ve dış noktaları hesapla
        Vector3 innerPoint = new Vector3(
            Mathf.Cos(radians) * innerRadius,
            Mathf.Sin(radians) * innerRadius,
            0
        );
        
        Vector3 outerPoint = new Vector3(
            Mathf.Cos(radians) * outerRadius,
            Mathf.Sin(radians) * outerRadius,
            0
        );
        
        // LineRenderer pozisyonlarını ayarla
        lineRenderer.SetPosition(0, innerPoint);
        lineRenderer.SetPosition(1, outerPoint);
    }
    
    Material CreateLineMaterial()
    {
        // Basit unlit material oluştur
        Material lineMat = new Material(Shader.Find("Sprites/Default"));
        lineMat.color = lineColor;
        return lineMat;
    }
    
    public void SetAngle(float newAngle)
    {
        angle = newAngle;
        UpdateLinePosition();
    }
    
    public void SetColor(Color color)
    {
        lineColor = color;
        if (lineRenderer != null && lineRenderer.material != null)
        {
            lineRenderer.material.color = color;
        }
    }
    
    public void SetWidth(float width)
    {
        lineWidth = width;
        if (lineRenderer != null)
        {
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
        }
    }
}
