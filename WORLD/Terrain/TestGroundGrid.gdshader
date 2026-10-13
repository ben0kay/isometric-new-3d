shader_type spatial;
render_mode diffuse_burley;

uniform vec4 ground_color : source_color = vec4(0.38, 0.43, 0.40, 1.0);
uniform float grid_spacing = 2.0;
varying vec3 world_position;

void vertex() {
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
}

void fragment() {
    vec2 coord = world_position.xz / max(grid_spacing, 0.5);
    vec2 distance_to_line = abs(fract(coord - 0.5) - 0.5);
    vec2 pixel_width = max(fwidth(coord), vec2(0.0001));
    vec2 lines = vec2(1.0) - smoothstep(vec2(0.0), pixel_width * 1.2, distance_to_line);
    float grid = max(lines.x, lines.y);
    ALBEDO = ground_color.rgb * mix(1.0, 0.82, grid);
    ROUGHNESS = 0.95;
}
