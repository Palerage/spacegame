shader_type canvas_item;

uniform float pulse_speed = 4.0;
uniform float glow_intensity = 1.5;

void fragment() {
    vec2 uv = UV - 0.5;
    float dist = length(uv) * 2.0;

    // Skapa puls
    float pulse = 0.5 + 0.5 * sin(TIME * pulse_speed);

    // Glow falloff
    float glow = pow(1.0 - dist, 3.0) * glow_intensity * pulse;

    vec4 tex = texture(TEXTURE, UV);
    COLOR = tex * (1.0 + glow);
}