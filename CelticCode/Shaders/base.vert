#version 450

layout(set = 0, binding = 0) uniform MvpBuffer
{
    mat4 Mvp;
};

layout(location = 0) in vec2 Position;
layout(location = 1) in vec2 TexCoords;

layout(location = 0) out vec2 fsin_texCoords;

void main()
{
    vec4 pos = vec4(Position, 1., 1.);
    gl_Position = Mvp * pos;
    fsin_texCoords = TexCoords;
}
