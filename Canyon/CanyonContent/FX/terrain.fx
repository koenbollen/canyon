float4x4 World;
float4x4 View;
float4x4 Projection;

// Lighting settings:
float3 LightDirection;
float Ambient;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float3 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float3 Normal : TEXCOORD0;
    float3 Color : COLOR0;
};

VertexShaderOutput JustWhiteVertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.Normal = normalize(input.Normal);
    output.Color = input.Color;

    return output;
}

float4 JustWhitePixelShader(VertexShaderOutput input) : COLOR0
{
	float4 output = float4(input.Color,1);

	float lighting = saturate(dot( input.Normal, -LightDirection ) * .5 + .5) + Ambient;
	output.rgb *= lighting;

    return output;
}

technique Colored
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 JustWhiteVertexShader();
        PixelShader = compile ps_2_0 JustWhitePixelShader();
    }
}
 