using Godot;
using System;

public partial class FoodSource : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		AddToGroup("FoodSource");
		var area = GetViewportRect().Size;
		Position = new Vector2(area.X/2, area.Y/2);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
