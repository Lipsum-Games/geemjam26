using Godot;

public partial class DnaPaneel : PanelContainer
{
	Vector2 _drukPositie;
	JoekelSim Sim => GetTree().CurrentScene.GetNodeOrNull<JoekelSim>("JoekelSim");

	// elke frame (ook tijdens pauze): info van de geselecteerde joekel in de snelle sim bijwerken
	public override void _Process(double delta) => Sim?.ToonGeselecteerde();

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		var sluit = new Button
		{
			// knopje om DNA paneel te kunnen closen.
			Text = "X",
			FocusMode = FocusModeEnum.None,
			SizeFlagsHorizontal = SizeFlags.ShrinkEnd,
			SizeFlagsVertical = SizeFlags.ShrinkBegin,
		};
		sluit.Pressed += Sluit;
		AddChild(sluit);

		var volg = new Button
		{
			Text = "Volg",
			FocusMode = FocusModeEnum.None,
			SizeFlagsHorizontal = SizeFlags.ShrinkEnd,
			SizeFlagsVertical = SizeFlags.ShrinkEnd,
		};
		volg.Pressed += () =>
		{
			Node2D doel = Sim?.Volgpunt;
			if (doel != null)
				(GetViewport().GetCamera2D() as Camera)?.Volg(doel);
		};
		AddChild(volg);
	}

	void Sluit()
	{
		Sim?.Deselecteer();
		Visible = false;
		(GetViewport().GetCamera2D() as Camera)?.StopVolgen();
	}

	public override void _UnhandledInput(InputEvent e)
	{
		if (e is not InputEventMouseButton { ButtonIndex: MouseButton.Left } knop) return;

		if (knop.Pressed)
		{
			_drukPositie = knop.Position;
			return;
		}

		// als je sleept moet je niet closen
		if (knop.Position.DistanceTo(_drukPositie) > 5) return;

		// anders moet je checken of je closed of een nieuw scherm opent.
		var wereldPositie = GetViewport().GetCamera2D().GetGlobalMousePosition();
		if (Sim?.Kies(wereldPositie) == true) return;

		Sluit();
	}
}
