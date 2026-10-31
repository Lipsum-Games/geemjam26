using Godot;
using System;
using System.Collections.Generic;

public static class WorldTileAtlasCoords
{
	///
	/// THIS CODE IS INTENTIONALLY CURSED IN ITS LAYOUT
	/// it's a visual representation of how the atlas is ingedeeld.
	/// 
	public static readonly Vector2I Grass = new(0, 0);  public static readonly Vector2I Forest = new(1, 0);
	public static readonly Vector2I Water = new(0, 1);

	public static readonly List<Vector2I> List = new() {
		Grass, 
		Forest,
		Water
	};
}

public partial class WorldTiles : TileMapLayer
{
	public const int WORLD_SIZE_X = 1000;
	public const int WORLD_SIZE_Y = 1000;

	private const int TILE_ATLAS_TILE_SET_SOURCE_ID = 0;
	private readonly FastNoiseLite _perlinNoiseSource = InitPerlinNoise();


	public override void _Ready()
	{
		for (int y = 0; y < WORLD_SIZE_Y; y++) {
			for (int x = 0; x < WORLD_SIZE_X; x++) {
				// get atlas coordinates based on Perlin Noise value at this tile.
				float noise = _perlinNoiseSource.GetNoise2D(x, y);
				float noiseNormal = (noise + 1f) / 2f;
				int perlI = (int) (noiseNormal * (WorldTileAtlasCoords.List.Count));
				GD.Print("noisNorm", noiseNormal, "perli is ", perlI);
				var atlasCoords = WorldTileAtlasCoords.List[perlI];

				this.SetCell(
					new Vector2I(x, y),  // Grid position
					TILE_ATLAS_TILE_SET_SOURCE_ID,
					atlasCoords,
					0 // Alternative ID (don't care about this)
				);
			}
		}
	}

	private static FastNoiseLite InitPerlinNoise() {
		var n = new FastNoiseLite();

		n.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
		n.Frequency = 0.1f;
		n.Seed = (int)GD.Randi();

		return n;
	}
}
