// Cohesive mineral soil with subordinate vegetation/sediment and smooth slope-driven rock.
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
uniform float grass_strength = 0.38;
uniform float dust_strength = 0.24;
uniform float blend_softness = 0.18;
uniform float macro_variation = 0.10;
uniform float soil_start = 0.022;
uniform float soil_end = 0.117;
uniform float flow_angle = -0.436332;
uniform float flow_stretch = 2.8;
uniform float warp_strength = 0.35;
varying vec3 ground_position;
varying vec3 ground_normal;

// =========================================================
// Pass continuous world position and world-space slope normals.
void vertex() {
    ground_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    ground_normal = normalize(MODEL_NORMAL_MATRIX * NORMAL);
}

// =========================================================
// Separate broad correlated regions from subtle grain; lighting remains fully engine-driven.
void fragment() {
    vec2 p = ground_position.xz;
    float c = cos(flow_angle), s = sin(flow_angle);
    vec2 flow = vec2(c * p.x - s * p.y, s * p.x + c * p.y);
    flow.x /= max(flow_stretch, 1.0);
    vec2 broad = flow / max(patch_scale, 1.0) + seed_offset;
    vec2 warp = vec2(ground_noise(broad * 0.6 + vec2(13.1, 39.2)),
        ground_noise(broad * 0.6 + vec2(71.3, 17.8))) - vec2(0.5);
    vec2 domain = broad + warp * warp_strength;
    // Two broad octaves, replacing two four-octave coverage fields.
    float region = ground_noise(domain);
    region = region * 0.8 + ground_noise(domain * 2.03 + vec2(8.2, 19.7)) * 0.2;
    float deposit = ground_noise(domain * 0.75 + vec2(43.7, -21.2));
    float strata = region * 0.7 + deposit * 0.3;

    vec2 detail_p = p / max(detail_scale, 0.05) + seed_offset;
    float footprint = max(length(dFdx(detail_p)), length(dFdy(detail_p)));
    float detail_fade = 1.0 - smoothstep(0.3, 1.0, footprint);
    float grain_fade = 1.0 - smoothstep(0.3, 1.0, footprint * 4.0);
    float detail = mix(0.5, ground_noise(detail_p), detail_fade);
    float grain = mix(0.5, ground_noise(detail_p * 4.0 + vec2(31.7, 9.2)), grain_fade);
    float pebbles = ground_pebbles(detail_p * 1.1) * detail_fade;

    float steepness = 1.0 - clamp(normalize(ground_normal).y, 0.0, 1.0);
    // Mask breakup affects transition strength, never sampling coordinates.
    float slope_field = max(0.0, steepness + (deposit - 0.5) * 0.012);
    float soil_weight = smoothstep(soil_start, max(soil_end, soil_start + 0.001), slope_field);
    float rock_weight = smoothstep(rock_start, max(rock_end, rock_start + 0.001), slope_field);
    float grass_weight = surface_mask(region, grass_coverage, blend_softness)
        * grass_strength * (1.0 - soil_weight);
    float dust_weight = surface_mask(deposit, dust_coverage, blend_softness)
        * dust_strength * (1.0 - soil_weight * 0.65);
    vec3 weights = surface_weights(grass_weight, dust_weight);

    vec3 soil = soil_surface(soil_low.rgb, soil_high.rgb, strata, detail, grain, detail_strength);
    vec3 grass = grass_surface(grass_low.rgb, grass_high.rgb, strata, detail, grain, detail_strength);
    vec3 dust = dust_surface(dust_low.rgb, dust_high.rgb, strata, detail, grain, pebbles, detail_strength);
    vec3 color = soil * weights.x + grass * weights.y + dust * weights.z;
    vec3 rock = rock_surface(rock_low.rgb, rock_high.rgb, strata, detail, grain, detail_strength * detail_fade);
    color = mix(color, rock, rock_weight);
    color *= 1.0 + (region - 0.5) * macro_variation;
    ALBEDO = clamp(color, vec3(0.0), vec3(1.0));
    ROUGHNESS = surface_roughness;
    SPECULAR = 0.15;
}
