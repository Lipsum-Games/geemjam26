using Godot;
using System;
using System.Collections.Generic;

public enum WorldTileType
{
    Forest,
    Grass,
    Water
}

public static class WorldTileAtlasCoords
{
    //
	/// THIS CODE IS INTENTIONALLY CURSED IN ITS LAYOUT
	/// it's a visual representation of how the atlas is ingedeeld.
	/// 
    public static readonly Dictionary<WorldTileType, Vector2I> Coordinates = new()
    {
        { WorldTileType.Forest, new(0, 0) },         { WorldTileType.Grass,  new(1, 0) },
        { WorldTileType.Water,  new(0, 1) }
    };
}

public class WorldTile {
    public int XPos;
    public int YPos;
    public WorldTileType Type;

    public float GetJoekelSpeedMultiplier() {
        switch (this.Type) {
            case Grass:
                return 1;
            case 

        }
    }
}

public partial class WorldTiles : TileMapLayer
{
	public const int WORLD_SIZE_X = 100;
	public const int WORLD_SIZE_Y = 100;

	private const int TILE_ATLAS_TILE_SET_SOURCE_ID = 0;
	private readonly FastNoiseLite _noiseSource = initNoiseSource();

    public static Vector2 GetTileAt(Vector2 pos)

	public override void _Ready()
	{
		for (int y = 0; y < WORLD_SIZE_Y; y++) {
			for (int x = 0; x < WORLD_SIZE_X; x++) {
				// get atlas coordinates based on Perlin Noise value at this tile.
				float noise       = _noiseSource.GetNoise2D(x, y); /* [-1,1] */
				float noiseNormal = (noise + 1f) / 2f;                   /* [ 0,1] */
				int noiseI = (int) (noiseNormal * (WorldTileAtlasCoords.List.Count));
				var atlasCoords = WorldTileAtlasCoords.Coordinates[noiseI];

				this.SetCell(
					new Vector2I(x, y),  // Grid position
					TILE_ATLAS_TILE_SET_SOURCE_ID,
					atlasCoords,
					0 // Alternative ID (don't care about this)
				);
			}
		}
	} 

	private static FastNoiseLite initNoiseSource() {
		var n = new FastNoiseLite();

		n.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
		n.Frequency = 0.05f;
		n.Seed = (int)GD.Randi();

		return n;
	}
}
