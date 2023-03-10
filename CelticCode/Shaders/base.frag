#version 450

layout(location = 0) in vec2 fsin_texCoords;
layout(location = 1) in vec4 fsin_foreground;
layout(location = 2) in vec4 fsin_background;

layout(location = 0) out vec4 fsout_Color;

layout(set = 1, binding = 1) uniform texture2D SurfaceTexture;
layout(set = 1, binding = 2) uniform sampler SurfaceSampler;

vec4 pow4(vec4 x, float y)
{
    return vec4(pow(x.r, y), pow(x.g, y), pow(x.b, y), pow(x.a, y));
}

void main()
{
    vec4 tex_col =  texture(sampler2D(SurfaceTexture, SurfaceSampler), fsin_texCoords);

    tex_col = pow4(tex_col,1.44);

    float r = tex_col.r * fsin_foreground.r + (1.0-tex_col.r) * fsin_background.r;
    float g = tex_col.g * fsin_foreground.g + (1.0-tex_col.g) * fsin_background.g;
    float b = tex_col.b * fsin_foreground.b + (1.0-tex_col.b) * fsin_background.b;

    fsout_Color = vec4(r, g, b, 1.0) ;
}
