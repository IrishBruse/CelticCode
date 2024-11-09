#version 450

in vec2 fragTexCoord;
in vec4 fragColor;
uniform sampler2D texture0;

uniform vec4 colDiffuse;
uniform vec4 background;
uniform vec4 foreground;

out vec4 finalColor;

void main()
{
    vec4 texelColor = texture(texture0, fragTexCoord);

    float r = texelColor.r * (foreground.r-background.r);
    float g = texelColor.g * (foreground.g-background.g);
    float b = texelColor.b * (foreground.b-background.b);

    finalColor = vec4(r, g, b, 1.0);
}
