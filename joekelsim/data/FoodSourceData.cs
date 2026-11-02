using Godot;

// Een foodsource als struct om in arrays te ploppen voor effiency.
public struct FoodSourceData
{
	// unique id that never changes
	public int Id;

	public Vector2 Position;

	// hoeveel er nog te eten is
	public float ResourceCount;
}
