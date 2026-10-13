// Continuous world-space value noise; no per-chunk random state.
float ground_hash(vec2 p) {
    vec3 q = fract(vec3(p.xyx) * 0.1031);
    q += dot(q, q.yzx + 33.33);
    return fract((q.x + q.y) * q.z);
}
float ground_noise(vec2 p) {
    vec2 cell = floor(p);
    vec2 t = fract(p);
    t = t * t * (3.0 - 2.0 * t);
    return mix(mix(ground_hash(cell), ground_hash(cell + vec2(1.0, 0.0)), t.x),
        mix(ground_hash(cell + vec2(0.0, 1.0)), ground_hash(cell + vec2(1.0)), t.x), t.y);
}
