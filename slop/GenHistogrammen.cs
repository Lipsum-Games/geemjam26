using Godot;
using System.Collections.Generic;

// 3x3 raster van histogrammen, één per DNA-cel. Staven gestapeld per groep (groepskleur),
// zodat je ziet op welke genen de groepen verschillen (twee bulten = splitsend gen).
public partial class GenHistogrammen : Control
{
	public Clusters Bron;
	const int Bakjes = 20;

	public override void _Ready() => TooltipText = " "; // niet-leeg, anders vraagt Godot _GetTooltip nooit op

	public override void _Process(double delta) => QueueRedraw();

	public override string _GetTooltip(Vector2 p)
	{
		int c = Mathf.Clamp((int)(p.X / (Size.X / 3)), 0, 2), r = Mathf.Clamp((int)(p.Y / (Size.Y / 3)), 0, 2);
		return $"Gen ({r},{c}): hoe deze genwaarde verdeeld is over alle joekels (-1 links, +1 rechts).\n" +
			"Hoogte = aantal joekels met die waarde. Kleuren = groepen (zie tab 'Groepen').\n" +
			"Eén bult = iedereen lijkt op elkaar. Twee bulten = dit gen splitst de populatie\n" +
			"(vaak precies waar twee groepen verschillen).";
	}

	public override void _Draw()
	{
		DrawRect(new Rect2(Vector2.Zero, Size), new Color(0, 0, 0, 0.4f));
		if (Bron == null) return;

		var font = ThemeDB.FallbackFont;
		float celB = Size.X / 3, celH = Size.Y / 3;

		for (int r = 0; r < 3; r++)
		for (int c = 0; c < 3; c++)
		{
			var tellingen = new Dictionary<int, int[]>();
			var totaal = new int[Bakjes];
			foreach (var (joekel, groep) in Bron.Indeling)
			{
				if (!IsInstanceValid(joekel)) continue;
				int bakje = Mathf.Clamp((int)((joekel.Dna[r, c] + 1) / 2 * Bakjes), 0, Bakjes - 1);
				if (!tellingen.TryGetValue(groep, out var t)) tellingen[groep] = t = new int[Bakjes];
				t[bakje]++;
				totaal[bakje]++;
			}
			int max = 1;
			foreach (var t in totaal) max = Mathf.Max(max, t);

			var cel = new Rect2(c * celB + 6, r * celH + 20, celB - 12, celH - 34);
			DrawRect(cel, new Color(1, 1, 1, 0.05f));
			float staafB = cel.Size.X / Bakjes;

			for (int b = 0; b < Bakjes; b++)
			{
				float y = cel.End.Y;
				foreach (var g in Bron.Groepen)
				{
					if (!tellingen.TryGetValue(g.Id, out var t) || t[b] == 0) continue;
					float h = t[b] / (float)max * cel.Size.Y;
					y -= h;
					DrawRect(new Rect2(cel.Position.X + b * staafB, y, staafB - 1, h), g.Kleur);
				}
			}

			DrawString(font, new Vector2(cel.Position.X, cel.Position.Y - 4), $"gen ({r},{c})", HorizontalAlignment.Left, -1, 13, Colors.White);
			DrawString(font, new Vector2(cel.Position.X, cel.End.Y + 13), "-1", HorizontalAlignment.Left, -1, 11, Colors.Gray);
			DrawString(font, new Vector2(cel.Position.X + cel.Size.X / 2 - 3, cel.End.Y + 13), "0", HorizontalAlignment.Left, -1, 11, Colors.Gray);
			DrawString(font, new Vector2(cel.End.X - 8, cel.End.Y + 13), "1", HorizontalAlignment.Left, -1, 11, Colors.Gray);
		}
	}
}
