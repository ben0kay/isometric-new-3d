// Shared surface weights preserve a dominant base rather than replacing it with competing patches.
// =========================================================
// Broad coverage is separate from blend strength; explicit endpoints support disabling a layer.
float surface_mask(float field, float coverage, float softness) {
    if (coverage <= 0.0) { return 0.0; }
    if (coverage >= 1.0) { return 1.0; }
    float threshold = mix(0.85, 0.15, coverage);
    return smoothstep(threshold - softness, threshold + softness, field);
}
// =========================================================
// Cap combined secondary weights so some base always remains on gentle/intermediate slopes.
vec3 surface_weights(float grass, float dust) {
    float total = grass + dust;
    float scale = min(1.0, 0.65 / max(total, 0.0001));
    grass *= scale;
    dust *= scale;
    return vec3(1.0 - grass - dust, grass, dust);
}
