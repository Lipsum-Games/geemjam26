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
				return 0.1f * Mathf.Pow(2, zwemdiplomas);
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
	private static Texture2D _origineleAtlas;

	private FastNoiseLite _noiseSource;
	private const int NOISE_BLEND_FINENESS = 3;

	public override void _Ready()
	{
		The_Tiles.Clear();

		int noiseSeed = (int)GD.Randi();
		_noiseSource = initNoiseSource(noiseSeed);

		// resize tile / tile atlas / tile textue so it all fits neatly on the actual world tile size
		TileSet.TileSize = new Vector2I(WORLD_TILE_SIZE_X, WORLD_TILE_SIZE_Y);
		var atlas = (TileSetAtlasSource)TileSet.GetSource(0);
		// de originele atlas: bij het herladen van de scene is de TileSet dezelfde en al verkleind, dus altijd vanaf het origineel
		_origineleAtlas ??= atlas.Texture;
		atlas.Texture = GenerateResizedAtlasTexture(_origineleAtlas);
		TileSet.TileSize        = new Vector2I(WORLD_TILE_SIZE_X, WORLD_TILE_SIZE_Y);
		atlas.TextureRegionSize = new Vector2I(WORLD_TILE_SIZE_X, WORLD_TILE_SIZE_Y);

		// setup tile shader params
		ShaderMaterial material = (ShaderMaterial)Material;
		Vector2 worldMaxPos = new Vector2(World.WorldSize.X - 1, World.WorldSize.Y - 1);
		material.SetShaderParameter("world_max_pos", worldMaxPos);
		Vector2 worldTileSize = new Vector2(WORLD_TILE_SIZE_X, WORLD_TILE_SIZE_Y);
		material.SetShaderParameter("world_tile_size", worldTileSize);
		material.SetShaderParameter("atlas_texture", atlas.Texture);

		var noiseMap = Image.Create(
			WORLD_AMOUNT_TILES_X * NOISE_BLEND_FINENESS,
			WORLD_AMOUNT_TILES_Y * NOISE_BLEND_FINENESS,
			false, /* no mipmapping */
			Image.Format.Rf /* single red channel to pass single int per tile */
		);
		for (int noisey = 0; noisey < WORLD_AMOUNT_TILES_Y * NOISE_BLEND_FINENESS; noisey++) {
			for (int noisex = 0; noisex < WORLD_AMOUNT_TILES_X * NOISE_BLEND_FINENESS; noisex++) {
				// get atlas coordinates based on Noise value at this tile.
				float noise       = _noiseSource.GetNoise2D(((float) noisex) / ((float) NOISE_BLEND_FINENESS), ((float) noisey) / ((float) NOISE_BLEND_FINENESS)); /* [-1,1] */
				float noiseNormal = (noise + 1f) / 2f;             /* [ 0,1] */
				noiseMap.SetPixel(noisex, noisey, new Color(noiseNormal, 0, 0));

				if (!(noisex % NOISE_BLEND_FINENESS == 0 && noisey % NOISE_BLEND_FINENESS == 0))
					continue;

				int x = noisex / NOISE_BLEND_FINENESS;
				int y = noisey / NOISE_BLEND_FINENESS;
				
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

		var tileIndexTexture = ImageTexture.CreateFromImage(noiseMap);
		material.SetShaderParameter("noise_map", tileIndexTexture);
	} 
	
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

	private static FastNoiseLite initNoiseSource(int seed) {
		var n = new FastNoiseLite();

		n.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
		n.Frequency = 0.05f;
		n.Seed = seed;

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
