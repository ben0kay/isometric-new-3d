// Creates subdued stone grit for flattened mountain paths.

// =========================================================
// Produce a shared path surface with fine gravel and occasional dark flecks.
vec3 stony_path_surface(float strata, float detail, float grain, vec3 path_low, vec3 path_high) {
    vec3 path = mix(
        path_low,
        path_high,
        strata
    );

    float stones = smoothstep(0.62, 0.83, detail);
    path += vec3(stones * 0.026);
    path -= vec3(
        (1.0 - smoothstep(0.12, 0.28, grain)) * 0.018);
    path += vec3((grain - 0.5) * 0.014);

    return path;
}