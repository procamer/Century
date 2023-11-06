#version 450 core

in vec2 passTextureCoords;

out vec4 FragColor;

uniform sampler2D textureDiffuse;

void main()
{     
    FragColor = texture(textureDiffuse, passTextureCoords);
}

