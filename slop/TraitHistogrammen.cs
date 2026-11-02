using Godot;
using System.Collections.Generic;

// Slop: een histogram per trait (Karaktereigenschap.Alle), zelfde stijl als GenHistogrammen:
// staven gestapeld per groep (groepskleur). Bereik per trait = laagste..hoogste waarde in de populatie nu.
// Herberekend 2x per seconde (traits uitrekenen voor iedereen is niet gratis).
public partial class TraitHistogrammen : Control
{
	public Clusters Bron;
	const int Bakjes = 20;
	const int Kolommen = 4;

	// per trait: bereik + tellingen per groep, uit de laatste meting
	readonly List<(string naam, float lo, float hi, Dictionary<int, int[]> perGroep, int max)> _data = new();
	double _timer;

	public override void _Ready() => HoverUitleg.Koppel(this, HoverTekst);

	public override void _Process(double delta)
	{
		_timer -= delta;
		if (_timer <= 0 && Bron != null && IsVisibleInTree()) { _timer = 0.5; Meet(); }
		QueueRedraw();
	}

	void Meet()
	{
		_data.Clear();
		foreach (var k in Karaktereigenschap.Alle)
		{
			var waarden = new List<(float v, int groep)>();
			float lo = float.MaxValue, hi = float.MinValue;
			foreach (var (joekel, groep) in Bron.Indeling)
			{
				float v = k.Van(joekel.Dna);
				waarden.Add((v, groep));
				lo = Mathf.Min(lo, v); hi = Mathf.Max(hi, v);
			}
			if (waarden.Count == 0) continue;
			if (hi - lo < 1e-4f) { lo -= 0.5f; hi += 0.5f; } // iedereen gelijk: toch een bereik

			var perGroep = new Dictionary<int, int[]>();
			var totaal = new int[Bakjes];
			foreach (var (v, groep) in waarden)
			{
				int bakje = Mathf.Clamp((int)((v - lo) / (hi - lo) * Bakjes), 0, Bakjes - 1);
				if (!perGroep.TryGetValue(groep, out var t)) perGroep[groep] = t = new int[Bakjes];
				t[bakje]++;
				totaal[bakje]++;
			}
			int max = 1;
			foreach (var t in totaal) max = Mathf.Max(max, t);
			_data.Add((k.Naam, lo, hi, perGroep, max));
		}
	}

	public string HoverTekst(Vector2 p)
	{
		int rijen = Mathf.Max(1, (_data.Count + Kolommen - 1) / Kolommen);
		int c = Mathf.Clamp((int)(p.X / (Size.X / Kolommen)), 0, Kolommen - 1), r = Mathf.Clamp((int)(p.Y / (Size.Y / rijen)), 0, rijen - 1);
		int i = r * Kolommen + c;
		if (i >= _data.Count) return "";
		return $"{_data[i].naam}: hoe deze trait verdeeld is over alle joekels (van {_data[i].lo:0.##} tot {_data[i].hi:0.##}, laagste tot hoogste nu).\n" +
			"Hoogte = aantal joekels met die waarde. Kleuren = groepen (zie tab 'Groepen').";
	}

	public override void _Draw()
	{
		DrawRect(new Rect2(Vector2.Zero, Size), new Color(0, 0, 0, 0.4f));
		if (Bron == null || _data.Count == 0) return;

		var font = ThemeDB.FallbackFont;
		int rijen = (_data.Count + Kolommen - 1) / Kolommen;
		float celB = Size.X / Kolommen, celH = Size.Y / rijen;

		for (int i = 0; i < _data.Count; i++)
		{
			var (naam, lo, hi, perGroep, max) = _data[i];
			var cel = new Rect2((i % Kolommen) * celB + 6, (i / Kolommen) * celH + 20, celB - 12, celH - 34);
			DrawRect(cel, new Color(1, 1, 1, 0.05f));
			float staafB = cel.Size.X / Bakjes;

			for (int b = 0; b < Bakjes; b++)
			{
				float y = cel.End.Y;
				foreach (var g in Bron.Groepen)
				{
					if (!perGroep.TryGetValue(g.Id, out var t) || t[b] == 0) continue;
					float h = t[b] / (float)max * cel.Size.Y;
					y -= h;
					DrawRect(new Rect2(cel.Position.X + b * staafB, y, staafB - 1, h), g.Kleur);
				}
			}

			DrawString(font, new Vector2(cel.Position.X, cel.Position.Y - 4), naam, HorizontalAlignment.Left, cel.Size.X, 13, Colors.White);
			DrawString(font, new Vector2(cel.Position.X, cel.End.Y + 13), lo.ToString("0.##"), HorizontalAlignment.Left, -1, 11, Colors.Gray);
			DrawString(font, new Vector2(cel.Position.X, cel.End.Y + 13), hi.ToString("0.##"), HorizontalAlignment.Right, cel.Size.X, 11, Colors.Gray);
		}
	}
}
