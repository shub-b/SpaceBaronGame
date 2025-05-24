shader_type spatial;
render_mode unshaded;

varying vec3 world_vertex;

uniform float scale = 10.0;
// Set the two checker colors:
uniform vec3 color_black = vec3(0.0, 0.0, 0.0);
uniform vec3 color_yellow = vec3(1.0, 0.1, 0.7);

bool trues_even(bvec3 val) {
    return (int(val.x) + int(val.y) + int(val.z)) % 2 == 0;
}

void vertex() {
    world_vertex = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz / scale;
}

void fragment() {
    // determine which tile we're in
    vec3 rounded = floor(world_vertex + 0.0001);
    ivec3 tile = ivec3(rounded);
    bvec3 parity = equal(tile % 2, ivec3(0, 0, 0));
    bool evenTile = trues_even(parity);

    // assign black or yellow accordingly
    ALBEDO = evenTile
        ? color_black
        : color_yellow;
}
