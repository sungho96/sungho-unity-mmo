Shader "Custom/PointCloudShader"
{
    Properties
    {
        _PointSize ("Point Size", Float) = 5.0
        _Rotation ("Rotation", Vector) = (0,0,0)
        _Position ("Position", Vector) = (0,0,0)
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" } 
        Pass
        {
            Cull Off
            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma require geometry
            #pragma target 4.0  // ✅ 안드로이드 & 리눅스 지원을 위해 4.0으로 변경
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct PointData
            {
                float3 position;
                float4 color;
            };

            StructuredBuffer<PointData> pointBuffer;
            float _PointSize;
            float3 _Rotation;  
            float3 _Position;  

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
            };

            v2f vert(uint id : SV_VertexID)
            {
                v2f o;
                PointData p = pointBuffer[id];

                float3 finalPos = p.position + _Position;
                o.pos = UnityObjectToClipPos(float4(finalPos, 1.0));
                o.color = p.color;

                return o;
            }

            [maxvertexcount(1)]
            void geom(point v2f input[1], inout PointStream<v2f> output)
            {
                #ifdef SHADER_API_OPENGL
                    gl_PointSize = _PointSize;  // OpenGL에서는 반드시 geometry 쉐이더에서 설정해야 함
                #endif
                output.Append(input[0]);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
