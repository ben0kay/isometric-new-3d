// Adapted sediment recipe with fine grit and small gravel flecks.
// =========================================================
// Muted dust avoids bright golden blobs.
vec3 dust_surface(vec3 low_color, vec3 high_color, float strata, float detail, float grain, float pebbles, float strength) {
    vec3 surface = mix(low_color, high_color, strata);
    surface *= 1.0 + ((detail - 0.5) * 0.3 + (grain - 0.5) * 0.25) * strength;
    return mix(surface, surface * 0.72, pebbles * strength);
}
