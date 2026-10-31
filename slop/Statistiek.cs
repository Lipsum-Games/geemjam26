using Godot;
using System.Collections.Generic;

// Meet elke (sim)seconde de populatie, gemiddelde traits en stuitertype-verdeling.
// Bij MaxPunten wordt elk tweede punt weggegooid en halveert de meetfrequentie,
// zo blijft de hele geschiedenis bewaard (verder terug = grover).
public partial class Statistiek : Node
{
	[Export] public int MaxPunten = 600;
	float _interval = 1f;
	float _timer;
	public float Interval => _interval; // sim-seconden per meetpunt (verdubbelt bij halveren)

	public readonly List<float> Populatie = new();
	public readonly Dictionary<string, List<float>> Traits = new();
	public readonly Dictionary<string, List<float>> Types = new();

	// Theoretisch min/max per trait over alle mogelijke DNA (genen -1..1).
	// Traits zijn sommen/sigmoids/ranges van genen, die hun uitersten op de hoeken van de genruimte hebben:
	// alle 2^9 = 512 combinaties van -1/+1, plus 2000 random DNA als vangnet voor rare formules.
	public readonly Dictionary<string, (float min, float max)> TheoretischBereik = new();

	void BerekenBereiken()
	{
		var rng = new RandomNumberGenerator();
		foreach (var k in Karaktereigenschap.Alle)
		{
			float min = float.MaxValue, max = float.MinValue;
			void Probeer(Dna d) { float v = k.Van(d); min = Mathf.Min(min, v); max = Mathf.Max(max, v); }

			for (int hoek = 0; hoek < 512; hoek++)
			{
				var d = new Dna();
				for (int i = 0; i < 9; i++) d.M[i / 3, i % 3] = ((hoek >> i) & 1) == 1 ? 1 : -1;
				Probeer(d);
			}
			for (int s = 0; s < 2000; s++)
			{
				var d = new Dna();
				for (int i = 0; i < 9; i++) d.M[i / 3, i % 3] = rng.RandfRange(-1, 1);
				Probeer(d);
			}
			if (max - min < 1e-6f) max = min + 1;
			TheoretischBereik[k.Naam] = (min, max);
		}
	}

	public override void _Process(double delta)
	{
		_timer -= (float)delta;
		if (_timer > 0) return;
		_timer = _interval;
		Meet();
	}

	void Meet()
	{
		var joekels = new List<Joekel>();
		foreach (var n in GetParent().GetChildren())
			if (n is Joekel j && j.Dna != null) joekels.Add(j);

		Populatie.Add(joekels.Count);

		if (TheoretischBereik.Count < Karaktereigenschap.Alle.Count) BerekenBereiken();
		foreach (var k in Karaktereigenschap.Alle)
		{
			float som = 0;
			foreach (var j in joekels) som += k.Van(j.Dna);
			Reeks(Traits, k.Naam).Add(joekels.Count > 0 ? som / joekels.Count : 0);
		}

		var telling = new Dictionary<string, int>();
		foreach (var j in joekels)
			telling[j.StuiterNaam] = telling.GetValueOrDefault(j.StuiterNaam) + 1;
		foreach (var naam in new[] { "Garfield", "Fumo", "Kei", "Gondola" })
			Reeks(Types, naam).Add(joekels.Count > 0 ? telling.GetValueOrDefault(naam) / (float)joekels.Count : 0);

		if (Populatie.Count >= MaxPunten) Halveer();
	}

	static List<float> Reeks(Dictionary<string, List<float>> d, string naam)
	{
		if (!d.TryGetValue(naam, out var lijst)) d[naam] = lijst = new List<float>();
		return lijst;
	}

	void Halveer()
	{
		Dun(Populatie);
		foreach (var l in Traits.Values) Dun(l);
		foreach (var l in Types.Values) Dun(l);
		_interval *= 2;
	}

	static void Dun(List<float> l)
	{
		for (int i = l.Count - 1; i >= 0; i -= 2) l.RemoveAt(i);
	}
}
