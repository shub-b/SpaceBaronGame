shader_type spatial;
render_mode unshaded;

uniform float health_percent = 1.0;
uniform float blink_speed  = 5.0;
uniform float crack_scale = 2.0;
uniform sampler2D cracks_texture;

const vec3 FULL_COLOR = vec3(0.0,0.5,1.0);
const vec3 MID_COLOR = vec3(1.0,1.0,0.0);
const vec3 LOW_COLOR1 = vec3(1.0,0.2,0.0);
const vec3 LOW_COLOR2 = vec3(1.0,0.0,0.0);

void fragment() {
    vec3 base_col;
    if (health_percent > 0.5) {
        float t = (health_percent - 0.5)*2.0;
        base_col = mix(MID_COLOR, FULL_COLOR, t);
    } else {
        float t = health_percent*2.0;
        base_col = mix(LOW_COLOR2, LOW_COLOR1, t);
    }
    if (health_percent < 0.15) {
        float blink = step(0.0, sin( TIME * blink_speed));
        base_col = mix(LOW_COLOR1, LOW_COLOR2, blink);
    }

    float mask = texture(cracks_texture, UV * crack_scale).r;
    float alpha = mask * (1.0 - health_percent);
    vec3 final_col = mix(base_col, vec3(0.0), alpha);

    ALBEDO   = final_col;
    EMISSION = final_col;
}
