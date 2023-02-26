#version 450

layout(location = 0) in vec2 Position;
layout(location = 1) in vec2 TexCoords;

layout(location = 0) out vec2 fsin_texCoords;

void main()
{
    gl_Position = vec4(Position, 0, 1);
    fsin_texCoords = TexCoords;
}
