using Godot;
using System;
using System.Collections.Generic;

// Lijngrafiek met assen en legenda.
// - Gedeelde schaal (0..max): y-as met waarden + rasterlijnen.
// - ElkEigenSchaal (elke lijn eigen min..max, voor trends): y-as alleen als er precies 1 lijn zichtbaar is;
//   anders staat per lijn het bereik in de legenda.
// - x-as = sim-tijd, via Tijdsduur (totale sim-seconden die de data beslaat).
// Lijnen in Verborgen worden overgeslagen.
public partial class Grafiek : Control
{
	public readonly List<(string naam, Color kleur, List<float> data)> Lijnen = new();
	public readonly HashSet<string> Verborgen = new();
	public bool ElkEigenSchaal;
	public Func<float> Tijdsduur;
	// vaste (theoretische) bereiken per lijn: lijnen op 0..1 van hun mogelijke bereik, onderling vergelijkbaar
	public Dictionary<string, (float min, float max)> VasteBereiken;
	public bool GebruikVast;
	public bool ToonLegenda = true; // uit op tabs met een checkbox-lijst (die toont naam + waarde al)
	public bool VerbergNulInLegenda; // lijnen die nu op 0 staan (uitgestorven groepen) niet in de legenda
	bool Vast => GebruikVast && VasteBereiken != null && VasteBereiken.Count > 0;

	const float Links = 52, Rechts = 10, Boven = 10, Onder = 22;
	const int YStappen = 4, XStappen = 5;

	Rect2 Plot => new(Links, Boven, Mathf.Max(1, Size.X - Links - Rechts), Mathf.Max(1, Size.Y - Boven - Onder));

	public override void _Ready() => HoverUitleg.Koppel(this, HoverTekst); // eigen hover-paneel (leesbaar, werkt ook tijdens de sim)

	public override void _Process(double delta) => QueueRedraw();

	// hover = waarde van elke zichtbare lijn op dat punt in de tijd
	// niet _GetTooltip: dan toont Godot óók zijn eigen tooltip (dubbel)
	public string HoverTekst(Vector2 p)
	{
		var plot = Plot;
		float fractie = Mathf.Clamp((p.X - plot.Position.X) / plot.Size.X, 0, 1);
		var regels = new List<string>();
		foreach (var (naam, _, data) in Lijnen)
		{
			if (Verborgen.Contains(naam) || data.Count == 0) continue;
			float v = data[Mathf.RoundToInt(fractie * (data.Count - 1))];
			string regel = $"{naam}: {Getal(v)}";
			if (Vast && VasteBereiken.TryGetValue(naam, out var b))
				regel += $"  ({(v - b.min) / (b.max - b.min) * 100:0}% van {Getal(b.min)} – {Getal(b.max)})";
			regels.Add(regel);
		}
		if (regels.Count == 0) return "";
		if (Tijdsduur != null) regels.Add($"op t = {Tijd(fractie * Tijdsduur())}");
		return string.Join("\n", regels);
	}

	(float min, float max) Bereik(string naam, List<float> data, float globaalMax)
	{
		if (Vast && VasteBereiken.TryGetValue(naam, out var b)) return b;
		if (!ElkEigenSchaal) return (0, globaalMax);
		float min = float.MaxValue, max = float.MinValue;
		foreach (var v in data) { min = Mathf.Min(min, v); max = Mathf.Max(max, v); }
		if (max - min < 0.0001f) max = min + 1;
		return (min, max);
	}

	public override void _Draw()
	{
		var font = ThemeDB.FallbackFont;
		var plot = Plot;
		DrawRect(new Rect2(Vector2.Zero, Size), new Color(0, 0, 0, 0.4f));

		var zichtbaar = Lijnen.FindAll(l => !Verborgen.Contains(l.naam) && l.data.Count >= 2);

		float globaalMax = 0.0001f;
		foreach (var l in zichtbaar) foreach (var v in l.data) globaalMax = Mathf.Max(globaalMax, v);

		// Vast: alle lijnen genormaliseerd naar 0..1 van hun theoretisch bereik, dan de y-as automatisch
		// passend gemaakt op de zichtbare data (lo..hi), zodat lijnen vergelijkbaar blijven maar niet in een dun streepje zitten.
		float lo = 0, hi = 1;
		if (Vast)
		{
			lo = float.MaxValue; hi = float.MinValue;
			foreach (var (naam, _, data) in zichtbaar)
			{
				var (bmin, bmax) = Bereik(naam, data, globaalMax);
				foreach (var v in data) { float n = (v - bmin) / (bmax - bmin); lo = Mathf.Min(lo, n); hi = Mathf.Max(hi, n); }
			}
			if (lo > hi) { lo = 0; hi = 1; }
			float rand = Mathf.Max(0.02f, (hi - lo) * 0.08f);
			lo = Mathf.Max(0, lo - rand); hi = Mathf.Min(1, hi + rand);
		}

		// y-as: gedeelde schaal, of 1 lijn (in zijn eenheden), of vaste bereiken met meerdere lijnen (in % van bereik)
		bool procentAs = Vast && zichtbaar.Count > 1;
		bool yAs = Vast || !ElkEigenSchaal || zichtbaar.Count == 1;
		var (asMin, asMax) = zichtbaar.Count == 1 ? Bereik(zichtbaar[0].naam, zichtbaar[0].data, globaalMax) : (0, globaalMax);
		for (int s = 0; s <= YStappen; s++)
		{
			float y = plot.End.Y - s / (float)YStappen * plot.Size.Y;
			DrawLine(new Vector2(plot.Position.X, y), new Vector2(plot.End.X, y), new Color(1, 1, 1, 0.08f));
			if (!yAs) continue;
			float p = Mathf.Lerp(lo, hi, s / (float)YStappen);
			string label = procentAs ? $"{p * 100:0}%"
				: Vast ? Getal(Mathf.Lerp(asMin, asMax, p))
				: Getal(Mathf.Lerp(asMin, asMax, s / (float)YStappen));
			DrawString(font, new Vector2(2, y + 4), label, HorizontalAlignment.Right, Links - 8, 11, Colors.Gray);
		}

		// x-as: sim-tijd
		if (Tijdsduur != null)
			for (int s = 0; s <= XStappen; s++)
			{
				float x = plot.Position.X + s / (float)XStappen * plot.Size.X;
				DrawLine(new Vector2(x, plot.Position.Y), new Vector2(x, plot.End.Y), new Color(1, 1, 1, 0.05f));
				DrawString(font, new Vector2(x - 30, Size.Y - 6), Tijd(s / (float)XStappen * Tijdsduur()), HorizontalAlignment.Center, 60, 11, Colors.Gray);
			}

		// lijnen
		var legenda = new List<(string tekst, Color kleur)>();
		foreach (var (naam, kleur, data) in zichtbaar)
		{
			var (min, max) = Bereik(naam, data, globaalMax);
			var punten = new Vector2[data.Count];
			for (int i = 0; i < data.Count; i++)
			{
				float t = (data[i] - min) / (max - min);
				if (Vast) t = (t - lo) / (hi - lo);
				punten[i] = new Vector2(plot.Position.X + i / (float)(data.Count - 1) * plot.Size.X, plot.End.Y - t * plot.Size.Y);
			}
			DrawPolyline(punten, kleur, 2, true);

			if (VerbergNulInLegenda && data[^1] == 0) continue;
			string tekst = $"{naam}: {Getal(data[^1])}";
			if (Vast && VasteBereiken.ContainsKey(naam))
				tekst += $"   ({(data[^1] - min) / (max - min) * 100:0}% van {Getal(min)} – {Getal(max)})";
			else if (ElkEigenSchaal && zichtbaar.Count > 1)
				tekst += $"   ({Getal(min)} – {Getal(max)})";
			legenda.Add((tekst, kleur));
		}

		// legenda met eigen achtergrond, zodat hij niet door de lijnen loopt
		if (ToonLegenda && legenda.Count > 0)
		{
			float breedte = 0;
			foreach (var (tekst, _) in legenda) breedte = Mathf.Max(breedte, font.GetStringSize(tekst, HorizontalAlignment.Left, -1, 13).X);
			DrawRect(new Rect2(plot.Position.X + 4, plot.Position.Y + 4, breedte + 12, legenda.Count * 16 + 6), new Color(0, 0, 0, 0.7f));
			for (int i = 0; i < legenda.Count; i++)
				DrawString(font, new Vector2(plot.Position.X + 10, plot.Position.Y + 20 + i * 16), legenda[i].tekst, HorizontalAlignment.Left, -1, 13, legenda[i].kleur);
		}
	}

	// voor de checkbox-labels: huidige waarde (+ % van theoretisch bereik als dat aan staat)
	public string Status(string naam, List<float> data)
	{
		if (data.Count == 0) return "";
		string s = Getal(data[^1]);
		if (Vast && VasteBereiken.TryGetValue(naam, out var b))
			s += $"  ({(data[^1] - b.min) / (b.max - b.min) * 100:0}%)";
		return s;
	}

	public static string Getal(float v) => Mathf.Abs(v) >= 100 ? v.ToString("0") : v.ToString("0.##");

	static string Tijd(float seconden)
	{
		int s = Mathf.RoundToInt(seconden);
		return s < 60 ? $"{s}s" : s < 3600 ? $"{s / 60}m{s % 60:00}" : $"{s / 3600}u{s / 60 % 60:00}";
	}
}
