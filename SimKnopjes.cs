using Godot;
using System;

public partial class SimKnopjes : HBoxContainer
{
	static readonly float[] Snelheden = { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512 };
	int _stap;
	Button _pauze;
	Label _snelheid;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		Engine.MaxPhysicsStepsPerFrame = 256;

		_pauze = Knop("Pauze", TogglePauze);
		Knop("<", () => ZetStap(_stap - 1));
		_snelheid = new Label { Text = "1x", CustomMinimumSize = new Vector2(60, 0), HorizontalAlignment = HorizontalAlignment.Center };
		AddChild(_snelheid);
		Knop(">", () => ZetStap(_stap + 1));
	}

	Button Knop(string tekst, Action actie)
	{
		var knop = new Button { Text = tekst, FocusMode = FocusModeEnum.None };
		knop.Pressed += actie;
		AddChild(knop);
		return knop;
	}

	void TogglePauze()
	{
		GetTree().Paused = !GetTree().Paused;
		_pauze.Text = GetTree().Paused ? "Verder" : "Pauze";
	}

	// Godot's TimeScale alleen maakt elke physics-stap groter (bij 64x: 2 sim-seconden per stap), dan gedraagt de sim
	// zich anders. Daarom schalen we het aantal stappen mee: elke stap blijft 1/basis sim-seconde, er komen er
	// gewoon meer per echte seconde. Kost dus echt k× zoveel rekenwerk; haalt de CPU het niet, dan loopt de sim
	// langzamer dan de knop zegt, maar wel correct.
	int _basisTicks;

	void ZetStap(int stap)
	{
		if (_basisTicks == 0) _basisTicks = Engine.PhysicsTicksPerSecond;
		_stap = Mathf.Clamp(stap, 0, Snelheden.Length - 1);
		Engine.TimeScale = Snelheden[_stap];
		Engine.PhysicsTicksPerSecond = _basisTicks * (int)Snelheden[_stap];
		_snelheid.Text = Snelheden[_stap] + "x";
	}

	// eerlijk label: toon de echt gehaalde snelheid als de CPU het niet bijhoudt, bv. "64x (≈16x)"
	ulong _meetStart, _meetFrames;
	public override void _Process(double delta)
	{
		if (_basisTicks == 0 || GetTree().Paused) { _meetStart = 0; return; }
		ulong nu = Time.GetTicksMsec(), frames = Engine.GetPhysicsFrames();
		if (_meetStart == 0) { _meetStart = nu; _meetFrames = frames; return; }
		double echt = (nu - _meetStart) / 1000.0;
		if (echt < 0.5) return;
		double gehaald = (frames - _meetFrames) / (double)_basisTicks / echt;
		float doel = Snelheden[_stap];
		_snelheid.Text = gehaald < doel * 0.9 ? $"{doel}x (≈{gehaald:0}x)" : $"{doel}x";
		_meetStart = nu; _meetFrames = frames;
	}

	public override void _UnhandledInput(InputEvent e)
	{
		if (e is not InputEventKey { Pressed: true, Echo: false } toets) return;

		switch (toets.Keycode)
		{
			case Key.Space: TogglePauze(); break;
			case Key.Right: case Key.Up: ZetStap(_stap + 1); break;
			case Key.Left: case Key.Down: ZetStap(_stap - 1); break;
			default: return;
		}
		GetViewport().SetInputAsHandled();
	}
}
