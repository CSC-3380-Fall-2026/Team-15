shader_type canvas_item;
uniform vec4 outfit_color : source_color = vec4(1.0);
uniform vec4 weapon_color : source_color = vec4(0.90, 0.86, 0.75, 1.0);
uniform bool custom_outfit = false;
uniform int outfit_pattern = 0;
uniform int weapon_finish = 0;
varying vec4 vertex_tint;
void vertex() { vertex_tint = COLOR; }
void fragment() {
    vec4 tex = texture(TEXTURE, UV);
    vec2 pixel = mod(UV / TEXTURE_PIXEL_SIZE, 64.0);
    bool weapon = tex.r > 0.20 && tex.b > 0.20 && tex.g < 0.16 && abs(tex.r - tex.b) < 0.15;
    vec3 result;
    if (weapon) {
        result = weapon_color.rgb * (0.30 + tex.r * 0.75);
        if (weapon_finish == 1)
            result += weapon_color.rgb * (0.14 + 0.12 * sin(TIME * 3.0 + pixel.y * 0.7));
        if (weapon_finish == 2 && mod(pixel.y + pixel.x, 9.0) < 2.0)
            result = mix(result, vec3(1.0), 0.55);
    } else {
        float lightness = max(tex.r, max(tex.g, tex.b));
        result = custom_outfit ? mix(tex.rgb, vec3(lightness) * outfit_color.rgb, 0.82) : tex.rgb * outfit_color.rgb;
        bool cloth = pixel.x > 22.0 && pixel.x < 42.0 && pixel.y > 29.0 && pixel.y < 42.0;
        if (cloth && outfit_pattern == 1 && (pixel.x < 25.0 || pixel.x > 39.0))
            result = mix(result, weapon_color.rgb, 0.8);
        if (cloth && outfit_pattern == 2 && mod(pixel.x + pixel.y, 7.0) < 1.4)
            result = mix(result, weapon_color.rgb, 0.52);
    }
    COLOR = vec4(result, tex.a) * vertex_tint;
}
