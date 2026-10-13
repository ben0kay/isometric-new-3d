// Sparse ground fibres and fine grain adapted from the 2D grass material.
// =========================================================
// Small brightness changes preserve material detail without pale camouflage islands.
vec3 grass_surface(vec3 low_color, vec3 high_color, float strata, float detail, float grain, float strength) {
    vec3 surface = mix(low_color, high_color, 0.35 + strata * 0.30);
    float fibres = smoothstep(0.48, 0.78, detail) - 0.5;
    return surface * (1.0 + (fibres * 0.07 + (grain - 0.5) * 0.16) * strength);
}
