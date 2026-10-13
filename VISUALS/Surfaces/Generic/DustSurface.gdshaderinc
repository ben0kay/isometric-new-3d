// Defines muted sediment coloration with fine grain and broad tonal variation.

// =========================================================
// Produce a dusty surface that blends into basalt without bright golden blobs.
vec3 dust_surface(float strata, float detail, float grain, vec3 dust_low, vec3 dust_high) {
    vec3 dust = mix(
        dust_low,
        dust_high,
        strata
    );

    dust += vec3((detail - 0.5) * 0.009);
    dust += vec3((grain - 0.5) * 0.012);
    return dust;
}