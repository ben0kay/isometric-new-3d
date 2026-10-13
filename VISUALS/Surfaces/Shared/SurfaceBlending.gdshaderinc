// More defined patch transitions retain a soft edge without broad blurred bands.
// =========================================================
// Coverage controls masks, with explicit endpoints.
float surface_mask(float noise_value, float coverage) {
    if (coverage <= 0.0) { return 0.0; }
    if (coverage >= 1.0) { return 1.0; }
    float threshold = 1.0 - coverage;
    return smoothstep(threshold - 0.055, threshold + 0.055, noise_value);
}
