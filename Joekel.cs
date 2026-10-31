using System;
using Geemjam26ShipsumEvolve.genetica;
using Godot;
using static Activations;

public partial class Joekel : CharacterBody2D
{

	[Export] public float HungerMax = 10f;
	[Export] public float HungerMin = 1f;
	[Export] public float ReproductionHunger = 2f;
	[Export] public float ReproductionHungerCost = 4f;
	[Export] public int AgeOfConsent = 200;


	// de joekel die nu geselecteerd is, zodat we live health etc. kunnen opdaten in de window
	static Joekel _geselecteerd;

	private float _heading;
	private float _kachow;
	private float _hunger;
	private float _zwem;
	private float _turnJitter;
	private Behaviour _behaviour = new Behaviour();
	
	private Area2D _detectionArea;
	private FoodSource _isEatingSource;
	private float _doodgaansSnelheidEnAlgemeneGezondheid;
	private int age;
	private float _maximaleGezondheid;
	private float _huidigeGezonheid;
	private float _chonkheid;

	public Dna Dna;

	private readonly Color[] _kleuren_bandjes = new Color[3];

	public override void _Ready()
	{
		_heading = GD.Randf() * Mathf.Tau;
		_kachow = Karaktereigenschappen.Kachow.Van(Dna);
		_turnJitter = Karaktereigenschappen.TurnJitter.Van(Dna);
		_detectionArea = GetNode<Area2D>("Area2D");
		_detectionArea.AreaEntered += OnAreaEntered;
		_detectionArea.AreaExited += OnAreaExited;
		_isEatingSource = null;
		_hunger = (HungerMin + HungerMax) / 2;
		_doodgaansSnelheidEnAlgemeneGezondheid = Karaktereigenschappen.DoodgaansSnelheidEnAlgemeneGezondheid.Van(Dna);
		// Joekeltjes hun maximale gezondheid scalet gewoon lekker met het DoodgaansSnelheidEnAlgemeneGezondheid gen.
		_maximaleGezondheid = Karaktereigenschappen.DoodgaansSnelheidEnAlgemeneGezondheid.Van(Dna) * 100;
		_huidigeGezonheid = _maximaleGezondheid;
		_chonkheid = Karaktereigenschappen.Chonkheid.Van(Dna);
		GetNode<CollisionShape2D>("Area2D/CollisionShape2D").Shape = new CircleShape2D { Radius = _chonkheid + 2 };
		GetNode<CollisionShape2D>("CollisionShape2D").Shape = new CircleShape2D { Radius = _chonkheid };
		for (int r = 0; r < 3; r++)
			_kleuren_bandjes[r] = new Color(Dna[r, 0], Dna[r, 1], Dna[r, 2]);
	}

	public override void _PhysicsProcess(double delta)
	{
		age += 1;
		// Als deze joekel is selected update the toon window.
		if (_geselecteerd == this) Toon();
		if (_huidigeGezonheid <= 0) {
			// DOOD!!!!!
			if (_geselecteerd == this)
			{
				_geselecteerd = null;
				GetTree().CurrentScene.GetNode<Control>("%DnaLabel").GetParent<Control>().Visible = false;
			}
			QueueFree();
			return;
		}
		float dt = (float)delta;

		(float direction, float velocity) = _behaviour.DoeDingen(this);

		if (_behaviour.CheckReproduce())
		{
			Reproduce();
		}
		
		_heading = direction;
		_heading += (float)GD.RandRange(-_turnJitter, _turnJitter) * dt;

		Velocity = velocity * GetVelocityVector();
		MoveAndSlide();

		foreach (var body in _detectionArea.GetOverlappingBodies())
		{
			if (body is Joekel ander && ander != this)
				// hier kan je op de andere joekel leuke code toepassen voor interacties.
				// ze botsen voor meerdere frames dus dit moet denk ik niet te hoog
				_huidigeGezonheid -= 6f * dt; // ook leuk idee om dit te scalen met een robuustheid ofzo wettewel
		}

		Rotation = _heading;

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
		float r = _chonkheid;
		float s = Mathf.DegToRad(150);
		Vector2[] Boog(float a0, float a1)
		{
				const int n = 16;
				var pts = new Vector2[n + 1];
				for (int k = 0; k <= n; k++)
						pts[k] = Vector2.FromAngle(Mathf.Lerp(a0, a1, k / (float)n)) * _chonkheid;
				return pts;
		}
		DrawColoredPolygon(Boog(0, s), _kleuren_bandjes[0]);
		DrawColoredPolygon([new Vector2(r, 0), ..Boog(s, Mathf.Tau - s)], _kleuren_bandjes[1]);
		DrawColoredPolygon(Boog(Mathf.Tau - s, Mathf.Tau), _kleuren_bandjes[2]);

	}

	private Vector2 GetVelocityVector() {
		var result = Vector2.FromAngle(_heading); 				    // unit vector 
		result *= _kachow; 										    // joekel speed multiplier
		result *= (HungerMax + HungerMin - _hunger) / HungerMax;    // hunger discount
		var joekelTile = WorldTiles.GetTileAt(this.GlobalPosition);
		if (joekelTile != null) {
			result *= joekelTile.GetJoekelSpeedMultiplier(this); 	// modifier from the tile the joekel is on
		}
		return result;
	}
	
	private void CalculateHunger(double delta)
	{
		if (_isEatingSource != null)
		{
			_hunger -= _isEatingSource.eat(delta, HungerMax-_hunger);
		}
		_hunger = Mathf.Min(_hunger + (float)delta, HungerMax);
		}

	private void OnAreaEntered(Area2D area)
	{
		if (area.IsInGroup("FoodSource"))
		{
			_isEatingSource = area.GetParent<FoodSource>();
		}
	}

	private void OnAreaExited(Area2D area)
	{
		if (area.IsInGroup("FoodSource"))
		{
			_isEatingSource = null;
		}
	}

	public override void _InputEvent(Viewport viewport, InputEvent e, int shapeIdx)
	{
		if (e is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
		{
			_geselecteerd = this;
			Toon();
		}
	}

	void Toon()
		{
			var s = "DNA\n";
			for (int r = 0; r < Dna.N; r++)
				s += Dna[r, 0].ToString("F2") + "  " + Dna[r, 1].ToString("F2") + "  " + Dna[r, 2].ToString("F2") + "\n";

			s += "\n";
			foreach (var k in Karaktereigenschap.Alle)
				s += k.Naam + ": " + k.Van(Dna) + "\n";

			s += "Gezond: " + _huidigeGezonheid + "/" + _maximaleGezondheid +"\n";

			var label = GetTree().CurrentScene.GetNode<Label>("%DnaLabel");
			label.Text = s;
			label.GetParent<Control>().Visible = true;
	}

	
	private void Reproduce(){
		if (!IsNodeReady() || Dna == null || age < AgeOfConsent)
			return;
		((World) GetParent()).SpawnJoekel(Position, Dna.Noisified());
		_hunger += ReproductionHungerCost;
	}
	public float GetHunger()
	{
		return _hunger;
	}
	public float getHeading()
	{
		return _heading;
	}
}
