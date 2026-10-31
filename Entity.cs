using Godot;
using System;

public partial class Entity : Node2D
{

	[Export] public float MinSpeed = 50f;
	[Export] public float MaxSpeed = 150f;
	[Export] public float TurnJitter = 10f;
	[Export] public float HungerMax = 10f;
	[Export] public float HungerMin = 1f;
	

	private float _heading;
	private float _speed;
	private float _hunger;
	
	public Dna Dna;

	public override void _Ready()
	{
		Dna ??= Dna.Random();
		_heading = GD.Randf() * Mathf.Tau;
		_speed = (float)GD.RandRange(MinSpeed, MaxSpeed);
		_hunger = HungerMin;
	}

	public override void _Process(double delta)
	{
		float dt = (float)delta;
		_heading += (float)GD.RandRange(-TurnJitter, TurnJitter) * dt;
		Position += Vector2.FromAngle(_heading) * _speed * dt * (HungerMax + HungerMin - _hunger)/HungerMax;
		Rotation = _heading;

		var area = GetViewportRect().Size;
		Position = new Vector2(Mathf.PosMod(Position.X, area.X), Mathf.PosMod(Position.Y, area.Y));
		_hunger = Mathf.Min(_hunger + (float)delta, HungerMax);
	}
}
