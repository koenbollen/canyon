
#include "common.h"

float LiftY = 2;
float2 NewParticle;
float4 NewPosition;

float epsilon = 0.1;

float3 generateNewPosition(float2 uv)
{
	float3 pos = float3(0,0,0);
	if( NewPosition.w != 0 )
		pos = NewPosition.xyz;
	return pos;
}

float4 ResetPositionsPS(in float2 uv : TEXCOORD0) : COLOR
{
	float4 pos = float4(generateNewPosition(uv), 0);
	return pos;
}

float4 ResetVelocitiesPS(in float2 uv : TEXCOORD0) : COLOR
{
	return float4(0,0,0,0);
}

float4 UpdatePositionsPS(in float2 uv : TEXCOORD0) : COLOR
{
	float4 pos = tex2D(positionSampler, uv);
	if ( NewParticle.x + epsilon >= uv.x && NewParticle.x - epsilon <= uv.x  && NewParticle.y + epsilon >= uv.y && NewParticle.y - epsilon <= uv.y )
	{
		pos.xyz = generateNewPosition(uv);
		pos.w = NewPosition.w;
	}
	if (pos.w > 0 )
	{
		float4 velocity = tex2D(velocitySampler, uv);
		pos.xyz += ElapsedTime * velocity;
		pos.w -= ElapsedTime;
	}
	else
	{
		pos.xyzw = 0;
	}
	return pos;
}

float4 UpdateVelocitiesPS(in float2 uv : TEXCOORD0) : COLOR
{
	float4 velocity = tex2D(velocitySampler, uv);
	float4 pos = tex2D(positionSampler, uv);
	if (pos.w == 0)
	{
		float4 rand = tex2D(randomSampler, uv);
		float4 rand2 = tex2D(randomSampler, uv + float2(rand.y,rand.w));
		velocity.xyz = rand2.xyz * .4 + 1.0 * rand.xyz;
		velocity.y = LiftY * 10 * ElapsedTime;
		pos.w = -1;
	}
	else
	{
		velocity.y += LiftY * ElapsedTime;
	}
	return velocity;
}

#include "techniques.h"
