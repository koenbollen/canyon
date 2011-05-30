
float4x4 World : WORLD;
float4x4 View : VIEW;
float4x4 Projection : PROJECTION;
float3 CameraPosition : POSITION;

float SizeModifier = 1;
float MaxLife = 5.0;
float FadeAlpha = 0.0;

texture ParticleTexture;
sampler particleSampler = sampler_state
{
    Texture = <ParticleTexture>;
    AddressU  = CLAMP;
    AddressV  = CLAMP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

texture PositionMap;
sampler positionSampler = sampler_state
{
    Texture   = <PositionMap>;
    MipFilter = None;
    MinFilter = Point;
    MagFilter = Point;
    AddressU  = Clamp;
    AddressV  = Clamp;
};

texture VelocityMap;
sampler velocitySampler = sampler_state
{
    Texture   = <VelocityMap>;
    MipFilter = None;
    MinFilter = Point;
    MagFilter = Point;
    AddressU  = Clamp;
    AddressV  = Clamp;
};

struct TransformInput
{
    float4 VertexData : POSITION0;
    float4 Color : COLOR0;
	float3 TexCoord : TEXCOORD0;
};

struct TransformOutput
{    
	float4 Position : POSITION0;
    float4 Color : COLOR0;
	float3 TexCoord : TEXCOORD0;
};

struct PixelShaderInput
{    
    float4 Color : COLOR0;
	float2 TexCoord : TEXCOORD0;
};

TransformOutput Transform(TransformInput input)
{
    TransformOutput output = (TransformOutput)0;

	float4x4 viewProjection = mul(View, Projection);


	float4 realPosition = tex2Dlod( positionSampler, float4(input.VertexData.x, input.VertexData.y,0,0) );
	if( realPosition.w <= 0 )
		return output;
		
	float age = realPosition.w;
	realPosition.w = 1; // not needed
	realPosition = mul(realPosition, World);

	float3 normal = normalize(CameraPosition - realPosition);
	float3 size0 = normalize(float3(normal.y, -normal.x, 0));
	float3 size1 = cross(normal,size0)  * SizeModifier;
	size0 *= SizeModifier;
	if( input.VertexData.z == 0 )
	{
		realPosition.xyz += size0;
		realPosition.xyz += size1;
	}
	if( input.VertexData.z == 1 )
	{
		realPosition.xyz += size0;
		realPosition.xyz -= size1;
	}
	if( input.VertexData.z == 2 )
	{
		realPosition.xyz -= size0;
		realPosition.xyz -= size1;
	}
	if( input.VertexData.z == 3 )
	{
		realPosition.xyz -= size0;
		realPosition.xyz += size1;
	}

	output.Position = mul(realPosition, viewProjection);
	
	output.TexCoord = input.TexCoord;
	output.Color = input.Color;
	if( FadeAlpha > 0 )
		output.Color.a *= clamp(age/FadeAlpha, 0, 1);

	return output;
}

float4 ApplyTexture(PixelShaderInput input) : COLOR0
{           
	float4 output=tex2D(particleSampler, input.TexCoord) * input.Color;
	return output * input.Color.a;
}

technique TransformAndTexture
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 Transform();
        PixelShader = compile ps_3_0 ApplyTexture();
    }
}
