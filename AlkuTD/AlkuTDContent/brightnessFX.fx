
float value;
sampler texSampler : register(s0);

float4 Brightness(float2 coord:TEXCOORD) : COLOR0
{
	float4 color = tex2D(texSampler, coord.xy);
	color.rgba *= value;
    return color;
}

technique Brighten
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 Brightness();
    }
}
