// Multiscale soil coloration with subtle grain.
// =========================================================
// Reusable biome palette avoids hardcoded Test colours.
vec3 soil_surface(vec3 low_color, vec3 high_color, float strata, float detail, float grain, float strength) {
    vec3 soil = mix(low_color, high_color, smoothstep(0.15, 0.85, strata));
    return soil * (1.0 + ((detail - 0.5) * 0.3 + (grain - 0.5) * 0.3) * strength);
}
