shader_type canvas_item;

uniform float health = 1.0;

uniform float blink_threshold = 0.2;

uniform float blink_speed  = 5.0;

void fragment() {
    vec3 col;

    if (health > 0.5) {
        float t = (health - 0.5) * 2.0;
        col = mix(vec3(1.0,1.0,0.0), vec3(0.0,1.0,0.0), t);

    } else if (health > blink_threshold) {
        float t = health * 2.0;
        col = mix(vec3(1.0,0.0,0.0), vec3(1.0,1.0,0.0), t);

    } else {
        float phase = sin(TIME * 2.0 * PI * blink_speed) * 0.5 + 0.5;
        vec3 orange = vec3(1.0, 0.5, 0.0);
        vec3 red    = vec3(1.0, 0.0, 0.0);
        col = mix(orange, red, phase);
    }

    COLOR = vec4(col, 1.0);
}
