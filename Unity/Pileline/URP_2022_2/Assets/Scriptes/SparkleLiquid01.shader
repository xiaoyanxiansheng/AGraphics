Shader "WalkingFat/LiquidBottle/SparkleLiquid01"
{
    Properties
    {
        _MainColor ("_MainColor", Color) = (1,1,1,1)
        _TopColor ("_TopColor", Color) = (1,1,1,1)
        _FillAmount ("Fill Amount", Range(-1,1)) = 0.0
        _WobbleCount ("Wobble Count", Range(0,1)) = 0.5
        _WobbleHeight ("Wobble Height", Range(0,1)) = 0.5
        _WobbleSpeed ("Wobble Speed", Range(0,1)) = 0.5
        _TopFade ("Top Fade", Range(0,1)) = 0.5
        _TopCount1 ("Top Count1", Range(0,1)) = 0.5
        _TopHeight1 ("Top Height1", Range(0,1)) = 0.5
        _TopSpeed1 ("Top Speed1", Range(0,1)) = 0.5
        _TopCount2 ("Top Count2", Range(0,1)) = 0.5  
        _TopHeight2 ("Top Height2", Range(0,1)) = 0.5   
        _TopSpeed2 ("Top Speed2", Range(0,1)) = 0.5 
        
        _WobbleX ("WobbleX", Range(0,1)) = 0.5
        _WobbleZ ("WobbleZ", Range(-0.2,0.2)) = 0.0
    }
 
    SubShader
    {
        Tags
        { 
            "DisableBatching" = "True" 
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        
        //1st pass draw liquid
        Pass
        {
            Tags {"RenderType" = "Opaque" "Queue" = "Geometry"}
            
            Cull OFF
            AlphaToMask on
            //ZWrite On
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM


            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 positionWS : TEXCOORD1;
            };
            
            float _FillAmount, _WobbleX, _WobbleZ;
            float4 _MainColor, _TopColor;
            float _TopFade;
            float _TopCount1;
            float _TopHeight1;
            float _TopSpeed1;
            float _TopCount2;
            float _TopHeight2;
            float _TopSpeed2;
            float _WobbleCount;
            float _WobbleHeight;
            float _WobbleSpeed;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.positionWS.xyz = mul (unity_ObjectToWorld, v.vertex.xyz);
                o.positionWS.w = 0;
                
                return o;
            }

            half4 frag (v2f i, fixed facing : VFACE) : SV_Target
            {
                float WobbleX = sin(_Time.x * _WobbleSpeed * 100) * _WobbleX * 10;
                float worldPosAdjusted = i.positionWS.y + sin(i.positionWS.x * WobbleX * UNITY_PI * _WobbleCount * 0.4) * _WobbleHeight * 0.5;
                float fillEdge = worldPosAdjusted + _FillAmount;
                // 角度
                float temp = acos(dot(normalize(float2(i.positionWS.xz)),float2(1,0))); 
                float degree1 = cos(temp * _TopCount1 * 20 + _Time.x * _TopSpeed1 * 200) * _TopHeight1 * 0.04;
                float degree2 = cos(temp * _TopCount2 * 20 + _Time.x * _TopSpeed2 * 200) * _TopHeight2 * 0.04;
                fillEdge = saturate(1 - fillEdge + degree1 + degree2);
                // 位移
                float split = pow(fillEdge,_TopFade * 200);
                // Color
                half4 frontColor = split * _MainColor;
                half4 backColor = split * _TopColor;
                return facing > 0 ? frontColor : backColor;
            }
            ENDCG
        }
    }
}