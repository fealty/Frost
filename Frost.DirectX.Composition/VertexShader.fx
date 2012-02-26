struct ObjectData
{
// The vertex specifies texture coordinates within the bounds of this rectangle.
float texRectLeft;
float texRectTop;
float texRectRight;
float texRectBottom;

// The vertex specifies position coordinates within the bounds of this rectangle.
float posRectLeft;
float posRectTop;
float posRectRight;
float posRectBottom;

// The transform is specified in screen space.
row_major matrix transform;
};

cbuffer ConstantsPerFrame : register(b0)
{
// The viewport size is needed to properly fill the stencil buffer.
float2 viewportSize : packoffset(c0);

// This matrix converts from screen space to orthographic space.
row_major matrix orthoTransform : packoffset(c1);
} 

cbuffer ConstantsPerDraw : register(b1)
{
// Per-object data is intended to be accessed by the instance ID.
ObjectData instanceData[512] : packoffset(c0);
}

struct PS_IN
{
float4 position : SV_POSITION;
float2 coordinate : TEXCOORD;

uint instanceId : SV_InstanceId;
};

PS_IN Main(float2 position : POSITION,
      float2 coordinate : TEXCOORD,
      uint instanceId : SV_InstanceId)
{
PS_IN output;

output.instanceId = instanceId;
output.position.z = 0.0f;
output.position.w = 1.0f;

// Compute the position and size of the element rectangle.
float elementX = instanceData[instanceId].posRectLeft;
float elementY = instanceData[instanceId].posRectTop;

float elementWidth = instanceData[instanceId].posRectRight - elementX;
float elementHeight = instanceData[instanceId].posRectBottom - elementY;

// Combine the element rectangle with the current vertex position.
output.position.x = elementX + (elementWidth * position.x);
output.position.y = elementY + (elementHeight * position.y);

// Compute the position and size of the texture coordinate rectangle.
float coordX = instanceData[instanceId].texRectLeft;
float coordY = instanceData[instanceId].texRectTop;

float coordWidth = instanceData[instanceId].texRectRight - coordX;
float coordHeight = instanceData[instanceId].texRectBottom - coordY;

// Combine with the current texture coordinate.
output.coordinate.x = coordX + (coordWidth * coordinate.x);
output.coordinate.y = coordY + (coordHeight * coordinate.y);

// Apply the object transformation.
output.position = mul(output.position, instanceData[instanceId].transform);

output.position.x = round(output.position.x);
output.position.y = round(output.position.y);

// The left and top edges of the screen are half the viewport below zero.
output.position.x -= viewportSize.x / 2.0f;
output.position.y -= viewportSize.y / 2.0f;

// Transform the screen space position to orthographic space.
output.position = mul(output.position, orthoTransform);

return output;
}