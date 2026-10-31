using Godot;
using System;

public partial class Entity : Node2D
{

	[Export] public float MinSpeed = 50f;
	[Export] public float MaxSpeed = 150f;
	[Export] public float TurnJitter = 10f;

	private float _heading;
	private float _speed;

	public override void _Ready()
	{
		_heading = GD.Randf() * Mathf.Tau;
		_speed = (float)GD.RandRange(MinSpeed, MaxSpeed);
	}

	public override void _Process(double delta)
	{
		float dt = (float)delta;
		_heading += (float)GD.RandRange(-TurnJitter, TurnJitter) * dt;
		Position += Vector2.FromAngle(_heading) * _speed * dt;
		Rotation = _heading;

		var area = GetViewportRect().Size;
		Position = new Vector2(Mathf.PosMod(Position.X, area.X), Mathf.PosMod(Position.Y, area.Y));
	}
}
