#version 450

in vec2 fragTexCoord;
in vec4 fragColor;
uniform sampler2D texture0;

uniform vec4 colDiffuse;
uniform vec4 fsin_background;
uniform vec4 fsin_foreground;

out vec4 finalColor;

#define gamma 2.2

vec4 pow4(vec4 v, float p)
{
    return vec4(pow(v.r, p), pow(v.g, p), pow(v.b, p), v.a);
}

void main()
{
    vec4 texelColor = texture(texture0, fragTexCoord);
    vec4 foreground = pow4(fsin_foreground, 1.0 / gamma);
    vec4 background = pow4(fsin_background, 1.0 / gamma);

    float r = texelColor.r * foreground.r + (1.0 - texelColor.r) * background.r;
    float g = texelColor.g * foreground.g + (1.0 - texelColor.g) * background.g;
    float b = texelColor.b * foreground.b + (1.0 - texelColor.b) * background.b;

    finalColor = pow4(vec4(r, g, b, 1.0),  gamma);
}
