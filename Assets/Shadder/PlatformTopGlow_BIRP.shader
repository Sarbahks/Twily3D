Shader "Custom/PlatformTopGlow_BIRP"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.3, 0.7, 1.0, 1.0)

        // Cylinder top radius in object space (Unity primitive cylinder = 0.5)
        _TopRadius ("Top Radius (OS)", Float) = 0.5

        // Only the inner portion (0..1 of TopRadius) blends to white
        _WhiteRadius ("Inner White Radius", Range(0.05,1.0)) = 0.35

        // Sharpness of the center gradient
        _TopGradientPower ("Top Gradient Power", Range(0.5, 8.0)) = 3.0

        // Strength of the white blend in the inner area
        _TopWhiteStrength ("Top White Strength", Range(0,1)) = 0.9

        // Makes the whole object emit (so sides also glow)
        _GlobalGlow ("Global Glow Intensity", Range(0,20)) = 2.0
        // Extra emission focused at top center
        _TopGlowBoost ("Top Glow Boost", Range(0,40)) = 10.0

        // Optional view fresnel on sides
        _SideFresnelStrength ("Side Fresnel Strength", Range(0,10)) = 1.0
        _SideFresnelPower ("Side Fresnel Power", Range(0.5,8)) = 2.0

        // PBR sliders
        _Smoothness ("Smoothness", Range(0,1)) = 0.0
        _Metallic  ("Metallic", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 250
        Cull Back
        ZWrite On
        ZTest LEqual
        Blend One Zero

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        fixed4 _BaseColor;
        float  _TopRadius;
        float  _WhiteRadius;
        float  _TopGradientPower;
        float  _TopWhiteStrength;
        float  _GlobalGlow;
        float  _TopGlowBoost;
        float  _SideFresnelStrength;
        float  _SideFresnelPower;
        half   _Smoothness;
        half   _Metallic;

        struct Input
        {
            float3 posOS;
            float3 worldNormal;
            float3 viewDir;
        };

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.posOS = v.vertex.xyz;
            o.worldNormal = UnityObjectToWorldNormal(v.normal);
        }

        // get object axis scales from the object-to-world matrix columns
        float GetScaleX()
        {
            return length(float3(unity_ObjectToWorld._m00, unity_ObjectToWorld._m10, unity_ObjectToWorld._m20));
        }
        float GetScaleZ()
        {
            return length(float3(unity_ObjectToWorld._m02, unity_ObjectToWorld._m12, unity_ObjectToWorld._m22));
        }
        float3 GetObjUpWS()
        {
            return normalize(float3(unity_ObjectToWorld._m01, unity_ObjectToWorld._m11, unity_ObjectToWorld._m21));
        }

        void surf(Input i, inout SurfaceOutputStandard o)
        {
            float3 baseCol = _BaseColor.rgb;

            // object up in world space (top cap direction)
            float3 objUp = GetObjUpWS();

            // top mask: 1 on the top cap, 0 elsewhere
            float3 n = normalize(i.worldNormal);
            float topMask = smoothstep(0.6, 0.98, dot(n, objUp));

            // scale-aware normalized radial distance on the top:
            // treat non-uniform scale as an ellipse and compute r in [0..1]
            float sx = max(GetScaleX(), 1e-5);
            float sz = max(GetScaleZ(), 1e-5);
            float rx = max(_TopRadius * sx, 1e-5);
            float rz = max(_TopRadius * sz, 1e-5);

            float px = i.posOS.x * sx;
            float pz = i.posOS.z * sz;

            // ellipse distance normalization: sqrt((x/rx)^2 + (z/rz)^2)
            float r = sqrt((px * px) / (rx * rx) + (pz * pz) / (rz * rz));
            r = saturate(r);

            // inner radius for white blend (as a fraction of TopRadius)
            float inner = saturate(_WhiteRadius);

            // gradient only inside the inner disk: 1 at center, 0 at r >= inner
            float gInner = pow(saturate(1.0 - r / max(inner, 1e-5)), _TopGradientPower);

            // top color: base -> white only in the inner area
            float3 topCol = lerp(baseCol, 1.0.xxx, _TopWhiteStrength * gInner);

            // final base color
            float3 col = lerp(baseCol, topCol, topMask);

            // emission:
            float glow = _GlobalGlow;                     // uniform glow everywhere
            glow += gInner * topMask * _TopGlowBoost;     // strong at top center

            // optional side fresnel to sell the glow on edges
            float3 V = normalize(i.viewDir);
            float fres = pow(1.0 - saturate(abs(dot(V, n))), _SideFresnelPower) * _SideFresnelStrength;
            glow += fres;

            o.Albedo = col;
            o.Emission = col * glow;  // HDR emission drives bloom
            o.Smoothness = _Smoothness;
            o.Metallic = _Metallic;
            o.Alpha = 1.0;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
