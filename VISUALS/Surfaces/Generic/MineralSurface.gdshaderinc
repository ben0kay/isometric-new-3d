// Defines restrained mineral deposits following continuous stone fractures.
// Mineral coloration is reflected surface color rather than emitted light.

// =========================================================
// Blend sparse mineral seams into the existing ground surface.
vec3 mineral_surface(vec3 ground, float strata, float detail, float deposit, vec3 mineral_low, vec3 mineral_high) {
    float vein = 1.0 - smoothstep(0.009, 0.028, abs(detail - 0.53));
    vein *= smoothstep(0.40, 0.68, strata) * deposit;

    vec3 mineral = mix(
        mineral_low,
        mineral_high,
        strata
    );

    ground = mix(ground, ground * vec3(0.94, 1.025, 1.015), deposit * 0.25);
    return mix(ground, mineral, vein * 0.70);
}