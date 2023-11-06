#version 400 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec2 textureCoords;

out vec2 passTextureCoords;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;

void main(void)
{	
	gl_Position = projectionMatrix * viewMatrix * vec4(position,1.0);
	passTextureCoords = textureCoords;	
}
