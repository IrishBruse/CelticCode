#version 450

layout(location = 1) in vec3 fsin_foreground;

layout(location = 0) out vec4 fsout_Color;

void main()
{
    float r = fsin_foreground.r;
    float g = fsin_foreground.g;
    float b = fsin_foreground.b;

    fsout_Color = vec4(r, g, b, 1.0);
}
