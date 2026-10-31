using Godot;

// Lijst van gevonden groepen: kleur, banden (zoals ze er in de wereld uitzien), grootte,
// opvallende kenmerken en gemiddeld DNA als 3x3 heatmap. Klik = markeren in de wereld.
public partial class ClusterLijst : Control
{
	public Clusters Bron;
	const float RijHoogte = 76;
	const float CelB = 34, CelH = 21; // heatmap-vakjes, groot genoeg voor een getal met 1 decimaal

	public override void _Ready() => TooltipText = " "; // niet-leeg, anders vraagt Godot _GetTooltip nooit op

	public override void _Process(double delta)
	{
		if (Bron != null) CustomMinimumSize = new Vector2(0, Bron.Groepen.Count * RijHoogte + 30);
		QueueRedraw();
	}

	// hover-uitleg per onderdeel van een rij (zelfde maten als in _Draw)
	public override string _GetTooltip(Vector2 p)
	{
		if (Bron == null) return "";
		if (p.Y < 30)
			return Bron.Wijze switch
			{
				Clusters.Modus.Plek =>
					"Kuddes: joekels die bij elkaar in de buurt lopen (binnen ~120 px, min. 4 samen).\n" +
					"Het aantal kuddes volgt vanzelf. Losse joekels tellen als zwervers.\n" +
					"Per kudde zie je waarin hij genetisch afwijkt van de rest (σ-kenmerken).",
				Clusters.Modus.Geur =>
					"Geurgroepen: joekels met (bijna) dezelfde geurwaarde = dezelfde mistkleur.\n" +
					"Een groep loopt door zolang er geen gat in de geurwaardes zit,\n" +
					"dus blauw en groen worden apart, ook als ze door elkaar lopen.",
				_ =>
					"Soorten op basis van alle 9 genen (k-means). Een extra groep komt er pas als die\n" +
					"de genetische spreiding minstens 30% verlaagt. Verschillen in maar 1-3 genen\n" +
					"(zoals alleen geur) vallen hier weg; daarvoor zijn 'Plek' en 'Geur'.",
			} + "\nElke 2 sim-seconden opnieuw berekend.";

		int i = (int)((p.Y - 30) / RijHoogte);
		if (i < 0 || i >= Bron.Groepen.Count) return "";
		var g = Bron.Groepen[i];
		float y = p.Y - (30 + i * RijHoogte);

		float x0 = Size.X - 3 * CelB - 8;
		if (p.X >= x0 && y >= 8 && y < 8 + 3 * CelH)
		{
			int c = (int)((p.X - x0) / CelB), r = (int)((y - 8) / CelH);
			if (c is >= 0 and < 3 && r is >= 0 and < 3)
				return $"Gemiddeld DNA van deze groep, gen ({r},{c}): {g.Centrum[r * 3 + c]:0.00}\n" +
					"Rood = positief, blauw = negatief, wit ≈ 0. Zelfde indeling als de DNA-matrix.";
		}
		if (p.X < 8)
			return "Groepskleur: dezelfde kleur in de grafiek hieronder en in de tab 'DNA genen'.";
		if (p.X < 58)
			return "Zo ziet een gemiddeld lid eruit: de 3 kleurbanden van de joekel,\nééntje per DNA-rij (rij = rood, groen, blauw).";
		if (y < 30)
			return "Grootte van de groep en aandeel van alle joekels.\n" +
				"Klik = markeer in de wereld (de rest wordt doorzichtig), nogmaals klikken = uit.\n" +
				"Blijft aan als je het stats-venster sluit.";
		return "Meest voorkomende stuitertype in deze groep, plus max 3 traits waarin de groep\n" +
			"afwijkt van de héle populatie, in σ (standaarddeviaties).\n" +
			"+1σ = duidelijk hoger dan gemiddeld, −1σ = duidelijk lager. Onder 0,5σ wordt niet getoond.\n" +
			"Met één groep is er niets om van af te wijken, dan staat alleen het type er.";
	}

	public override void _Draw()
	{
		var font = ThemeDB.FallbackFont;
		string kop = "Klik een groep om hem in de wereld te markeren (nogmaals = uit)";
		if (Bron != null && Bron.Zwervers > 0) kop += $"   ·   {Bron.Zwervers} zwervers (in geen groep)";
		if (Bron != null && Bron.Herberekent) kop += "   ·   geschiedenis wordt herberekend…";
		DrawString(font, new Vector2(6, 18), kop, HorizontalAlignment.Left, -1, 13, Colors.Gray);
		Info(6 + font.GetStringSize(kop, HorizontalAlignment.Left, -1, 13).X + 12, 14);
		if (Bron == null) return;

		int totaal = 0;
		foreach (var g in Bron.Groepen) totaal += g.Aantal;

		for (int i = 0; i < Bron.Groepen.Count; i++)
		{
			var g = Bron.Groepen[i];
			float y = 30 + i * RijHoogte;

			if (Bron.Gemarkeerd == g.Id)
				DrawRect(new Rect2(0, y, Size.X, RijHoogte - 4), new Color(g.Kleur, 0.25f));
			DrawRect(new Rect2(0, y, 6, RijHoogte - 4), g.Kleur);

			for (int b = 0; b < 3; b++)
				DrawRect(new Rect2(14, y + 8 + b * 18, 40, 16), g.Banden[b]);

			float pct = totaal > 0 ? 100f * g.Aantal / totaal : 0;
			string titel = $"Groep {g.Id}  —  {g.Aantal} joekels ({pct:0}%)";
			DrawString(font, new Vector2(64, y + 22), titel, HorizontalAlignment.Left, -1, 15, g.Kleur);
			Info(64 + font.GetStringSize(titel, HorizontalAlignment.Left, -1, 15).X + 12, y + 17);

			Info(71, y + 40);
			DrawString(font, new Vector2(84, y + 44), g.Kenmerken, HorizontalAlignment.Left, Size.X - 84 - 3 * CelB - 20, 12, Colors.White);

			float x0 = Size.X - 3 * CelB - 8;
			for (int r = 0; r < 3; r++)
				for (int c = 0; c < 3; c++)
				{
					float v = g.Centrum[r * 3 + c];
					var kleur = v >= 0 ? new Color(1, 1, 1).Lerp(Colors.Red, v) : new Color(1, 1, 1).Lerp(Colors.RoyalBlue, -v);
					var vak = new Rect2(x0 + c * CelB, y + 8 + r * CelH, CelB - 1, CelH - 1);
					DrawRect(vak, kleur);
					DrawString(font, new Vector2(vak.Position.X, vak.Position.Y + 15), v.ToString("0.0"), HorizontalAlignment.Center, vak.Size.X, 11, new Color(0.1f, 0.1f, 0.1f));
				}
		}
	}

	// klein ⓘ-rondje: "hier zit hover-uitleg"
	void Info(float x, float yMidden)
	{
		DrawCircle(new Vector2(x, yMidden), 7, new Color(1, 1, 1, 0.25f));
		DrawString(ThemeDB.FallbackFont, new Vector2(x - 7, yMidden + 4), "i", HorizontalAlignment.Center, 14, 11, Colors.White);
	}

	public override void _GuiInput(InputEvent e)
	{
		if (Bron == null || e is not InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true } klik) return;
		int i = (int)((klik.Position.Y - 30) / RijHoogte);
		if (i < 0 || i >= Bron.Groepen.Count) return;
		int id = Bron.Groepen[i].Id;
		Bron.Markeer(Bron.Gemarkeerd == id ? null : id);
		AcceptEvent();
	}
}
