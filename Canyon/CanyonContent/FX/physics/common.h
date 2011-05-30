
float ElapsedTime = 0.0;
float MaxLife = 5.0;

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

