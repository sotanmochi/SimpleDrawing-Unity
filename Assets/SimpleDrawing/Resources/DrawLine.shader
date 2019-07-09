Shader "SimpleDrawing/DrawLine"
{
    Properties
    {
        [HideInInspector]
        _MainTex ("Texture", 2D) = "white" {}
		[HideInInspector]
		_Thickness ("Thickness", INT) = 1
		[HideInInspector]
		_Color ("Color", VECTOR) = (0,0,0,0)
		[HideInInspector]
		_StartPositionUV ("Start UV Position", VECTOR) = (0,0,0,0)
		[HideInInspector]
		_EndPositionUV ("End UV Position", VECTOR) = (0,0,0,0)
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _Color;
            int _Thickness;
            float4 _StartPositionUV;
            float4 _EndPositionUV;

            bool FloatApproximately(float a, float b)
            {
                return abs(b - a) < (1.0e-3 * max(abs(a), abs(b)));
            }

            bool GreaterThanEqualApproximately(float a, float b)
            {
                return (a > b) || FloatApproximately(a, b);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);


                float4 texelSize = _MainTex_TexelSize;
                float width = texelSize.z;
                float height = texelSize.w;

                float2 p = float2(i.uv.x * width, i.uv.y * height);
                float2 a = float2(_StartPositionUV.x * width, _StartPositionUV.y * height);
                float2 b = float2(_EndPositionUV.x * width, _EndPositionUV.y * height);

                float radius = sqrt(2.0)*_Thickness;
                bool neighborOfStartPosition = distance(p,a) < radius;
                bool neighborOfEndPosition = distance(p,b) < radius;

                if (neighborOfStartPosition || neighborOfEndPosition)
                {
                    col = _Color;
                }

                // *******************************************
                //  Projection of the point p to the line AB
                // *******************************************
                float dot1 = dot(p-a, b-a);
                float dot2 = dot(b-a, b-a);
                float2 q = dot1/dot2 * (b-a) + a;

                // *********************************************************
                //  Solve a system of linear equations using Cramer's rule
                //  The system of linear equations:
                //    qx = s*ax + t*bx
                //    qy = s*ay + t*by
                //    Unknowns: s,t
                // *********************************************************
                float determinant = a.x*b.y - b.x*a.y;
                if (!FloatApproximately(determinant, 0.0))
                {
                    float s = (q.x*b.y - b.x*q.y) / determinant;
                    float t = (a.x*q.y - q.x*a.y) / determinant;

                    // Whether the point (px,py) is on the line segment AB.
                    if (GreaterThanEqualApproximately(s, 0.0) &&
                        GreaterThanEqualApproximately(t, 0.0) &&
                        FloatApproximately((s + t), 1.0))
                    {
                        float d = sqrt((b.y - a.y)*(b.y - a.y) + (b.x - a.x)*(b.x - a.x));
                        if (d > 0.0)
                        {
                            d = 1.0/d * abs((b.y - a.y)*p.x - (b.x - a.x)*p.y + (b.x*a.y - b.y*a.x));
                        }

                        if (d < radius)
                        {
                            col = _Color;
                        }
                    }
                }


                return col;
            }
            ENDCG
        }
    }
}
