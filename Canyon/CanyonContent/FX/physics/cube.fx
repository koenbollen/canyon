
#include "common.h"

float GravityY = -9.84;

float3 generateNewPosition(float2 uv)
{
	float4 rand = tex2D(randomSampler, uv);
	return float3(rand.x*10,frac( rand.w * 10.2486 ) * 1 + 9,rand.y*10);
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
	if (pos.w <= 0 )
	{
		pos.w = MaxLife;
		pos.xyz = generateNewPosition(uv);
	}
	else
	{
		float4 velocity = tex2D(velocitySampler, uv);
		pos.xyz += ElapsedTime * velocity;
		pos.w -= ElapsedTime;
	}
	return pos;
}

float4 UpdateVelocitiesPS(in float2 uv : TEXCOORD0) : COLOR
{
	float4 velocity = tex2D(velocitySampler, uv);
	float4 pos = tex2D(positionSampler, uv);
	if (pos.w <= 0)
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

#include "techniques.h"