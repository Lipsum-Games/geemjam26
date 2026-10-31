using Godot;

public partial class DnaPaneel : PanelContainer
{
	Vector2 _drukPositie;

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
			if (Joekel.Geselecteerd != null)
				(GetViewport().GetCamera2D() as Camera)?.Volg(Joekel.Geselecteerd);
		};
		AddChild(volg);
	}

	void Sluit()
	{
		Joekel.Deselecteer();
		Visible = false;
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
		var vraag = new PhysicsPointQueryParameters2D { Position = wereldPositie };
		foreach (var raak in GetViewport().World2D.DirectSpaceState.IntersectPoint(vraag))
		{
			if (raak["collider"].AsGodotObject() is Joekel joekel)
			{
				joekel.Selecteer();
				return;
			}
		}

		Sluit();
	}
}
