
float MaxLife = 5.0f;
float ElapsedTime = 0.0f;
float GravityY = -.984;

texture Noise;
sampler randomSampler : register(s0)  = sampler_state
{
    Texture   = <Noise>;
    MipFilter = None;
    MinFilter = Point;
    MagFilter = Point;
    AddressU  = Wrap;
    AddressV  = Wrap;
};

texture TemporaryMap;
sampler temporarySampler : register(s0)  = sampler_state
{
    Texture   = <TemporaryMap>;
    MipFilter = None;
    MinFilter = Point;
    MagFilter = Point;
    AddressU  = Clamp;
    AddressV  = Clamp;
};
texture PositionMap;
sampler positionSampler  = sampler_state
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

float3 generateNewPosition(float2 uv)
{
	float4 rand = tex2D(randomSampler, uv);
	return float3(rand.x*10,frac( rand.w * 10.2486 ) * 1 + 9,rand.y*10);
}

float4 CopyTexturePS(in float2 uv : TEXCOORD0) : COLOR
{
	return tex2D(temporarySampler,uv);
}

float4 ResetPositionsPS(in float2 uv : TEXCOORD0) : COLOR
{
	float4 pos = float4(generateNewPosition(uv), MaxLife * frac(tex2D(randomSampler, 10.2484 * uv).w));
	pos.y = (tex2D(randomSampler, uv).z * 5 + 5);
	return pos;
}

float4 ResetVelocitiesPS(in float2 uv : TEXCOORD0) : COLOR
{
	return float4(0,0,0,0);
}

float4 UpdatePositionsPS(in float2 uv : TEXCOORD0) : COLOR
{
	float4 pos = tex2D(positionSampler, uv);
	if (pos.w >= MaxLife )
	{
		pos.w = 0;
		pos.xyz = generateNewPosition(uv);
	}
	else
	{
		float4 velocity = tex2D(velocitySampler, uv);
		pos.xyz += ElapsedTime * velocity;
		pos.w += ElapsedTime;
	}
	return pos;
}

float4 UpdateVelocitiesPS(in float2 uv : TEXCOORD0) : COLOR
{
	float4 velocity = tex2D(velocitySampler, uv);
	float4 pos = tex2D(positionSampler, uv);
	if (pos.w >= MaxLife)
	{
		float4 rand = tex2D(randomSampler, uv);
		float4 rand2 = tex2D(randomSampler, uv + float2(rand.y,rand.w));
		velocity.xyz = rand2.xyz * 0.8 + 1.0 * rand.xyz;
	}
	else
	{
		velocity.y += GravityY * ElapsedTime;
	}
	return velocity;
}

technique CopyTexture
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 CopyTexturePS();
	}
}


technique ResetPositions
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 ResetPositionsPS();
	}
}

technique ResetVelocities
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 ResetVelocitiesPS();
	}
}

technique UpdatePositions
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 UpdatePositionsPS();
	}
}

technique UpdateVelocities
{
	pass Pass1
	{
		PixelShader = compile ps_2_0 UpdateVelocitiesPS();
	}
}
