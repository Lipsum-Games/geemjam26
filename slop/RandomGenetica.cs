using Godot;
using System.Linq;

// Slop: random wereld. Per trait wordt het DNA eerst gehusseld: elke cel wordt 1-2 geloote cellen met een random gewicht.
// De originele formule draait daarna op het gehusselde DNA, dus bereiken blijven heel maar welke genen wat doen verschilt per wereld.
public static class RandomGenetica
{
	public static void Pas(string seed)
	{
		_ = Karaktereigenschappen.Kachow; // zorg dat alle traits bestaan
		var rng = new System.Random(seed.Aggregate(17, (h, c) => h * 31 + c));
		foreach (var k in Karaktereigenschap.Alle)
		{
			var bron = new (int a, float wa, int b, float wb)[9];
			for (int cel = 0; cel < 9; cel++)
			{
				float Gewicht() => (rng.Next(2) == 0 ? -1 : 1) * (0.5f + (float)rng.NextDouble());
				bron[cel] = (rng.Next(9), Gewicht(), rng.NextDouble() < 0.4 ? rng.Next(9) : -1, Gewicht());
			}
			k.Remix = d =>
			{
				var nieuw = new Dna();
				for (int cel = 0; cel < 9; cel++)
				{
					var (a, wa, b, wb) = bron[cel];
					nieuw.M[cel / 3, cel % 3] = wa * d[a / 3, a % 3] + (b >= 0 ? wb * d[b / 3, b % 3] : 0);
				}
				return nieuw;
			};
			k.RemixTekst = "\nmet: " + string.Join(", ", bron.Select((x, cel) => $"d[{cel / 3},{cel % 3}]={x.wa:0.0}·d[{x.a / 3},{x.a % 3}]" + (x.b >= 0 ? $"{x.wb:+0.0;-0.0}·d[{x.b / 3},{x.b % 3}]" : "")));
		}
	}

	// plaatsnaam zoals de groepen (GroepNamen) + "-" + 4 letters, bv. "Oud-Gondoladrecht-QXZB"
	public static string NieuweSeed()
	{
		var r = new System.Random();
		string letters = new(Enumerable.Range(0, 4).Select(_ => (char)('A' + r.Next(26))).ToArray());
		return GroepNamen.Willekeurig(r) + "-" + letters;
	}
}
