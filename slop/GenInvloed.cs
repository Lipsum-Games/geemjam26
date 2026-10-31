using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// Welke traits hangt een gen aan? Automatisch bepaald, zonder iets bij te houden:
// we duwen dat ene gen een beetje omhoog en omlaag en kijken welke traits meebewegen (gevoeligheid).
// Werkt voor elke formule, ook nieuwe traits, omdat we gedrag meten in plaats van formules te lezen.
// - "In het algemeen": gemiddeld over 300 random DNA's (de bedrading), één keer berekend.
// - "Bij deze groep nu": op het gemiddelde DNA van de groep. Kan veel kleiner zijn als een sigmoid verzadigd is
//   (dan maakt het gen op dit moment weinig uit, en kan het vrij wegdriften).
// Effect = verandering van de trait per +1 in het gen, als deel van het theoretisch bereik van die trait.
public static class GenInvloed
{
	const float Stap = 0.25f;
	const float Drempel = 0.02f; // kleiner = negeren (ruis / doet niks)
	static Dictionary<int, List<(string naam, float effect)>> _algemeen;
	static bool _bereikBekend;

	static bool BereikBekend() =>
		((Engine.GetMainLoop() as SceneTree)?.CurrentScene?.GetNodeOrNull<Statistiek>("Statistiek")?.TheoretischBereik.Count ?? 0) > 0;

	public static string Tekst(int cel, float[] groepDna)
	{
		// pas cachen als de theoretische bereiken er zijn (eerste meting), anders schaal later opnieuw
		if (_algemeen == null || !_bereikBekend) { _bereikBekend = BereikBekend(); _algemeen = BerekenAlgemeen(); }

		var algemeen = (_algemeen.GetValueOrDefault(cel) ?? new()).ToDictionary(e => e.naam, e => e.effect);
		var lokaal = Effecten(cel, groepDna).ToDictionary(e => e.naam, e => e.effect);
		var traits = algemeen.Keys.Union(lokaal.Keys)
			.OrderByDescending(n => Mathf.Abs(algemeen.GetValueOrDefault(n)))
			.ThenByDescending(n => Mathf.Abs(lokaal.GetValueOrDefault(n)))
			.ToList();

		int breed = Math.Max(5, traits.Count == 0 ? 0 : traits.Max(n => n.Length)) + 2;
		var regels = new List<string>
		{
			$"Gen ({cel / 3},{cel % 3}) = {groepDna[cel]:0.00}  (gemiddeld in deze groep)",
			"",
			"trait".PadRight(breed) + "algemeen  bij deze groep nu",
		};
		if (traits.Count == 0) regels.Add("(dit gen beïnvloedt geen enkele trait)");
		foreach (var naam in traits)
		{
			string a = algemeen.TryGetValue(naam, out var ea) ? Pijlen(ea) : "–";
			string l = lokaal.TryGetValue(naam, out var el) ? Pijlen(el) : "–   (geen effect meer)";
			regels.Add(naam.PadRight(breed) + a.PadRight(10) + l);
		}
		regels.Add("");
		regels.Add("↑ gen omhoog = trait hoger · ↓ = lager · meer pijlen = sterker");
		regels.Add("'nu' = op het gemiddelde DNA van deze groep; verzadigd = gen kan vrij wegdriften");
		return string.Join("\n", regels);
	}

	static string Pijlen(float effect)
	{
		int n = Mathf.Abs(effect) > 0.3f ? 3 : Mathf.Abs(effect) > 0.1f ? 2 : 1;
		return new string(effect > 0 ? '↑' : '↓', n);
	}

	// effect per trait van dit gen rond een gegeven DNA, gesorteerd op sterkte
	static List<(string naam, float effect)> Effecten(int cel, float[] basis)
	{
		var resultaat = new List<(string, float)>();
		foreach (var k in Karaktereigenschap.Alle)
		{
			float hoog = k.Van(Maak(basis, cel, +Stap));
			float laag = k.Van(Maak(basis, cel, -Stap));
			float effect = (hoog - laag) / (2 * Stap) / Bereik(k);
			if (Mathf.Abs(effect) >= Drempel) resultaat.Add((k.Naam, effect));
		}
		return resultaat.OrderByDescending(e => Mathf.Abs(e.Item2)).ToList();
	}

	static Dictionary<int, List<(string naam, float effect)>> BerekenAlgemeen()
	{
		var rng = new Random(7);
		var som = new Dictionary<(int cel, string naam), float>();
		const int aantal = 300;
		for (int s = 0; s < aantal; s++)
		{
			var basis = new float[9];
			for (int i = 0; i < 9; i++) basis[i] = (float)(rng.NextDouble() * 2 - 1);
			for (int cel = 0; cel < 9; cel++)
				foreach (var k in Karaktereigenschap.Alle)
				{
					float effect = (k.Van(Maak(basis, cel, +Stap)) - k.Van(Maak(basis, cel, -Stap))) / (2 * Stap) / Bereik(k);
					som[(cel, k.Naam)] = som.GetValueOrDefault((cel, k.Naam)) + effect / aantal;
				}
		}
		return som.Where(kv => Mathf.Abs(kv.Value) >= Drempel)
			.GroupBy(kv => kv.Key.cel)
			.ToDictionary(g => g.Key, g => g.Select(kv => (kv.Key.naam, kv.Value)).OrderByDescending(e => Mathf.Abs(e.Value)).ToList());
	}

	// theoretisch bereik van een trait (uit Statistiek), zodat effecten van verschillende traits vergelijkbaar zijn
	static float Bereik(Karaktereigenschap k)
	{
		var stats = (Engine.GetMainLoop() as SceneTree)?.CurrentScene?.GetNodeOrNull<Statistiek>("Statistiek");
		if (stats != null && stats.TheoretischBereik.TryGetValue(k.Naam, out var b)) return Mathf.Max(1e-4f, b.max - b.min);
		return 1f;
	}

	static Dna Maak(float[] basis, int cel, float duw)
	{
		var d = new Dna();
		for (int i = 0; i < 9; i++) d.M[i / 3, i % 3] = basis[i] + (i == cel ? duw : 0);
		return d;
	}
}
