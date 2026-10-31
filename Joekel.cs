using Godot;
using static Activations;

public partial class Joekel : CharacterBody2D
{

	[Export] public float HungerMax = 10f;
	[Export] public float HungerMin = 1f;
	[Export] public float ReproductionHunger = 2f;
	[Export] public float ReproductionHungerCost = 4f;
	[Export] public int AgeOfConsent = 200;


	private float _heading;
	private float _kachow;
	private float _hunger;
	private float _turnJitter;
	private float _doodgaansSnelheidEnAlgemeneGezondheid;
	private int age;
	public Dna Dna;

	[Export] public float joekel_tekenRadius = 10f;
	private readonly Color[] _kleuren_bandjes = new Color[3];

	public override void _Ready()
	{
		// Dna ??= Dna.Random();
		_heading = GD.Randf() * Mathf.Tau;
		_kachow = Karaktereigenschappen.Kachow.Van(Dna);
		_turnJitter = Karaktereigenschappen.TurnJitter.Van(Dna);
		_hunger = (HungerMin + HungerMax) / 2;
		_doodgaansSnelheidEnAlgemeneGezondheid = Karaktereigenschappen.DoodgaansSnelheidEnAlgemeneGezondheid.Van(Dna);
		for (int r = 0; r < 3; r++)
			_kleuren_bandjes[r] = new Color(Dna[r, 0], Dna[r, 1], Dna[r, 2]);
	}

	public override void _PhysicsProcess(double delta)
	{
		age += 1;
		float dt = (float)delta;
		_heading += (float)GD.RandRange(-_turnJitter, _turnJitter) * dt;

		Velocity = Vector2.FromAngle(_heading) * _kachow * (HungerMax + HungerMin - _hunger) / HungerMax;
		MoveAndSlide();

		for (int i = 0; i < GetSlideCollisionCount(); i++)
		{
			if (GetSlideCollision(i).GetCollider() is Joekel ander)
			{
				continue;
				// hier kan je op de andere joekel leuke code toepassen voor interacties.
				}
		}

		Rotation = _heading;

		var area = GetViewportRect().Size;
		Position = new Vector2(Mathf.PosMod(Position.X, area.X), Mathf.PosMod(Position.Y, area.Y));
		CalculateHunger(delta);
	}

	public override void _Draw() {
//	    DrawSetTransform(Vector2.Zero, Mathf.Pi / 4);
//	    float w = 2 * Radius / 3;
//        for (int i = 0; i < 3; i++)
//        {
//            float x0 = -joekel_tekenRadius + i * w;
//            DrawColoredPolygon(Strip(x0, x0 + w), _kleuren_bandjes[i]);
//        }
//
//        DrawSetTransform(Vector2.Zero);
//        DrawArc(Vector2.Zero, Radius, 0, Mathf.Tau, 32, Colors.Black, 1.5f, true);
		DrawCircle(Vector2.Zero, 10, _kleuren_bandjes[0]);
		DrawCircle(Vector2.Zero, 7, _kleuren_bandjes[1]);
		DrawCircle(Vector2.Zero, 4, _kleuren_bandjes[2]);
	}
	
	private void CalculateHunger(double delta)
	{
		bool foodSourceClose = false;
		foreach (Node2D source in GetTree().GetNodesInGroup("FoodSource"))
		{
			float distance = GlobalPosition.DistanceTo(source.GlobalPosition);

			if (distance < 200)
			{
				foodSourceClose = true;
				break;
			}
		}
		if (foodSourceClose)
		{
			_hunger = Mathf.Max(_hunger - 10 * (float)delta, HungerMin);
		}
		if (_hunger < ReproductionHunger){
			Reproduce();
			_hunger = _hunger + ReproductionHungerCost;
		}
		_hunger = Mathf.Min(_hunger + (float)delta, HungerMax);
	}

	public override void _InputEvent(Viewport viewport, InputEvent e, int shapeIdx)
	{
		if (e is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
			Toon();
	}

	void Toon()
		{
			var s = "DNA\n";
			for (int r = 0; r < Dna.N; r++)
				s += Dna[r, 0].ToString("F2") + "  " + Dna[r, 1].ToString("F2") + "  " + Dna[r, 2].ToString("F2") + "\n";

			s += "\n";
			foreach (var k in Karaktereigenschap.Alle)
				s += k.Naam + ": " + k.Van(Dna) + "\n";

			var label = GetTree().CurrentScene.GetNode<Label>("%DnaLabel");
			label.Text = s;
			label.GetParent<Control>().Visible = true;
		}

	
	private void Reproduce(){
		if (!IsNodeReady() || Dna == null || age < AgeOfConsent)
			return;
		((World) GetParent()).SpawnJoekel(Position, Dna.Noisified());
	}
}
