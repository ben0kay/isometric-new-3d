// Layered ground appearance adapted from the 2D material recipes.
shader_type spatial;
render_mode diffuse_burley, specular_schlick_ggx;

#include "res://VISUALS/Surfaces/Shared/GroundNoise.gdshaderinc"
#include "res://VISUALS/Surfaces/Shared/SurfaceBlending.gdshaderinc"
#include "res://VISUALS/Surfaces/Generic/SoilSurface.gdshaderinc"
#include "res://VISUALS/Surfaces/Generic/GrassSurface.gdshaderinc"
#include "res://VISUALS/Surfaces/Generic/DustSurface.gdshaderinc"
#include "res://VISUALS/Surfaces/Generic/RockSurface.gdshaderinc"

uniform vec4 soil_low : source_color = vec4(0.24, 0.19, 0.13, 1.0);
uniform vec4 soil_high : source_color = vec4(0.43, 0.34, 0.23, 1.0);
uniform vec4 grass_low : source_color = vec4(0.18, 0.27, 0.12, 1.0);
uniform vec4 grass_high : source_color = vec4(0.35, 0.43, 0.21, 1.0);
uniform vec4 dust_low : source_color = vec4(0.43, 0.37, 0.26, 1.0);
uniform vec4 dust_high : source_color = vec4(0.61, 0.54, 0.39, 1.0);
uniform vec4 rock_low : source_color = vec4(0.26, 0.28, 0.29, 1.0);
uniform vec4 rock_high : source_color = vec4(0.48, 0.49, 0.46, 1.0);
uniform vec2 seed_offset = vec2(0.0);
uniform float patch_scale = 14.0;
uniform float grass_coverage = 0.55;
uniform float dust_coverage = 0.35;
uniform float rock_start = 0.06;
uniform float rock_end = 0.2;
uniform float detail_scale = 0.7;
uniform float detail_strength = 0.07;
uniform float surface_roughness = 0.95;
varying vec3 ground_position;
varying vec3 ground_normal;

// =========================================================
// Pass continuous world position and world-space slope normals.
void vertex() {
    ground_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    ground_normal = normalize(MODEL_NORMAL_MATRIX * NORMAL);
}

// =========================================================
// Blend irregular patches and material-specific detail in absolute world coordinates.
void fragment() {
    vec2 p = ground_position.xz;
    vec2 broad = p / max(patch_scale, 0.1) + seed_offset;
    vec2 warp = vec2(ground_noise(broad * 0.7 + vec2(13.1, 39.2)),
        ground_noise(broad * 0.7 + vec2(71.3, 17.8))) - vec2(0.5);
    float patches = ground_fractal(broad + warp * 0.7);
    float dust_patches = ground_fractal(broad * 1.37 + warp * 0.5 + vec2(43.7, -21.2));
    float strata = ground_noise(broad * 2.3 + vec2(7.1, 18.4));
    vec2 detail_p = p / max(detail_scale, 0.05) + seed_offset;
    float footprint = max(length(dFdx(detail_p)), length(dFdy(detail_p)));
    float detail_fade = 1.0 - smoothstep(0.3, 1.0, footprint);
    float grain_fade = 1.0 - smoothstep(0.3, 1.0, footprint * 4.0);
    float detail = mix(0.5, ground_noise(detail_p), detail_fade);
    float grain = mix(0.5, ground_noise(detail_p * 4.0 + vec2(31.7, 9.2)), grain_fade);
    float pebbles = ground_pebbles(detail_p * 1.1) * detail_fade;
    float steepness = 1.0 - clamp(normalize(ground_normal).y, 0.0, 1.0);
    float rock_weight = smoothstep(rock_start, max(rock_end, rock_start + 0.001),
        steepness + (strata - 0.5) * 0.025);
    vec3 color = soil_surface(soil_low.rgb, soil_high.rgb, strata, detail, grain, detail_strength);
    color = mix(color, grass_surface(grass_low.rgb, grass_high.rgb, strata, detail, grain, detail_strength),
        surface_mask(patches, grass_coverage) * (1.0 - rock_weight));
    color = mix(color, dust_surface(dust_low.rgb, dust_high.rgb, strata, detail, grain, pebbles, detail_strength),
        surface_mask(dust_patches, dust_coverage) * (1.0 - rock_weight));
    color = mix(color, rock_surface(rock_low.rgb, rock_high.rgb, strata, detail, grain,
        detail_strength * detail_fade), rock_weight);
    ALBEDO = clamp(color, vec3(0.0), vec3(1.0));
    ROUGHNESS = surface_roughness;
    SPECULAR = 0.15;
}
