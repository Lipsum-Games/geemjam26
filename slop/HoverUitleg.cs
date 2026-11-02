using Godot;
using System;

// Eigen hover-paneel i.p.v. Godot's tooltip: Godot verbergt een tooltip zodra de tekst verandert (en met een lopende
// sim verandert die steeds), en de standaard tooltip is doorzichtig met een proportioneel font.
// Dit paneel is ondoorzichtig, monospace (tabellen lijnen uit), staat naast de muis en ververst live.
public partial class HoverUitleg : PanelContainer
{
	const float Vertraging = 0.35f; // seconden stil hoveren voor het verschijnt

	Control _doel;
	Func<Vector2, string> _tekst;
	Label _label;
	Vector2 _muis;
	bool _erboven;
	float _rust;

	// koppel aan een control; tekst(positie in control) mag "" teruggeven = niks tonen
	public static void Koppel(Control doel, Func<Vector2, string> tekst)
	{
		var paneel = new HoverUitleg { _doel = doel, _tekst = tekst };
		doel.AddChild(paneel);
		doel.MouseEntered += () => { paneel._erboven = true; paneel._rust = 0; };
		doel.MouseExited += () => { paneel._erboven = false; paneel.Visible = false; };
		doel.GuiInput += e =>
		{
			if (e is not InputEventMouseMotion m) return;
			if (m.Position.DistanceTo(paneel._muis) > 3) paneel._rust = 0; // echt bewogen: opnieuw wachten
			paneel._muis = m.Position;
		};
	}

	public override void _Ready()
	{
		TopLevel = true;              // niet geclipt door de lijst/scroll, posities in venster-coördinaten
		MouseFilter = MouseFilterEnum.Ignore;
		Visible = false;
		ZIndex = 100;
		AddThemeStyleboxOverride("panel", new StyleBoxFlat
		{
			BgColor = new Color(0.08f, 0.08f, 0.1f, 0.97f),
			BorderColor = new Color(1, 1, 1, 0.25f),
			BorderWidthLeft = 1, BorderWidthRight = 1, BorderWidthTop = 1, BorderWidthBottom = 1,
			ContentMarginLeft = 10, ContentMarginRight = 10, ContentMarginTop = 8, ContentMarginBottom = 8,
		});
		_label = new Label();
		_label.AddThemeFontOverride("font", new SystemFont { FontNames = new[] { "Menlo", "Consolas", "DejaVu Sans Mono", "monospace" } });
		_label.AddThemeFontSizeOverride("font_size", 13);
		AddChild(_label);
	}

	public override void _Process(double delta)
	{
		if (!_erboven || !_doel.IsVisibleInTree()) { Visible = false; return; }
		_rust += (float)delta / Mathf.Max(0.0001f, (float)Engine.TimeScale); // echte tijd, ook op hoge sim-snelheid
		if (_rust < Vertraging) { Visible = false; return; }

		string tekst = _tekst(_muis);
		if (string.IsNullOrEmpty(tekst)) { Visible = false; return; }
		_label.Text = tekst;
		Size = Vector2.Zero; // krimp naar de inhoud
		Visible = true;

		// naast de muis, maar binnen het venster
		var scherm = GetViewportRect().Size;
		var pos = _doel.GetGlobalMousePosition() + new Vector2(16, 16);
		var maat = GetCombinedMinimumSize();
		if (pos.X + maat.X > scherm.X) pos.X = Mathf.Max(0, pos.X - maat.X - 32);
		if (pos.Y + maat.Y > scherm.Y) pos.Y = Mathf.Max(0, scherm.Y - maat.Y);
		GlobalPosition = pos;
	}
}
