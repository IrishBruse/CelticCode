#version 450

layout(set = 0, binding = 0) uniform MvpBuffer
{
    mat4 Mvp;
};

layout(location = 0) in vec2 Position;
layout(location = 1) in vec2 TexCoords;
layout(location = 2) in vec4 Foreground;
layout(location = 3) in vec4 Background;

layout(location = 0) out vec2 fsin_texCoords;
layout(location = 1) out vec4 fsin_foreground;
layout(location = 2) out vec4 fsin_background;

void main()
{
    vec4 pos = vec4(Position, 1., 1.);
    gl_Position = Mvp * pos;
    fsin_texCoords = TexCoords;
    fsin_foreground = Foreground;
    fsin_background = Background;
}
