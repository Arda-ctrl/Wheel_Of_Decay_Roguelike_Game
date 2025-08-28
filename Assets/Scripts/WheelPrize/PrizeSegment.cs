using UnityEngine;

[System.Serializable]
public class PrizeSegment
{
    [Header("Segment Properties")]
    public string segmentName = "Prize Segment";
    public float startAngle = 0f;
    public float endAngle = 60f;
    public Color segmentColor = Color.white;
    
    [Header("Prize Content")]
    public PrizeType prizeType;
    public SegmentData segmentReward; // Mevcut segment sisteminden
    public int resourceAmount = 0; // Para, coin vs.
    public string customRewardText = "";
    
    [Header("Visual")]
    public Sprite segmentIcon;
    
    // Hesaplanmış değerler
    public float AngleSize => endAngle - startAngle;
    public float CenterAngle => startAngle + (AngleSize * 0.5f);
    
    public bool ContainsAngle(float angle)
    {
        // Açıyı 0-360 arasına normalize et
        angle = ((angle % 360f) + 360f) % 360f;
        float start = ((startAngle % 360f) + 360f) % 360f;
        float end = ((endAngle % 360f) + 360f) % 360f;
        
        if (start <= end)
        {
            return angle >= start && angle <= end;
        }
        else
        {
            // Segment 360 dereceden geçiyorsa (örn: 350-30)
            return angle >= start || angle <= end;
        }
    }
}

public enum PrizeType
{
    SegmentReward,  // Mevcut segment sistemi
    Resource,       // Para, coin, vs.
    CustomReward    // Özel ödüller
}
