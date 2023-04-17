#version 450

layout(set = 0, binding = 0) uniform Matrix
{
    mat4 Mvp;
};

layout(location = 0) in vec2 Position;
layout(location = 1) in vec3 Foreground;

layout(location = 1) out vec3 fsin_foreground;

void main()
{
    vec4 pos = vec4(Position, 1., 1.);
    gl_Position = Mvp * pos;
    fsin_foreground = Foreground;
}
