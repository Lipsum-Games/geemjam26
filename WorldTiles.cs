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

	public static readonly WorldTileType[] TileTypes = [
		WorldTileType.Forest,
		WorldTileType.Grass,
		WorldTileType.Water
	];
}

public class WorldTile {
	public int XPos;
	public int YPos;
	public WorldTileType Type;

	public float GetJoekelSpeedMultiplier(Joekel j) {
		switch (this.Type) {
			case WorldTileType.Grass:
				return 1f;
			case WorldTileType.Forest:
				return 0.7f; // todo: boomklimmen karaktereigenschap ofzo iets koesls
			case WorldTileType.Water:
				return Karaktereigenschappen.Zwem.Van(j.Dna);
		}

		return 1f;
	}
}

public partial class WorldTiles : TileMapLayer
{
	public const int WORLD_SIZE_X = 100;
	public const int WORLD_SIZE_Y = 100;
	private static readonly List<WorldTile> The_Tiles = new List<WorldTile>();

	private const int TILE_ATLAS_TILE_SET_SOURCE_ID = 0;
	private readonly FastNoiseLite _noiseSource = initNoiseSource();


	public static WorldTile GetTileAt(Vector2 pos) {
		int x = (int) pos.X;
		int y = (int) pos.Y;

		if (x < 0f || y < 0f) {
			return null;
		}

		if (x >= WORLD_SIZE_X || y >= WORLD_SIZE_Y) {
			return null;
		}

		GD.Print("fetching tile at ", x, ", ", y);
		return The_Tiles[y * WORLD_SIZE_X + x];
	}

	public override void _Ready()
	{
		for (int y = 0; y < WORLD_SIZE_Y; y++) {
			for (int x = 0; x < WORLD_SIZE_X; x++) {
				// get atlas coordinates based on Noise value at this tile.
				float noise       = _noiseSource.GetNoise2D(x, y); /* [-1,1] */
				float noiseNormal = (noise + 1f) / 2f;             /* [ 0,1] */
				int noiseI = (int) (noiseNormal * (WorldTileAtlasCoords.TileTypes.Length));

				var tileType = WorldTileAtlasCoords.TileTypes[noiseI];
				var atlasCoords = WorldTileAtlasCoords.Coordinates[tileType];

				this.SetCell(
					new Vector2I(x, y),  // Grid position
					TILE_ATLAS_TILE_SET_SOURCE_ID,
					atlasCoords,
					0 // Alternative ID (don't care about this)
				);

				The_Tiles.Add(new() {
					XPos = x,
					YPos = y,
					Type = tileType
				});
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
