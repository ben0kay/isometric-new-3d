// Lightweight compatibility-friendly grade with a bounded contrast curve and shadow lift.
shader_type canvas_item;
render_mode unshaded;
uniform sampler2D world_screen : hint_screen_texture, repeat_disable, filter_linear;
uniform float contrast = 1.12;
uniform float saturation = 0.94;
uniform float shadow_lift = 0.012;
uniform float vignette = 0.04;

// =========================================================
// Grade the captured world once; HUD layers drawn later remain unaffected.
void fragment() {
    vec3 original = clamp(texture(world_screen, SCREEN_UV).rgb, vec3(0.0), vec3(1.0));
    // Smooth contrast without subtracting a constant that crushes dark basalt.
    vec3 color = original + (contrast - 1.0) * (original - vec3(0.5))
        * original * (vec3(1.0) - original) * 4.0;
    float luminance = dot(color, vec3(0.2126, 0.7152, 0.0722));
    color = mix(vec3(luminance), color, saturation);
    float lift_mask = 1.0 - smoothstep(0.0, 0.4, luminance);
    color += vec3(shadow_lift * lift_mask);
    vec2 q = SCREEN_UV * 2.0 - vec2(1.0);
    float edge = smoothstep(0.45, 1.35, length(q));
    color *= 1.0 - edge * vignette;
    COLOR = vec4(clamp(color, vec3(0.0), vec3(1.0)), 1.0);
}
