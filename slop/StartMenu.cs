using Godot;

// Slop: startscherm. Gewoon starten, of een random wereld (random genetica) met een seed.
// Bij random laden we de scene opnieuw zodat de starters al met de nieuwe genetica geboren worden.
public partial class StartMenu : CanvasLayer
{
	static bool _gekozen;
	static string _seed;

	public override void _Ready()
	{
		Layer = 10;
		ProcessMode = ProcessModeEnum.Always;

		// headless (tests): geen menu, gewoon starten
		if (DisplayServer.GetName() == "headless") _gekozen = true;

		if (_gekozen)
		{
			// na een random keuze: laat zien in welke wereld we zitten
			if (_seed != null)
			{
				var label = new Label { Text = $"Wereld: {_seed}", Position = new Vector2(10, 40) };
				AddChild(label);
			}
			return;
		}

		GetTree().Paused = true;
		var paneel = new PanelContainer();
		paneel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
		paneel.GrowHorizontal = Control.GrowDirection.Both;
		paneel.GrowVertical = Control.GrowDirection.Both;
		var vak = new VBoxContainer { CustomMinimumSize = new Vector2(360, 0) };
		vak.AddChild(new Label { Text = "Joekels", HorizontalAlignment = HorizontalAlignment.Center });

		var start = new Button { Text = "Start (vaste genetica)" };
		start.Pressed += () => { _gekozen = true; GetTree().Paused = false; QueueFree(); };
		vak.AddChild(start);

		var rij = new HBoxContainer();
		var seed = new LineEdit { Text = RandomGenetica.NieuweSeed(), SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
		var dobbel = new Button { Text = "Andere" };
		dobbel.Pressed += () => seed.Text = RandomGenetica.NieuweSeed();
		var random = new Button { Text = "Random wereld" };
		random.Pressed += () =>
		{
			_gekozen = true;
			_seed = seed.Text;
			RandomGenetica.Pas(_seed);
			GetTree().Paused = false;
			GetTree().CallDeferred(SceneTree.MethodName.ReloadCurrentScene);
		};
		rij.AddChild(seed);
		rij.AddChild(dobbel);
		rij.AddChild(random);
		vak.AddChild(rij);

		paneel.AddChild(vak);
		AddChild(paneel);
	}
}
