// Muted sediment with sparse gravel; not a separate bright sand island.
// =========================================================
// Restrained grain and flecks read inside a low-contrast deposit.
vec3 dust_surface(vec3 low_color, vec3 high_color, float strata, float detail, float grain, float pebbles, float strength) {
    vec3 surface = mix(low_color, high_color, 0.35 + strata * 0.30);
    surface *= 1.0 + ((detail - 0.5) * 0.16 + (grain - 0.5) * 0.20) * strength;
    return mix(surface, surface * 0.78, pebbles * strength);
}
