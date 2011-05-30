
float4 CopyTexturePS(in float2 uv : TEXCOORD0) : COLOR
{
	return tex2D(temporarySampler,uv);
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
