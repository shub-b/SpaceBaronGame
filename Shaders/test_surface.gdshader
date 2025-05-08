shader_type spatial;

// Size of each “star cell” in world units
uniform float cell_size         = 10.0;
// Radius of each star dot (in cell‐UV coordinates 0→1)
uniform float star_radius       = 0.5;
// How much to bias brightness (bigger → more faint with a few brights)
uniform float brightness_power  = 2.0;

float hash12(vec2 p) {
    // simple hash → [0,1]
    return fract(sin(dot(p, vec2(127.1,311.7))) * 43758.5453123);
}

void fragment() {
    // 1) World‐space XZ wrapped into cells
    vec3 world_pos = (MODEL_MATRIX * vec4(VERTEX,1)).xyz;
    vec2 wp = world_pos.xz / cell_size;
    vec2 cell = floor(wp);
    vec2 fuv  = fract(wp);

    // 2) Generate a single pseudo‐random offset + base brightness
    float rnd       = hash12(cell);
    vec2  star_pos  = vec2(
        hash12(cell + vec2(1.0,0.0)),
        hash12(cell + vec2(0.0,1.0))
    );
    float brightness = pow(rnd, brightness_power);

    // 3) Distance from this fragment to the star center
    float d = distance(fuv, star_pos);

    // 4) Sharp circular mask: 1.0 inside inner radius, 0.0 outside full radius
    float inner = star_radius * 0.5;
    float m = smoothstep(star_radius, inner, d); 
    // m == 1 when d≤inner, m == 0 when d≥star_radius

    // 5) Output: white*brightness for the star, else black
    ALBEDO = mix(vec3(0.0), vec3(brightness), m);
}
