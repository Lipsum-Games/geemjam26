using Godot;

// Een foodsource als struct om in arrays te ploppen voor effiency.
public struct FoodSourceData
{
	public Vector2 Position;

	// hoeveel er nog te eten is
	public float ResourceCount;
}
