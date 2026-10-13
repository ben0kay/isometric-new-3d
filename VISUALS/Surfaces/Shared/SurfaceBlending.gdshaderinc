// Soft masks avoid hard contour lines and chunk-dependent texture coordinates.
float surface_mask(float noise_value, float coverage) {
    if (coverage <= 0.0) { return 0.0; }
    if (coverage >= 1.0) { return 1.0; }
    float threshold = 1.0 - coverage;
    return smoothstep(threshold - 0.12, threshold + 0.12, noise_value);
}
