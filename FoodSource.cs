using Godot;
using System;

public partial class FoodSource : Node2D
{
	private float resourceCount = 50.0f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public float eat(double delta, float maxRes)
	{
		var res = Mathf.Min(10*(float)delta, resourceCount);
		res = Mathf.Min(res, maxRes);
		resourceCount -= res;
		if(resourceCount == 0 && res > 0){
			QueueFree();
		}
		return res;
	}
}
