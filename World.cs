using Godot;
using System;

public partial class World : Node2D
{
	
	[Export] public PackedScene EntityScene;
	[Export] public PackedScene FoodSourceScene;
	[Export] public int Count = 50;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var area = GetViewportRect().Size;
			var fs = FoodSourceScene.Instantiate<Node2D>();
			AddChild(fs);
			for (int i = 0; i < Count; i++)
			{
				var e = EntityScene.Instantiate<Node2D>();

				e.Position = new Vector2(GD.Randf() * area.X, GD.Randf() * area.Y);
				AddChild(e);
			}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
