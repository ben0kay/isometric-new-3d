// Adapted 2D exposed rock recipe: strata, restrained fractures and grain.
// =========================================================
// Fractures are material coloration, not fixed-direction painted shadows.
vec3 rock_surface(vec3 low_color, vec3 high_color, float strata, float detail, float grain, float strength) {
    vec3 rock = mix(low_color, high_color, smoothstep(0.15, 0.85, strata));
    float fracture = (1.0 - smoothstep(0.008, 0.035, abs(detail - 0.5))) * smoothstep(0.35, 0.75, strata);
    rock *= 1.0 - fracture * 0.22 * strength;
    return rock * (1.0 + ((detail - 0.5) * 0.15 + (grain - 0.5) * 0.12) * strength);
}
