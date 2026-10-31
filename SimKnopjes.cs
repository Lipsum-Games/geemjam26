using Godot;
using System;

public partial class SimKnopjes : HBoxContainer
{
	static readonly float[] Snelheden = { 1, 2, 4, 8, 16, 32, 64 };
	int _stap;
	Button _pauze;
	Label _snelheid;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Engine.MaxPhysicsStepsPerFrame = 128;

		_pauze = Knop("Pauze", TogglePauze);
		Knop("<", () => ZetStap(_stap - 1));
		_snelheid = new Label { Text = "1x" };
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

	void ZetStap(int stap)
	{
		_stap = Mathf.Clamp(stap, 0, Snelheden.Length - 1);
		Engine.TimeScale = Snelheden[_stap];
		_snelheid.Text = Snelheden[_stap] + "x";
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
