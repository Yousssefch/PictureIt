shader_type canvas_item;
uniform float speed = .1;
varying float x;
varying float y;

varying float alp;

void vertex() {
	// Called for every vertex the material is visible on.
	UV.x += TIME * speed;
	UV.y += TIME * speed;
	x = VERTEX.x - 575.0;
	y = VERTEX.y - 324.0;
}

void fragment() {
	// Called for every pixel the material is visible on.
	alp = (x * SCREEN_PIXEL_SIZE.y) + ((y*y) * SCREEN_PIXEL_SIZE.y);
	COLOR.a = (1.0 - (alp / 175.0)) * COLOR.a;
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
