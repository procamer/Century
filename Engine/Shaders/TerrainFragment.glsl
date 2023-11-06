#version 400 core

in vec2 passTextureCoords;

out vec4 color;

uniform sampler2D textureTerrain;

void main(void)
{
	color = texture(textureTerrain, passTextureCoords * 40);
}