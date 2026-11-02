using Godot;

public partial class JoekelSim
{
	Label seizoenLabel;
	ProgressBar jaarBalk;
	CanvasModulate wereldTint;

	static readonly Color WinterTint = new(0.82f, 0.88f, 1f);
	static readonly Color ZomerTint = new(1f, 0.93f, 0.8f);

	// schemer over de wereld om de temperature gradient te laten zien
	void MaakKlimaatTekening()
	{
		var g = new Gradient();
		g.Offsets = [0f, 0.5f, 1f];
		g.Colors = [new Color(0.55f, 0.75f, 1f, 0.22f), new Color(1, 1, 1, 0f), new Color(1f, 0.6f, 0.3f, 0.22f)];
		var textuur = new GradientTexture2D { Gradient = g, FillFrom = new Vector2(0, 0), FillTo = new Vector2(0, 1), Width = 4, Height = 256 };
		AddChild(new Sprite2D { Name = "KlimaatTekening", Texture = textuur, Centered = false, Scale = World.WorldSize / new Vector2(4, 256) });
	}

	void MaakSeizoenTekening()
	{
		// tint de wereld een beetje om de season feel te geven
		wereldTint = new CanvasModulate();
		AddChild(wereldTint);

		var laag = new CanvasLayer();
		var paneel = new PanelContainer();
		paneel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.CenterTop);
		paneel.GrowHorizontal = Control.GrowDirection.Both;
		var vak = new VBoxContainer();
		seizoenLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center };
		jaarBalk = new ProgressBar { ShowPercentage = false, CustomMinimumSize = new Vector2(180, 6), MaxValue = 1 };
		vak.AddChild(seizoenLabel);
		vak.AddChild(jaarBalk);
		paneel.AddChild(vak);
		laag.AddChild(paneel);
		AddChild(laag);
	}

	void TekenSeizoen()
	{
		float temperatuur = Temperatuur;
		seizoenLabel.Text = $"{Seizoen} · {temperatuur:0}°C (noord {temperatuur - NoordZuidVerschil:0}, zuid {temperatuur + NoordZuidVerschil:0}) · jaar {Jaar}";
		jaarBalk.Value = JaarFase;

		// van koud (0) naar warm (1): eerst winterblauw naar wit, dan wit naar zomerwarm
		float warm = Warmte;
		Color kleur = warm < 0.5f ? WinterTint.Lerp(Colors.White, warm * 2) : Colors.White.Lerp(ZomerTint, (warm - 0.5f) * 2);
		wereldTint.Color = kleur;
		seizoenLabel.AddThemeColorOverride("font_color", warm < 0.5f ? new Color(0.6f, 0.8f, 1f).Lerp(Colors.White, warm * 2) : Colors.White.Lerp(new Color(1f, 0.7f, 0.35f), (warm - 0.5f) * 2));
	}
}
