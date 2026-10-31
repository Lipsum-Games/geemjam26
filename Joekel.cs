using Godot;
using System;

public partial class Joekel : Node2D
{

	[Export] public float HungerMax = 10f;
	[Export] public float HungerMin = 1f;

	private float _heading;
	private float _kachow;
	private float _hunger;
	private float _turnJitter;
	
	public Dna Dna;

	public override void _Ready()
	{
		Dna ??= Dna.Random();
		_heading = GD.Randf() * Mathf.Tau;
		_kachow = Karaktereigenschappen.Kachow.Van(Dna);
		_turnJitter = Karaktereigenschappen.TurnJitter.Van(Dna);
		_hunger = HungerMin;
	}

	public override void _Process(double delta)
	{
		float dt = (float)delta;
		_heading += (float)GD.RandRange(-_turnJitter, _turnJitter) * dt;
		Position += Vector2.FromAngle(_heading) * _kachow * dt * (HungerMax + HungerMin - _hunger)/HungerMax;
		Rotation = _heading;

		var area = GetViewportRect().Size;
		Position = new Vector2(Mathf.PosMod(Position.X, area.X), Mathf.PosMod(Position.Y, area.Y));
		_hunger = Mathf.Min(_hunger + (float)delta, HungerMax);
	}
}
