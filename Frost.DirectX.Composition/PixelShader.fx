Texture2D tex2D;
SamplerState linearSampler;

struct ObjectData
{
  float opacity;
};

cbuffer PSConstants : register(b6)
{
  ObjectData mInstanceData[512] : packoffset(c0);
};

struct PS_IN
{
  float4 pos : SV_POSITION;
  float2 tex : TEXCOORD;

  uint instanceId : SV_InstanceId;
};

float4 Main(PS_IN input) : SV_Target
{
  float4 color = tex2D.Sample(linearSampler, input.tex);

  color = color * mInstanceData[input.instanceId].opacity;

  return color;
}