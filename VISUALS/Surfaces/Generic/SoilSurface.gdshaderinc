// Cohesive mineral soil with restrained fine detail; biome palette supplies its identity.
// =========================================================
// Compress tonal range to avoid medium-scale mottling that looks like a second patch mask.
vec3 soil_surface(vec3 low_color, vec3 high_color, float strata, float detail, float grain, float strength) {
    vec3 soil = mix(low_color, high_color, 0.35 + strata * 0.30);
    return soil * (1.0 + ((detail - 0.5) * 0.20 + (grain - 0.5) * 0.25) * strength);
}
