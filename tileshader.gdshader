shader_type canvas_item;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
	// COLOR = vec4(1.0, 0.2, 0.2, 1.0);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}




/*
shader_type canvas_item;

varying vec2 tile_coords;

const vec2 TILE_SIZE = vec2(16.0, 16.0);

void vertex() {
    tile_coords = floor(VERTEX / TILE_SIZE);
}

void fragment() {
    COLOR = texture(TEXTURE, UV);
}
*/