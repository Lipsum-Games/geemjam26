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

		var fs = FoodSourceScene.Instantiate<Node2D>();
		AddChild(fs);

		for (int i = 0; i < Count; i++)
		{
			SpawnJoekel(new Vector2(GD.Randf() * area.X, GD.Randf() * area.Y), Dna.Random());

		}
	}

	public void SpawnJoekel(Vector2 Position, Dna Dna){
		// var e = new Joekel();
		var e = EntityScene.Instantiate<Joekel>();
		e.Position = Position;
		e.Dna = Dna;
		AddChild(e);

	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
