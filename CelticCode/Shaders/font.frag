#version 450

layout(location = 0) in vec2 fsin_texCoords;
layout(location = 0) in vec2 background;
layout(location = 0) in vec2 foreground;

layout(location = 0) out vec4 fsout_Color;

layout(set = 1, binding = 1) uniform texture2D SurfaceTexture;
layout(set = 1, binding = 2) uniform sampler SurfaceSampler;

void main()
{
	// convert the vertex colour to linear space
	vec4 v_colour_linear = pow4(v_colour, inv_gamma);

	// convert the background colour to linear space
	vec4 v_background_colour_linear = pow4(v_background_colour, inv_gamma);

	// blend
	float r = tex_col.r * v_colour_linear.r + (1.0 - tex_col.r) * v_background_colour_linear.r;
	float g = tex_col.g * v_colour_linear.g + (1.0 - tex_col.g) * v_background_colour_linear.g;
	float b = tex_col.b * v_colour_linear.b + (1.0 - tex_col.b) * v_background_colour_linear.b;

	// gamma encode the result
	gl_FragColor = pow4(vec4(r, g, b, tex_col.a), gamma);
}
