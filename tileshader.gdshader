shader_type canvas_item;

const int amount_tiles = 3;
const int atlas_width = 2;
const int atlas_height = 2;

uniform sampler2D noise_map;
uniform sampler2D atlas_texture;

uniform vec2 world_tile_size;
uniform vec2 world_max_pos;
varying vec4 world_position;

void vertex() {
    // MODEL_MATRIX: Godots transformation matrix to transform GlobalPosition (world) to screen.
    // 0.0: ignore z (the MODEL_MATRIX is secretly for 3d contexts also)
    // 1.0: switch that enables translations (vector*matrix math)
    world_position = MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0);
}

float get_norm_noise_at(vec2 world_pos) {
	// need (x,y) normalized as a proportion of world_max_pos
    vec2 pos_norm = world_pos / world_max_pos;
	
    float texel = texture(
        noise_map,
        pos_norm
    ).r;

    return texel;
}

vec3 sample_atlas_at(int tile_index, vec2 uv) {
	float off_x_norm = float(tile_index % atlas_width) / float(atlas_width);
	float off_y_norm = float(tile_index / atlas_width) / float(atlas_height);
	
	float uv_frac_x = uv.x / float(atlas_width);
	float uv_frac_y = uv.y / float(atlas_height); 
	
	return texture(
		atlas_texture,
		vec2(off_x_norm + uv_frac_x, off_y_norm + uv_frac_y)
	).rgb;
}

void fragment() {
    float norm_noise = get_norm_noise_at(world_position.xy);

    float scaled_noise = norm_noise * float(amount_tiles);

    int tile_index = int(floor(scaled_noise));
    tile_index = clamp(tile_index, 0, amount_tiles - 1);

	int blend_index;
    
	float blend = abs(fract(scaled_noise) - 0.5) * 2.0;
	if (blend > 0.5) {
		blend_index = tile_index + 1;
		if (blend_index == amount_tiles) {
			blend_index = 0;
		}
	} else {
		blend_index = tile_index - 1;
		if (blend_index == -1) {
			blend_index = amount_tiles - 1;
		}
	}

    vec4 base_color = texture(TEXTURE, UV);
    vec3 blend_color = sample_atlas_at(blend_index, UV);

    COLOR = vec4(
        mix(base_color.rgb, blend_color, blend),
        base_color.a
    );
}