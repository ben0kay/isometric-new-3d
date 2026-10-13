// Faithful 2D basalt-first material formulas, with 3D normals and engine lighting.
shader_type spatial;
render_mode diffuse_burley, specular_schlick_ggx;

#include "res://VISUALS/Surfaces/Generic/BasaltSurface.gdshaderinc"
#include "res://VISUALS/Surfaces/Generic/DustSurface.gdshaderinc"
#include "res://VISUALS/Surfaces/Generic/MineralSurface.gdshaderinc"
#include "res://VISUALS/Surfaces/Generic/GreyRockSurface.gdshaderinc"
#include "res://VISUALS/Surfaces/Generic/SteepRockSurface.gdshaderinc"
#include "res://VISUALS/Surfaces/Generic/GrassSurface.gdshaderinc"
#include "res://VISUALS/Surfaces/Generic/MudSurface.gdshaderinc"
#include "res://VISUALS/Surfaces/Generic/StonyPathSurface.gdshaderinc"
#include "res://VISUALS/Surfaces/Generic/SoilSurface.gdshaderinc"

uniform sampler2D ground_noise : repeat_enable, filter_linear_mipmap;
// Raw reference coefficients: no extra source_color conversion of the original formula palettes.
uniform vec4 basalt_low = vec4(0.075, 0.086, 0.098, 1.0);
uniform vec4 basalt_high = vec4(0.135, 0.148, 0.157, 1.0);
uniform vec4 dust_low = vec4(0.112, 0.105, 0.094, 1.0);
uniform vec4 dust_high = vec4(0.177, 0.158, 0.130, 1.0);
uniform vec4 mineral_low = vec4(0.105, 0.151, 0.145, 1.0);
uniform vec4 mineral_high = vec4(0.215, 0.279, 0.245, 1.0);
uniform vec4 rock_low = vec4(0.115, 0.125, 0.135, 1.0);
uniform vec4 rock_high = vec4(0.235, 0.245, 0.255, 1.0);
uniform vec4 steep_low = vec4(0.135, 0.145, 0.155, 1.0);
uniform vec4 steep_high = vec4(0.265, 0.275, 0.28, 1.0);
uniform vec4 grass_low = vec4(0.12, 0.20, 0.10, 1.0);
uniform vec4 grass_high = vec4(0.24, 0.34, 0.17, 1.0);
uniform vec4 soil_low = vec4(0.20, 0.16, 0.12, 1.0);
uniform vec4 soil_high = vec4(0.30, 0.25, 0.19, 1.0);
uniform vec4 mud_low = vec4(0.035, 0.036, 0.032, 1.0);
uniform vec4 mud_high = vec4(0.078, 0.071, 0.056, 1.0);
uniform vec4 path_low = vec4(0.145, 0.142, 0.135, 1.0);
uniform vec4 path_high = vec4(0.215, 0.210, 0.195, 1.0);
uniform vec2 pattern_offset = vec2(0.0);
uniform float meters_per_tile = 1.0;
uniform float deposit_scale = 1.0;
uniform float blend_softness = 1.0;
uniform float dust_amount = 0.55;
uniform float dust_strength = 1.0;
uniform float mineral_amount = 0.32;
uniform float grass_amount = 0.0;
uniform float grass_strength = 1.0;
uniform float grass_patch_scale = 32.0;
uniform float soil_base_weight = 0.0;
uniform float mud_strength = 0.0;
uniform float stony_strength = 0.0;
uniform float weathered_amount = 0.0;
uniform float rock_start = 0.072816;
uniform float rock_end = 0.211989;
uniform bool steep_blocks = true;
uniform float detail_strength = 0.35;
uniform float grain_strength = 1.0;
uniform float fracture_strength = 1.0;
uniform float relief_strength = 0.035;
uniform float surface_brightness = 1.0;
uniform float surface_roughness = 0.95;
varying vec3 ground_position;
varying vec3 ground_normal;

// =========================================================
// Preserve continuous absolute coordinates and unmodified physical slope normals.
void vertex() {
    ground_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    ground_normal = normalize(MODEL_NORMAL_MATRIX * NORMAL);
}

// =========================================================
// Build original material colours first; Godot handles all lighting and cast shadows.
void fragment() {
    vec2 p = ground_position.xz / max(meters_per_tile, 0.25) + pattern_offset;
    // Exact reference frequencies and offsets from GroundSun.gdshader.
    float dust_field = texture(ground_noise, p * 0.0039 / deposit_scale + vec2(0.17, 0.43)).r;
    float mineral_field = texture(ground_noise, p * 0.0061 / deposit_scale + vec2(0.68, 0.21)).r;
    float strata = texture(ground_noise, p * 0.035 + vec2(0.31, 0.74)).r;
    float detail = texture(ground_noise, p * 0.22 + vec2(0.53, 0.11)).r;
    float grain = texture(ground_noise, p * 2.7 + vec2(0.09, 0.62)).r;
    grain = mix(0.5, grain, grain_strength);

    float dust_threshold = mix(0.85, 0.30, dust_amount);
    float dust_mask = smoothstep(dust_threshold - 0.12 * blend_softness,
        dust_threshold + 0.12 * blend_softness, dust_field);
    dust_mask *= smoothstep(0.0, 0.08, dust_amount) * dust_strength;
    float mineral_threshold = mix(0.88, 0.38, mineral_amount);
    float mineral_mask = smoothstep(mineral_threshold - 0.07 * blend_softness,
        mineral_threshold + 0.09 * blend_softness, mineral_field);
    mineral_mask *= smoothstep(0.0, 0.08, mineral_amount);

    vec3 stone = basalt_surface(strata, detail, grain, basalt_low.rgb, basalt_high.rgb, fracture_strength);
    if (soil_base_weight > 0.0001) {
        stone = mix(stone, soil_surface(soil_low.rgb, soil_high.rgb,
            strata, detail, grain, detail_strength), soil_base_weight);
    }
    vec3 dust = dust_surface(strata, detail, grain, dust_low.rgb, dust_high.rgb);
    vec3 ground = mix(stone, dust, dust_mask);
    ground = mineral_surface(ground, strata, detail,
        mineral_mask * (1.0 - dust_mask * 0.75), mineral_low.rgb, mineral_high.rgb);

    float steepness = 1.0 - clamp(normalize(ground_normal).y, 0.0, 1.0);
    float steep_mask = smoothstep(rock_start, max(rock_end, rock_start + 0.001), steepness);
    if (weathered_amount > 0.0001) {
        float weathered = smoothstep(0.60, 0.82, mineral_field) * weathered_amount;
        ground = mix(ground, grey_rock_surface(strata, detail, grain,
            rock_low.rgb, rock_high.rgb, fracture_strength), weathered);
    }

    float grass_mask = 0.0;
    vec2 patch_uv = p * 0.0039 * (32.0 / max(grass_patch_scale, 4.0));
    if (grass_amount > 0.0001) {
        float grass_field = texture(ground_noise, patch_uv + vec2(0.29, 0.61)).r;
        float threshold = mix(0.90, 0.10, grass_amount);
        grass_mask = smoothstep(threshold - 0.14 * blend_softness,
            threshold + 0.14 * blend_softness, grass_field);
        grass_mask *= smoothstep(0.0, 0.08, grass_amount);
        grass_mask = mix(grass_mask, 1.0, smoothstep(0.90, 1.0, grass_amount));
        grass_mask *= grass_strength * (1.0 - steep_mask);
        ground = mix(ground, grass_surface(strata, detail, grain,
            grass_low.rgb, grass_high.rgb), grass_mask);
    }
    if (mud_strength > 0.0001) {
        ground = mud_surface(ground, ground_noise, patch_uv,
            mud_strength, strata, grain, mud_low.rgb, mud_high.rgb);
    }
    if (stony_strength > 0.0001) {
        ground = mix(ground, stony_path_surface(strata, detail, grain,
            path_low.rgb, path_high.rgb), stony_strength);
    }
    // The original 18-cell fracture search runs only where steep rock is visible.
    if (steep_mask > 0.001) {
        vec3 normal_world = normalize(ground_normal);
        vec2 gradient = -normal_world.xz / max(normal_world.y, 0.05);
        vec3 exposed = grey_rock_surface(strata, detail, grain,
            rock_low.rgb, rock_high.rgb, fracture_strength);
        if (steep_blocks) {
            exposed = steep_rock_surface(p, gradient, detail, grain,
                steep_low.rgb, steep_high.rgb, fracture_strength);
        }
        ground = mix(ground, exposed, steep_mask);
    }

    // Replace reference sunward brightness relief with tiny view-space normal relief.
    // Derivatives remain outside optional layer branches to keep mip/normal evaluation stable.
    vec3 dx = dFdx(VERTEX), dy = dFdy(VERTEX);
    vec3 normal_view = normalize(NORMAL);
    vec3 rx = cross(dy, normal_view), ry = cross(normal_view, dx);
    float determinant = dot(dx, rx);
    float height = detail * relief_strength * detail_strength *
        (1.0 - dust_mask * 0.70) * (1.0 - grass_mask * 0.35);
    vec3 gradient_view = sign(determinant) * (dFdx(height) * rx + dFdy(height) * ry);
    if (abs(determinant) > 0.000001) {
        NORMAL = normalize(abs(determinant) * normal_view - gradient_view);
    }
    ALBEDO = clamp(ground * surface_brightness, vec3(0.0), vec3(1.0));
    ROUGHNESS = surface_roughness;
    SPECULAR = 0.15;
}
