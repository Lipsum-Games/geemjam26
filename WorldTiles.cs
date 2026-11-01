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

	public float GetJoekelSpeedMultiplier(float zwemdiplomas) {
		switch (this.Type) {
			case WorldTileType.Grass:
				return 1f;
			case WorldTileType.Forest:
				return 0.7f; // todo: boomklimmen karaktereigenschap ofzo iets koesls
			case WorldTileType.Water:
				return zwemdiplomas;
		}

		return 1f;
	}
}

public partial class WorldTiles : TileMapLayer
{
	public static readonly int WORLD_AMOUNT_TILES_X = 100;
	public static readonly int WORLD_AMOUNT_TILES_Y = 100;
	public static readonly int WORLD_TILE_SIZE_X = ((int) World.WorldSize.X) / WORLD_AMOUNT_TILES_X;
	public static readonly int WORLD_TILE_SIZE_Y = ((int) World.WorldSize.Y) / WORLD_AMOUNT_TILES_Y;
	private static readonly List<WorldTile> The_Tiles = new List<WorldTile>();

	private const int TILE_ATLAS_TILE_SET_SOURCE_ID = 0;
	private readonly FastNoiseLite _noiseSource = initNoiseSource();

	public static WorldTile GetTileAt(Vector2 pos) {
		int x = Mathf.FloorToInt(pos.X / WORLD_TILE_SIZE_X);
		int y = Mathf.FloorToInt(pos.Y / WORLD_TILE_SIZE_Y);

		if (x < 0f || y < 0f) {
			return null;
		}

		if (x >= WORLD_AMOUNT_TILES_X || y >= WORLD_AMOUNT_TILES_Y) {
			return null;
		}

		int index = y * WORLD_AMOUNT_TILES_X + x;
		if (index >= The_Tiles.Count) return null; // (nog) geen tegels,
		return The_Tiles[index];
	}

	public override void _Ready()
	{
		The_Tiles.Clear();
		TileSet.TileSize = new Vector2I(WORLD_TILE_SIZE_X, WORLD_TILE_SIZE_Y);
		var atlas = (TileSetAtlasSource)TileSet.GetSource(0);
		atlas.Texture = GenerateResizedAtlasTexture(atlas.Texture);

		TileSet.TileSize = new Vector2I(
			WORLD_TILE_SIZE_X,
			WORLD_TILE_SIZE_Y
		);

		atlas.TextureRegionSize = new Vector2I(
			WORLD_TILE_SIZE_X,
			WORLD_TILE_SIZE_Y
		);

		for (int y = 0; y < WORLD_AMOUNT_TILES_Y; y++) {
			for (int x = 0; x < WORLD_AMOUNT_TILES_X; x++) {
				// get atlas coordinates based on Noise value at this tile.
				float noise       = _noiseSource.GetNoise2D(x, y); /* [-1,1] */
				float noiseNormal = (noise + 1f) / 2f;             /* [ 0,1] */
				int noiseI = (int) (noiseNormal * (WorldTileAtlasCoords.TileTypes.Length));
				noiseI = Math.Min(noiseI, WorldTileAtlasCoords.TileTypes.Length - 1); // noise precies 1.0 zou anders index 3 geven (bestaat niet)

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

	private ImageTexture GenerateResizedAtlasTexture(Texture2D originalAtlasTexture)
	{
		int originalTileSize = 100;

		int amountTilesX = originalAtlasTexture.GetWidth() / originalTileSize;
		int amountTilesY = originalAtlasTexture.GetHeight() / originalTileSize;

		var originalImage = originalAtlasTexture.GetImage();

		var resizedAtlasTexture = Image.CreateEmpty(
			amountTilesX * WORLD_TILE_SIZE_X,
			amountTilesY * WORLD_TILE_SIZE_Y,
			false,
			Image.Format.Rgba8
		);

		for (int y = 0; y < amountTilesY; y++)
		{
			for (int x = 0; x < amountTilesX; x++)
			{
				var tile = originalImage.GetRegion(new Rect2I(
					x * originalTileSize,
					y * originalTileSize,
					originalTileSize,
					originalTileSize
				));

				tile.Resize(
					WORLD_TILE_SIZE_X,
					WORLD_TILE_SIZE_Y,
					Image.Interpolation.Nearest
				);

				resizedAtlasTexture.BlitRect(
					tile,
					new Rect2I(0, 0, WORLD_TILE_SIZE_X, WORLD_TILE_SIZE_Y),
					new Vector2I(
						x * WORLD_TILE_SIZE_X,
						y * WORLD_TILE_SIZE_Y
					)
				);
			}
		}

		return ImageTexture.CreateFromImage(resizedAtlasTexture);
	}
}
