using Godot;
using System;
using System.Collections.Generic;

public static class WorldTileAtlasCoords
{
	public static readonly Vector2I Grass = new(0, 0);
	public static readonly Vector2I Forest = new(1, 0);

	public static readonly List<Vector2I> List = new() {
		Grass, 
		Forest
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
				int perlI = (int) System.MathF.Floor((
					(_perlinNoiseSource.GetNoise2D(x, y) + 1.0f) / 2.0f) * 
					WorldTileAtlasCoords.List.Count
				);
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
		n.Frequency = 0.05f;
		n.Seed = (int)GD.Randi();

		return n;
	}
}
