// Reusable grass palette; biome profiles provide colours.
vec3 grass_surface(vec3 low_color, vec3 high_color, float variation) {
    return mix(low_color, high_color, variation);
}
