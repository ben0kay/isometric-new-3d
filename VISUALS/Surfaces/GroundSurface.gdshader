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

void vertex() {
    ground_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    ground_normal = normalize(MODEL_NORMAL_MATRIX * NORMAL);
}

void fragment() {
    vec2 p = ground_position.xz;
    vec2 broad = p / max(patch_scale, 0.1) + seed_offset;
    float patches = ground_noise(broad);
    float dust_patches = ground_noise(broad * 1.37 + vec2(43.7, -21.2));
    float variation = ground_noise(broad * 3.1 + vec2(7.1, 18.4));
    float steepness = 1.0 - clamp(normalize(ground_normal).y, 0.0, 1.0);
    float rock_weight = smoothstep(rock_start, max(rock_end, rock_start + 0.001),
        steepness + (variation - 0.5) * 0.025);
    vec3 color = soil_surface(soil_low.rgb, soil_high.rgb, variation);
    color = mix(color, grass_surface(grass_low.rgb, grass_high.rgb, variation),
        surface_mask(patches, grass_coverage) * (1.0 - rock_weight));
    color = mix(color, dust_surface(dust_low.rgb, dust_high.rgb, variation),
        surface_mask(dust_patches, dust_coverage) * (1.0 - rock_weight));
    color = mix(color, rock_surface(rock_low.rgb, rock_high.rgb, variation), rock_weight);
    vec2 detail_p = p / max(detail_scale, 0.05) + seed_offset;
    float detail = ground_noise(detail_p);
    // Fade fine grain when zoomed out to reduce shimmer.
    float footprint = max(length(dFdx(detail_p)), length(dFdy(detail_p)));
    float detail_fade = 1.0 - smoothstep(0.3, 1.0, footprint);
    ALBEDO = clamp(color * (1.0 + (detail - 0.5) * detail_strength * detail_fade), vec3(0.0), vec3(1.0));
    ROUGHNESS = surface_roughness;
    SPECULAR = 0.15;
}
