shader_type spatial;
render_mode specular_schlick_ggx;

uniform float pulse_time = 1.0;
uniform vec3  highlight_color = vec3(1.0, 0.5, 0.0);

uniform float metallic = 1.0;
uniform float roughness = 0.2;

void fragment() {
    vec3 base_col  = vec3(0.1, 0.1, 0.1);
    vec3 pulse_col = highlight_color;

    ALBEDO   = mix(base_col, pulse_col, pulse_time);
    METALLIC  = metallic;
    ROUGHNESS = roughness;
    SPECULAR  = 0.5;
	EMISSION = pulse_col * pulse_time;
}
