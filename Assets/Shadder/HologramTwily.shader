Shader "Unlit/HologramGlowFlashSorted"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (0, 1, 1, 1)
        _ScanSpeed ("Scanline Speed", Float) = 3.0
        _ScanFrequency ("Scanline Frequency", Float) = 30.0
        _ScanStrength ("Scanline Strength", Range(0,1)) = 0.4
        _GlowThickness ("Glow Edge Thickness", Float) = 1.5
        _GlowIntensity ("Glow Brightness", Float) = 2.0
        _PulseSpeed ("Pulse Speed", Float) = 2.0
        _Alpha ("Overall Alpha", Range(0,1)) = 0.8
        _Flash ("Flash Multiplier", Float) = 1.0
    }

    SubShader
    {
        // Important for rendering above lower layers
        Tags { 
            "Queue" = "Transparent" // Renders after Geometry (use Overlay if needed)
            "RenderType" = "Transparent" 
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "False"
        }

        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _ScanSpeed;
            float _ScanFrequency;
            float _ScanStrength;
            float _GlowThickness;
            float _GlowIntensity;
            float _PulseSpeed;
            float _Alpha;
            float _Flash;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y;

                fixed4 texCol = tex2D(_MainTex, i.uv);
                float alpha = texCol.a * _Alpha;

                float scan = sin((i.worldPos.y + time * _ScanSpeed) * _ScanFrequency);
                scan = pow(abs(scan), 3.0);
                scan *= _ScanStrength;

                float edge = smoothstep(0.1, 0.0, texCol.a);
                float glow = edge * _GlowIntensity;

                float pulse = 0.9 + 0.1 * sin(time * _PulseSpeed);
                float brightness = (1.0 + scan + glow) * _Flash;

                fixed4 finalCol = texCol * _Color * brightness * pulse;
                finalCol.a = alpha + scan + glow * 0.3;

                return finalCol;
            }
            ENDCG
        }
    }
}
