Shader "Custom/PrizeWheelShader"
{
    Properties
    {
        _MainTex ("Wheel Texture", 2D) = "white" {}
        _LineColor ("Line Color", Color) = (0, 0, 0, 1)
        _LineWidth ("Line Width", Range(0.001, 0.1)) = 0.01
        
        // Segment colors (maksimum 10 segment)
        _SegmentColor1 ("Segment Color 1", Color) = (1, 0, 0, 1)
        _SegmentColor2 ("Segment Color 2", Color) = (0, 1, 0, 1)
        _SegmentColor3 ("Segment Color 3", Color) = (0, 0, 1, 1)
        _SegmentColor4 ("Segment Color 4", Color) = (1, 1, 0, 1)
        _SegmentColor5 ("Segment Color 5", Color) = (1, 0, 1, 1)
        _SegmentColor6 ("Segment Color 6", Color) = (0, 1, 1, 1)
        _SegmentColor7 ("Segment Color 7", Color) = (1, 0.5, 0, 1)
        _SegmentColor8 ("Segment Color 8", Color) = (0.5, 1, 0, 1)
        _SegmentColor9 ("Segment Color 9", Color) = (0, 0.5, 1, 1)
        _SegmentColor10 ("Segment Color 10", Color) = (1, 1, 1, 1)
        
        // Segment angles (start, end pairs)
        _SegmentAngles ("Segment Angles", Vector) = (0, 0, 0, 0)
        _SegmentCount ("Segment Count", Int) = 0
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _LineColor;
            float _LineWidth;
            
            // Segment properties
            fixed4 _SegmentColor1, _SegmentColor2, _SegmentColor3, _SegmentColor4, _SegmentColor5;
            fixed4 _SegmentColor6, _SegmentColor7, _SegmentColor8, _SegmentColor9, _SegmentColor10;
            float4 _SegmentAngles;
            int _SegmentCount;
            
            // Segment angle arrays (will be set from C#)
            uniform float _StartAngles[10];
            uniform float _EndAngles[10];
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 GetSegmentColor(int index)
            {
                if (index == 0) return _SegmentColor1;
                if (index == 1) return _SegmentColor2;
                if (index == 2) return _SegmentColor3;
                if (index == 3) return _SegmentColor4;
                if (index == 4) return _SegmentColor5;
                if (index == 5) return _SegmentColor6;
                if (index == 6) return _SegmentColor7;
                if (index == 7) return _SegmentColor8;
                if (index == 8) return _SegmentColor9;
                return _SegmentColor10;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // UV'yi merkez koordinatına çevir (-0.5 to 0.5)
                float2 center = i.uv - 0.5;
                
                // Dairesel mesafe kontrolü
                float distance = length(center);
                if (distance > 0.5)
                    discard; // Daire dışını çiz
                
                // Açıyı hesapla (0-360 derece) - Unity koordinat sistemi için düzeltildi
                // atan2 -180 ile +180 arası döner, biz 0-360 istiyoruz
                // Unity'de yukarı 90 derece, sağa 0 derece
                float angle = atan2(center.y, center.x) * 180.0 / 3.14159;
                // Unity koordinat sistemini bizim sistemimize çevir:
                // Unity: yukarı 90°, sağa 0°, aşağı -90°, sola 180°
                // Bizim: yukarı 0°, sağa 90°, aşağı 180°, sola 270°
                angle = (90.0 - angle + 360.0) % 360.0; // Saat yönü (clockwise)
                
                // Base texture
                fixed4 baseColor = tex2D(_MainTex, i.uv);
                
                // Hangi segment'te olduğumuzu bul
                fixed4 segmentColor = baseColor;
                for (int seg = 0; seg < _SegmentCount && seg < 10; seg++)
                {
                    float startAngle = _StartAngles[seg];
                    float endAngle = _EndAngles[seg];
                    
                    bool inSegment = false;
                    if (startAngle <= endAngle)
                    {
                        inSegment = (angle >= startAngle && angle <= endAngle);
                    }
                    else
                    {
                        // 360 dereceden geçen segment
                        inSegment = (angle >= startAngle || angle <= endAngle);
                    }
                    
                    if (inSegment)
                    {
                        segmentColor = GetSegmentColor(seg);
                        break;
                    }
                }
                
                // Çizgiler artık ayrı LineRenderer'larla çiziliyor
                
                // Base texture ile segment rengini karıştır
                return segmentColor * baseColor.a;
            }
            ENDCG
        }
    }
}
