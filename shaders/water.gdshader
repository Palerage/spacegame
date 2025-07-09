shader_type canvas_item;

uniform vec4 water_color = vec4(0.0, 0.4, 0.7, 1.0); // Blå vattenfärg
uniform float wave_amplitude = 0.02;   // Hur stora vågorna är
uniform float wave_length = 10.0;      // Våglängd (täthet)
uniform float wave_speed = 2.0;        // Hastighet på vågorna

void fragment() {
    vec2 uv = UV;

    float wave = sin((uv.y * wave_length) + (TIME * wave_speed)) * wave_amplitude;

    uv.x += wave;

    vec4 color = water_color;

    float light_variation = 0.1 * sin((uv.y * wave_length * 2.0) + (TIME * wave_speed * 1.5));
    color.rgb += light_variation;

    COLOR = color;
}
