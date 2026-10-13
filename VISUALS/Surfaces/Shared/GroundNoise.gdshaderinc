// Shared world-space noise at broad, medium and fine scales.
// =========================================================
// Stateless hash, independent of chunk load order.
float ground_hash(vec2 p) {
    vec3 q = fract(vec3(p.xyx) * 0.1031);
    q += dot(q, q.yzx + 33.33);
    return fract((q.x + q.y) * q.z);
}
// =========================================================
// Smooth continuous lattice noise.
float ground_noise(vec2 p) {
    vec2 cell = floor(p), t = fract(p);
    t = t * t * (3.0 - 2.0 * t);
    return mix(mix(ground_hash(cell), ground_hash(cell + vec2(1.0, 0.0)), t.x),
        mix(ground_hash(cell + vec2(0.0, 1.0)), ground_hash(cell + vec2(1.0)), t.x), t.y);
}
// =========================================================
// Four rotated octaves break up smooth blobs and visible lattice direction.
float ground_fractal(vec2 p) {
    mat2 rotation = mat2(vec2(0.8, 0.6), vec2(-0.6, 0.8));
    float value = 0.0, amplitude = 0.533333;
    for (int octave = 0; octave < 4; octave++) {
        value += ground_noise(p) * amplitude;
        p = rotation * p * 2.03 + vec2(17.2, 9.4);
        amplitude *= 0.5;
    }
    return value;
}
// =========================================================
// Small bounded irregular gravel flecks, without expensive neighbor searches.
float ground_pebbles(vec2 p) {
    vec2 cell = floor(p);
    float pick = ground_hash(cell + vec2(11.3, 47.1));
    vec2 center = vec2(0.3) + 0.4 * vec2(ground_hash(cell), ground_hash(cell + vec2(2.7)));
    vec2 d = (fract(p) - center) * vec2(1.0, 1.3);
    float radius = mix(0.09, 0.22, ground_hash(cell + vec2(6.2)));
    return (1.0 - smoothstep(radius - 0.025, radius + 0.025, length(d))) * step(pick, 0.3);
}
