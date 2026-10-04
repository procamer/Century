#version 450 core

in vec2 passTextureCoords;
in vec3 passNormal;

out vec4 FragColor;

uniform sampler2D textureDiffuse;

uniform vec3 lightDirection; // toward the sun
uniform vec3 lightColor;
uniform vec3 ambientSky;     // light on surfaces facing up
uniform vec3 ambientGround;  // light on surfaces facing down

void main()
{
    vec3 normal = normalize(passNormal);
    vec3 ambient = mix(ambientGround, ambientSky, normal.y * 0.5 + 0.5);
    float diffuse = max(dot(normal, lightDirection), 0.0);

    vec4 albedo = texture(textureDiffuse, passTextureCoords);
    FragColor = vec4(albedo.rgb * (ambient + lightColor * diffuse), albedo.a);
}
