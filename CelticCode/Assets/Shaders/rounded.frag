// Shader toy test will port to glsl later
float sdRoundBox( in vec2 p, in vec2 b, in vec4 r )
{
    r.xy = (p.x>0.0)?r.xy : r.zw;
    r.x  = (p.y>0.0)?r.x  : r.y;
    vec2 q = abs(p)-b+r.x;
    return min(max(q.x,q.y),0.0) + length(max(q,0.0)) - r.x;
}

void mainImage( out vec4 fragColor, in vec2 fragCoord )
{
	vec2 p = (2.0*fragCoord-iResolution.xy)/iResolution.y;

	vec2 si = vec2(0.9,0.6) + 0.3*cos(vec2(0,2));

	float dist = sdRoundBox( p, si, vec4(0.2));

    vec3 col = smoothstep( 0.0,0.01,dist)*vec3(0.65,0.85,1.0);


	fragColor = vec4(col,1.0);
}
