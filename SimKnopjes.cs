using Godot;
using System;

public partial class SimKnopjes : HBoxContainer
{
	
	static readonly float[] Snelheden = { 1, 2, 4, 8, 16, 32, 64 };
	int _stap;
	
	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Engine.MaxPhysicsStepsPerFrame = 128;
		
		var pauze = new Button { Text = "Pauze" };
		pauze.Pressed += () =>
		{
			GetTree().Paused = !GetTree().Paused;
			pauze.Text = GetTree().Paused ? "Verder" : "Pauze";
		};
		AddChild(pauze);
		
		var snelheid = new Button { Text = "1x" };
		snelheid.Pressed += () =>
		{
			_stap = (_stap + 1) % Snelheden.Length;
			Engine.TimeScale = Snelheden[_stap];
			snelheid.Text = Snelheden[_stap] + "x";
		};
		AddChild(snelheid);
	}
}
