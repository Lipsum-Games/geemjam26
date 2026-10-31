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
	[Export] public float SpringInterval = 2.6f;
	[Export] public float Frictie = 1f;
	// performance: onder deze snelheid (px/s) staat een joekel stil en slaan we MoveAndSlide over.
	[Export] public float RustDrempel = 10f;

	// Muur stuiter dingetjes
	[Export] public float ZachteStuiter = 0.4f;
	[Export] public float HardeStuiter = 0.9f;
	[Export] public float MuurHeal = 2f;
	[Export] public float MuurSchade = 0.02f;

	// geur exports voor wanneer heal en plateau enzo
	[Export] public float GeurHealZone = 0.1f;
	[Export] public float GeurPlateau = 0.05f;
	[Export] public float GeurVolleSchade = 0.5f;
	[Export] public float GeurHealKracht = 0.3f;

	// optimization, niet elke frame aura applying is goedkoper
    [Export] public int AuraElkeNFrames = 16;


	// de joekel die nu geselecteerd is, zodat we live health etc. kunnen opdaten in de window
	static Joekel _geselecteerd;
	public static Joekel Geselecteerd => _geselecteerd;

	private float _heading;
	private float _volgendeHeading;
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
	private float _springTimer;
	private float _vleeslustigheid;
	// Fumo: harde stuiter en heal
	// Garfield: zachte stuiter en heal
	// Gondola: zachte stuiter en damage
	// Kei: harde stuiter en damage
	enum StuiterType { Garfield, Fumo, Kei, Gondola }
	private StuiterType _stuiterType;
	public string StuiterNaam => _stuiterType.ToString(); // slop-hook: gelezen door slop/Statistiek + slop/Clusters

	// geurtje
	private float _ranzigBereik;
	private float _ranzigKracht;
	private float _geur;
	public float Geur => _geur;
	static readonly GradientTexture2D MistTextuur = MaakRanzigheidsMist();


	public Dna Dna;

	private readonly Color[] _kleuren_bandjes = new Color[3];

	public override void _Ready()
	{
		_heading = GD.Randf() * Mathf.Tau;
		_volgendeHeading = _heading;
		_springTimer = GD.Randf() * SpringInterval;
		_kachow = Karaktereigenschappen.Kachow.Van(Dna);
		_turnJitter = Karaktereigenschappen.TurnJitter.Van(Dna);
		_detectionArea = GetNode<Area2D>("Area2D");
		_detectionArea.AreaEntered += OnAreaEntered;
		_detectionArea.AreaExited += OnAreaExited;
		_vleeslustigheid = Karaktereigenschappen.Vleeslustigheid.Van(Dna);
		_isEatingSource = null;
		_hunger = (HungerMin + HungerMax) / 2;
		_doodgaansSnelheidEnAlgemeneGezondheid = Karaktereigenschappen.DoodgaansSnelheidEnAlgemeneGezondheid.Van(Dna);
		// Joekeltjes hun maximale gezondheid scalet gewoon lekker met het DoodgaansSnelheidEnAlgemeneGezondheid gen.
		_maximaleGezondheid = Karaktereigenschappen.DoodgaansSnelheidEnAlgemeneGezondheid.Van(Dna);
		_huidigeGezonheid = _maximaleGezondheid;
		_chonkheid = Karaktereigenschappen.Chonkheid.Van(Dna);
		GetNode<CollisionShape2D>("Area2D/CollisionShape2D").Shape = new CircleShape2D { Radius = _chonkheid + 2 };
		GetNode<CollisionShape2D>("CollisionShape2D").Shape = new CircleShape2D { Radius = _chonkheid };
		for (int r = 0; r < 3; r++)
			_kleuren_bandjes[r] = new Color((Dna[r, 0] + 1) / 2, (Dna[r, 1] + 1) / 2, (Dna[r, 2] + 1) / 2);

		float m = Karaktereigenschappen.Murigheid.Van(Dna);
			_stuiterType = m < -0.4f ? StuiterType.Fumo : m < 0f ? StuiterType.Garfield : m < 0.4f ? StuiterType.Kei : StuiterType.Gondola;

		// geur init
		_ranzigBereik = Karaktereigenschappen.RanzigheidAuraBereik.Van(Dna);
		_ranzigKracht = Karaktereigenschappen.RanzigheidAuraKracht.Van(Dna);
		_geur = Karaktereigenschappen.RanzigheidAuraGeur.Van(Dna);
		if (_ranzigKracht > 0)
		{
			_auraVraag = new PhysicsShapeQueryParameters2D { Shape = new CircleShape2D { Radius = _ranzigBereik }, CollideWithAreas = false };
			_auraTeller = GD.RandRange(0, AuraElkeNFrames - 1); // spreiden: niet alle joekels in hetzelfde frame and gaat perfromance kaduk
		}
	}

	public void RegelMuurStuiter(Vector2 voor) {
		for (int i = 0; i < GetSlideCollisionCount(); i++)
			{
				var botsing = GetSlideCollision(i);
				if (botsing.GetCollider() is not StaticBody2D) continue;

				var n = botsing.GetNormal();
				float klap = -voor.Dot(n);

				switch (_stuiterType)
				{
					case StuiterType.Garfield:
						Velocity = voor.Bounce(n) * HardeStuiter;
						_huidigeGezonheid = Mathf.Min(_huidigeGezonheid + MuurHeal, _maximaleGezondheid);
						break;
					case StuiterType.Fumo:
						Velocity = voor.Bounce(n) * ZachteStuiter;
						_huidigeGezonheid = Mathf.Min(_huidigeGezonheid + MuurHeal, _maximaleGezondheid);
						break;
					case StuiterType.Kei:
						Velocity = voor.Bounce(n) * ZachteStuiter;
						_huidigeGezonheid -= klap * MuurSchade;
						break;
					case StuiterType.Gondola:
						Velocity = voor.Bounce(n) * HardeStuiter;
						_huidigeGezonheid -= klap * MuurSchade;
						break;
				}

				_volgendeHeading = voor.Bounce(n).Angle();
				break;
			}
		}

	private float LynxEffect(float verschil)
		{
			// de lynx effect scalet geur damage/heal accoridng to exported values helemaal boven
			if (verschil < GeurHealZone)
				return -GeurHealKracht * (1 - verschil / GeurHealZone);

			float start = GeurHealZone + GeurPlateau;
			return Mathf.Max(0f, (verschil - start) / (GeurVolleSchade - start));
		}



	private PhysicsShapeQueryParameters2D _auraVraag;
	private int _auraTeller;

	private void RanzigeAura(float dt)
	{
		_auraVraag.Transform = new Transform2D(0, GlobalPosition);
		foreach (var raak in GetWorld2D().DirectSpaceState.IntersectShape(_auraVraag, 64))
		{
			if (raak["collider"].AsGodotObject() is not Joekel ander || ander == this) continue;

			float afstand = GlobalPosition.DistanceTo(ander.GlobalPosition);
			if (afstand >= _ranzigBereik) continue; // niet ranzig genoeg

			// tot op welke hoogte je geroken kan worden, je geurkracht ofzo
			float kan_ik_je_ruiken_hoeveelheid = 1 - afstand / _ranzigBereik;
			// als jouw geur lijkt op die van de ander dan bentei ei misschien toch nie zo vies als ie dacht hé
			float eigenlijk_toch_niet_zo_ranzigheid = LynxEffect(Mathf.Abs(_geur - ander.Geur));
			// pas jouw ranzigheid toe op de gezondheid van de wederpartij
			ander._huidigeGezonheid -= _ranzigKracht * kan_ik_je_ruiken_hoeveelheid * eigenlijk_toch_niet_zo_ranzigheid * dt;
			ander._huidigeGezonheid = Mathf.Min(ander._huidigeGezonheid, ander._maximaleGezondheid);
		}
	}

	// roep deze functie om de joekel op te ruimen
	public void MaakDoodEnRuimOp() {
		if (_geselecteerd == this)
		{
			_geselecteerd = null;
			GetTree().CurrentScene.GetNode<Control>("%DnaLabel").GetParent<Control>().Visible = false;
		}
		QueueFree();
	}


	public override void _PhysicsProcess(double delta)
	{
		age += 1;
		// Als deze joekel is selected update the toon window.
		if (_geselecteerd == this) Toon();
		if (_huidigeGezonheid <= 0) {
			// DOOD!!!!!
			MaakDoodEnRuimOp();
			return;
		}
		float dt = (float)delta;

		(float direction, float velocity) = _behaviour.DoeDingen(this, dt);

		_springTimer -= dt;
		if (_springTimer <= 0)
		{
			// jump!!
			_heading = direction;
			Velocity += velocity * GetVelocityVector();
			_volgendeHeading = direction;
			_springTimer = SpringInterval;
		}

		Velocity *= Mathf.Exp(-Frictie * dt);
		if (Velocity.Length() > RustDrempel)
		{
			var voor = Velocity;
			MoveAndSlide();
			// alleen na een echte MoveAndSlide: anders leest hij de oude botsing van vorige keer opnieuw
			RegelMuurStuiter(voor);
		}
		// Zet de velocity op nul als we al langzaam genoeg gaan.
		else Velocity = Vector2.Zero;

		foreach (var body in _detectionArea.GetOverlappingBodies())
		{
			if (body is Joekel ander && ander != this)
				if (_behaviour.CheckReproduce() && Dna.Similarity(Dna, ander.Dna) < 6)
				{
					Reproduce(ander);
				} else if (Dna.Similarity(Dna, ander.Dna) > 6){
					// ATTACK
					ander._huidigeGezonheid -= _chonkheid;
					_hunger -= Math.Max((_vleeslustigheid - 0.5f), 0);
				}
				continue;
				// hier kan je op de andere joekel leuke code toepassen voor interacties.
				// ze botsen voor meerdere frames dus dit moet denk ik niet te hoog
				// uitgecomment want was beetje te veel nu
//				_huidigeGezonheid -= 6f * dt; // ook leuk idee om dit te scalen met een robuustheid ofzo wettewel
		}

		if (_ranzigKracht > 0) {
			if (_auraVraag != null && ++_auraTeller >= AuraElkeNFrames) {
				_auraTeller = 0;
				RanzigeAura(dt * AuraElkeNFrames);
			}
		}

		float t = 1 - _springTimer / SpringInterval;
		Rotation = Mathf.LerpAngle(_heading, _volgendeHeading, Mathf.SmoothStep(0.7f, 1f, t));

		CalculateHunger(delta);
	}

	static GradientTexture2D MaakRanzigheidsMist()
	{
		var g = new Gradient();
		g.Offsets = [0f, 0.25f, 0.5f, 0.75f, 1f];
		g.Colors = [new Color(1, 1, 1, 1f), new Color(1, 1, 1, 0.82f), new Color(1, 1, 1, 0.46f), new Color(1, 1, 1, 0.17f), new Color(1, 1, 1, 0f)];
		return new GradientTexture2D
		{
			Gradient = g,
			Fill = GradientTexture2D.FillEnum.Radial,
			FillFrom = new Vector2(0.5f, 0.5f),
			FillTo = new Vector2(1f, 0.5f),
			Width = 128,
			Height = 128,
		};
	}


	public override void _Draw() {

		if (_ranzigKracht > 0)
		{
			var geurKleur = Color.FromHsv((_geur + 1) / 2 * 0.8f, 0.7f, 1f);
			geurKleur.A = _ranzigKracht / 8 * 0.5f;
			DrawTextureRect(MistTextuur, new Rect2(-_ranzigBereik, -_ranzigBereik, _ranzigBereik * 2, _ranzigBereik * 2), false, geurKleur);
		}

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

	public float GetVelocityVal()
	{
		float result = _kachow; 										    // joekel speed multiplier
		float honger = (_hunger - HungerMin) / (HungerMax - HungerMin);
		// 100-50% gevuld is geen penatly, daarna linear van 100% naar 70% speed
		result *= 1f - 0.3f * Mathf.Clamp((honger - 0.5f) / 0.5f, 0f, 1f);    // hunger discount
		var joekelTile = WorldTiles.GetTileAt(this.GlobalPosition);
		if (joekelTile != null) {
			result *= joekelTile.GetJoekelSpeedMultiplier(this); 	// modifier from the tile the joekel is on
		}
		return result;
	}

	private Vector2 GetVelocityVector() {
		var result = Vector2.FromAngle(_heading); 				    // unit vector
		result *= GetVelocityVal();
		return result;
	}
	
	private void CalculateHunger(double delta)
	{
		if (_isEatingSource != null)
		{
			var eating = _isEatingSource.eat(delta, (_hunger - HungerMin) * Math.Max(((_vleeslustigheid - 0.5f) * - 1), 0)) ;
			_huidigeGezonheid += 2f*eating;
			_huidigeGezonheid = Mathf.Min(_huidigeGezonheid, _maximaleGezondheid);
			_hunger -= eating;
		}
		_hunger = Mathf.Min(_hunger + Karaktereigenschappen.Chonkheid.Van(Dna) * (float)delta / 100 , HungerMax);

		float honger = (_hunger - HungerMin) / (HungerMax - HungerMin);
		if(honger>0.7f){
			_huidigeGezonheid -= 5f * (float)delta * honger*honger;
		}
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

	// selecteren gaat via DnaPaneel (physics-query), zodat het ook tijdens pauze werkt
	public void Selecteer()
	{
		_geselecteerd = this;
		Toon();
		if (GetViewport().GetCamera2D() is Camera camera && camera.VolgtIemand)
				camera.Volg(this);
	}

	public static void Deselecteer() => _geselecteerd = null;

	void Toon()
		{
			var s = "DNA\n";
			for (int r = 0; r < Dna.N; r++)
				s += Dna[r, 0].ToString("F2") + "  " + Dna[r, 1].ToString("F2") + "  " + Dna[r, 2].ToString("F2") + "\n";

			s += "\n";
			foreach (var k in Karaktereigenschap.Alle)
				if (k == Karaktereigenschappen.Murigheid)
						s += "Murigheid: " + _stuiterType + "\n";
					else
						s += k.Naam + ": " + k.Van(Dna) + "\n";

			s += "Gezond: " + _huidigeGezonheid + "/" + _maximaleGezondheid +"\n";

			var label = GetTree().CurrentScene.GetNode<Label>("%DnaLabel");
			label.Text = s;
			label.GetParent<Control>().Visible = true;
	}

	
	private void Reproduce(Joekel other){
		if (!IsNodeReady() || Dna == null || age < AgeOfConsent || _hunger > ReproductionHunger)
			return;
		((World) GetParent()).SpawnJoekel(Position, Dna.Combine(Dna, other.Dna));
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
