using Godot;
using System.Collections.Generic;

// Slop: per trait de formule (zoals in de code) en een 3x3 matrix: hoeveel duwt elk gen deze trait gemiddeld
// omhoog (rood) of omlaag (blauw) per +1 in dat gen. Gemeten door het gen een beetje te verschuiven over 300 random DNA's.
public partial class GenUitleg : HBoxContainer
{
	ItemList _lijst;
	Label _naam, _formule, _uitleg;
	Matrix _matrix;

	public override void _Ready()
	{
		_lijst = new ItemList { CustomMinimumSize = new Vector2(240, 0), SizeFlagsVertical = SizeFlags.ExpandFill };
		foreach (var k in Karaktereigenschap.Alle) _lijst.AddItem(k.Naam);
		_lijst.ItemSelected += i => Toon(Karaktereigenschap.Alle[(int)i]);
		AddChild(_lijst);

		// rechts scrollt, anders valt de matrix onderaan van het scherm af
		var scroll = new ScrollContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, SizeFlagsVertical = SizeFlags.ExpandFill, HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled };
		var rechts = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
		_naam = new Label();
		_naam.AddThemeFontSizeOverride("font_size", 18);
		_formule = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
		_formule.AddThemeFontOverride("font", new SystemFont { FontNames = ["Menlo", "Consolas", "monospace"] });
		_formule.AddThemeFontSizeOverride("font_size", 15);
		_uitleg = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart };
		_uitleg.AddThemeFontSizeOverride("font_size", 13);
		_uitleg.AddThemeColorOverride("font_color", new Color(0.75f, 0.75f, 0.75f));
		_matrix = new Matrix { CustomMinimumSize = new Vector2(Matrix.Links + 3 * Matrix.Vak, Matrix.Boven + 3 * Matrix.Vak) };
		rechts.AddChild(_naam);
		rechts.AddChild(_formule);
		rechts.AddChild(_uitleg);
		rechts.AddChild(_matrix);
		scroll.AddChild(rechts);
		AddChild(scroll);

		if (Karaktereigenschap.Alle.Count > 0) { _lijst.Select(0); Toon(Karaktereigenschap.Alle[0]); }
	}

	void Toon(Karaktereigenschap k)
	{
		// effect per gen: (trait bij gen +stap - trait bij gen -stap) / (2 stap), gemiddeld over random DNA's
		const int N = 300;
		const float stap = 0.25f;
		var effect = new float[9];
		float lo = float.MaxValue, hi = float.MinValue;
		var rng = new RandomNumberGenerator { Seed = 1 }; // vaste seed: elke keer dezelfde getallen
		for (int s = 0; s < N; s++)
		{
			var d = new Dna();
			for (int g = 0; g < 9; g++) d.M[g / 3, g % 3] = rng.RandfRange(-1, 1);
			float v = k.Van(d);
			lo = Mathf.Min(lo, v); hi = Mathf.Max(hi, v);
			for (int g = 0; g < 9; g++)
			{
				float oud = d.M[g / 3, g % 3];
				d.M[g / 3, g % 3] = oud + stap; float plus = k.Van(d);
				d.M[g / 3, g % 3] = oud - stap; float min = k.Van(d);
				d.M[g / 3, g % 3] = oud;
				effect[g] += (plus - min) / (2 * stap) / N;
			}
		}
		_naam.Text = k.Naam;
		_formule.Text = Netjes(k.Formule) + k.RemixTekst;
		_uitleg.Text = $"Bij random DNA (genen -1..1) loopt hij van {lo:0.##} tot {hi:0.##}. " +
			"Matrix: per +1 in dat gen gaat de trait gemiddeld zoveel omhoog (rood) of omlaag (blauw). Grijs = doet niks.";
		_matrix.Effect = effect;
		_matrix.QueueRedraw();
	}

	// code -> leesbaarder: "d =>" weg, 0.8f -> 0.8, d[0, 0] -> d[0,0], * -> ·
	static string Netjes(string code)
	{
		string s = System.Text.RegularExpressions.Regex.Replace(code.Trim(), @"^\w+\s*=>\s*", "");
		s = System.Text.RegularExpressions.Regex.Replace(s, @"(\d+(\.\d+)?)[fF]\b", "$1");
		s = System.Text.RegularExpressions.Regex.Replace(s, @"\[\s*(\d)\s*,\s*(\d)\s*\]", "[$1,$2]");
		return s.Replace(" * ", " · ").Replace("\t", "  ");
	}

	partial class Matrix : Control
	{
		public float[] Effect = new float[9];
		public const float Vak = 60, Links = 50, Boven = 30; // vaste maten: de scroll regelt de rest

		public override void _Draw()
		{
			var font = ThemeDB.FallbackFont;
			float max = 1e-6f;
			foreach (var e in Effect) max = Mathf.Max(max, Mathf.Abs(e));
			const float vak = Vak;

			for (int r = 0; r < 3; r++)
			{
				DrawString(font, new Vector2(4, Boven + r * vak + vak / 2 + 5), $"rij {r}", HorizontalAlignment.Left, -1, 12, Colors.Gray);
				for (int c = 0; c < 3; c++)
				{
					if (r == 0) DrawString(font, new Vector2(Links + c * vak, Boven - 8), $"kolom {c}", HorizontalAlignment.Center, vak, 12, Colors.Gray);
					float e = Effect[r * 3 + c];
					float sterkte = Mathf.Abs(e) / max;
					var kleur = Mathf.Abs(e) < max * 0.02f ? new Color(0.35f, 0.35f, 0.35f)
						: (e > 0 ? new Color(0.3f, 0.3f, 0.3f).Lerp(new Color(0.9f, 0.2f, 0.2f), sterkte) : new Color(0.3f, 0.3f, 0.3f).Lerp(new Color(0.25f, 0.45f, 0.95f), sterkte));
					var rect = new Rect2(Links + c * vak, Boven + r * vak, vak - 4, vak - 4);
					DrawRect(rect, kleur);
					string tekst = Mathf.Abs(e) < max * 0.02f ? "0" : (e > 0 ? "+" : "") + e.ToString("0.###");
					DrawString(font, new Vector2(rect.Position.X, rect.Position.Y + rect.Size.Y / 2 + 6), tekst, HorizontalAlignment.Center, rect.Size.X, 15, Colors.White);
				}
			}
		}
	}
}
