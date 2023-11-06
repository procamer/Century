#version 450 core

layout (location = 0) in vec3 Position;
layout (location = 1) in vec3 Normal;
layout (location = 2) in vec2 TexCoord;
layout (location = 3) in vec3 Tangent;
layout (location = 4) in vec3 Bitangent;
layout (location = 5) in vec4 BoneID;
layout (location = 6) in vec4 Weight;

#define MAX_BONE 150
#define MAX_WEIGHTS 4

out vec2 passTextureCoords;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 transformationMatrix;

uniform int maxBoneCount;
uniform mat4 boneTransform[MAX_BONE];

void main()
{
	mat4 boneTransformation = mat4(0.0f);
	vec4 normalizedWeight = normalize(Weight);    
	for(int i =0; i<MAX_WEIGHTS;i++)
		boneTransformation += boneTransform[uint(BoneID[i])] * normalizedWeight[i];	            
	vec4 worldPosition = transformationMatrix * boneTransformation * vec4(Position, 1.0);
	gl_Position = projectionMatrix * viewMatrix * worldPosition;
    passTextureCoords = TexCoord;
}
