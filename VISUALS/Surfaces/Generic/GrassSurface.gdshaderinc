// Adapted from the 2D grass recipe: mottled palette, fibres and restrained grain.
// =========================================================
// Biome tint remains data; no baked sun direction.
vec3 grass_surface(vec3 low_color, vec3 high_color, float strata, float detail, float grain, float strength) {
    vec3 surface = mix(low_color, high_color, smoothstep(0.15, 0.85, strata));
    float fibres = smoothstep(0.48, 0.78, detail);
    return surface * (0.92 + fibres * 0.08 + (grain - 0.5) * strength * 0.22);
}
