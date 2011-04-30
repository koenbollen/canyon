float4x4 World;
float4x4 View;
float4x4 Projection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
};

VertexShaderOutput JustWhiteVertexShader(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    return output;
}

float4 JustWhitePixelShader(VertexShaderOutput input) : COLOR0
{
    return float4(1, 1, 1, 1);
}

technique JustWhite
{
    pass Pass1
    {
        VertexShader = compile vs_2_0 JustWhiteVertexShader();
        PixelShader = compile ps_2_0 JustWhitePixelShader();
    }
}
