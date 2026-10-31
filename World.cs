using Godot;
using System;


public partial class World : Node2D
{
	[Export] public PackedScene EntityScene;
	[Export] public PackedScene FoodSourceScene;
	[Export] public int Count = 50;
	[Export] public Vector2 WorldSize = new(3000, 2000);

	void AddWall(StaticBody2D body, Vector2 normal, float distance)
	{
		body.AddChild(new CollisionShape2D { Shape = new WorldBoundaryShape2D { Normal = normal, Distance = distance } });
	}

	public override void _Draw()
	{
		DrawRect(new Rect2(Vector2.Zero, WorldSize), new Color(0.78f, 1, 1));
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// mooie muurtjes waar joekels niet doorheen kennen!!
		var walls = new StaticBody2D();
		AddChild(walls);
		AddWall(walls, new Vector2(0, 1), 0);
		AddWall(walls, new Vector2(1, 0), 0);
		AddWall(walls, new Vector2(0, -1), -WorldSize.Y);
		AddWall(walls, new Vector2(-1, 0), -WorldSize.X);

		var area = WorldSize;
		for (int i = 0; i < Count; i++)
		{
			SpawnFoodSource(new Vector2(GD.Randf() * area.X, GD.Randf() * area.Y));

		}

		for (int i = 0; i < Count; i++)
		{
			SpawnJoekel(new Vector2(GD.Randf() * area.X, GD.Randf() * area.Y), Dna.Random());

		}
	}

	public void SpawnFoodSource(Vector2 Position){
		var fs = FoodSourceScene.Instantiate<FoodSource>();
		fs.Position = Position;
		AddChild(fs);
	}

	public void SpawnJoekel(Vector2 Position, Dna Dna){
		// var e = new Joekel();

		
		var radius = Karaktereigenschappen.Chonkheid.Van(Dna);
		
		var space = GetWorld2D().DirectSpaceState;
		var success = false;
		var finalPosition = Position;

		for (int i = 0; i < 5; i++){
			var dir = GD.Randf() * Math.Tau;
			var candidatePosition = Position + new Vector2(((float) Math.Cos(dir)) * (radius * 2.1f), ((float) Math.Sin(dir)) * (radius * 2.1F));
			var query = new PhysicsShapeQueryParameters2D();
			query.Shape = new CircleShape2D {
				Radius = radius,
			};
			query.CollideWithBodies = true;
			query.CollideWithAreas = false;
			query.Transform = new Transform2D(0, candidatePosition);
			var result = space.CollideShape(query, 10);
			if (result.Count == 0){
				finalPosition = candidatePosition;
				success = true;
				break;
			}
		}
		if (success) {
			var e = EntityScene.Instantiate<Joekel>();
			e.Dna = Dna;
			e.Position = finalPosition;
			AddChild(e);
		}
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
