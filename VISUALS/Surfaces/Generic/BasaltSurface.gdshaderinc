// Defines dark basalt coloration, fine grain and sparse hairline fractures.

// =========================================================
// Produce restrained stone variation without isolated tile-centred shapes.
vec3 basalt_surface(float strata, float detail, float grain, vec3 basalt_low, vec3 basalt_high, float fracture_strength) {
    vec3 stone = mix(
        basalt_low,
        basalt_high,
        strata
    );

    float fracture = 1.0 - smoothstep(0.008, 0.026, abs(detail - 0.49));
    fracture *= smoothstep(0.59, 0.77, strata);

    stone *= 1.0 - fracture * 0.24 * fracture_strength;
    stone += vec3((detail - 0.5) * 0.014);
    stone += vec3((grain - 0.5) * 0.018);
    return stone;
}