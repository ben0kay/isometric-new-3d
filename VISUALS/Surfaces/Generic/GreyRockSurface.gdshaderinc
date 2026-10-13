// Creates cool grey exposed rock with restrained fractures and grain.

// =========================================================
// Produce a shared grey rock surface independently of biome placement.
vec3 grey_rock_surface(float strata, float detail, float grain, vec3 rock_low, vec3 rock_high, float fracture_strength) {
    vec3 rock = mix(
        rock_low,
        rock_high,
        smoothstep(0.15, 0.85, strata)
    );

    float fracture = 1.0 - smoothstep(
        0.008, 0.035, abs(detail - 0.5));
    fracture *= smoothstep(0.35, 0.75, strata);

    rock *= 1.0 - fracture * 0.22 * fracture_strength;
    rock += vec3((detail - 0.5) * 0.025);
    rock += vec3((grain - 0.5) * 0.018);

    return rock;
}