#version 400 core

#define MAX_SHADOW_CASTERS 16

in vec2 passTextureCoords;
in vec3 passWorldPosition;

out vec4 color;

uniform sampler2D textureTerrain;

uniform vec3 lightDirection; // toward the sun
uniform vec3 lightColor;
uniform vec3 ambientSky;

// Soft blob shadows under objects: xy = world X/Z of the center, z = radius, w = darkness (0..1)
uniform vec4 shadowCasters[MAX_SHADOW_CASTERS];
uniform int shadowCasterCount;

void main(void)
{
	float shade = 1.0;
	for (int i = 0; i < shadowCasterCount; i++)
	{
		vec4 caster = shadowCasters[i];
		float distance = length(passWorldPosition.xz - caster.xy) / caster.z;
		shade *= 1.0 - caster.w * (1.0 - smoothstep(0.3, 1.0, distance));
	}

	// The terrain is a flat plane facing straight up
	float diffuse = max(lightDirection.y, 0.0);
	vec4 albedo = texture(textureTerrain, passTextureCoords * 40);
	color = vec4(albedo.rgb * (ambientSky + lightColor * diffuse) * shade, albedo.a);
}
