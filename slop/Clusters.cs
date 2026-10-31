using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// Vindt groepen joekels, op drie manieren (Modus):
// - Plek (kuddes, standaard): DBSCAN op positie. Joekels dicht bij elkaar (zie Losheid), met minstens
//   KuddeMinimum buren, vormen samen een kudde; het aantal kuddes volgt vanzelf. Losse joekels = zwervers.
// - Geur: DBSCAN op de geurwaarde (= mistkleur). Groepen = geurwaardes met een gat ertussen,
//   dus blauwe-mist en groene-mist joekels worden aparte groepen, ook als ze door elkaar lopen.
// - DNA (soorten): k-means op alle 9 genen, k automatisch (elleboog). Ziet alleen soorten die in véél genen
//   verschillen; een verschil in 1-3 genen (zoals geur) valt weg in de ruis van de rest.
// Een groep telt pas vanaf MinGroep joekels (of MinGroepAandeel van de populatie); kleiner = zwervers.
// Alle drie de modi worden elke meting berekend en hebben elk hun eigen groepen, historie en Losheid.
// Elke meting wordt ook een momentopname (positie, geur, DNA per joekel) bewaard, zodat na het verschuiven
// van de slider de héle geschiedenis opnieuw ingedeeld kan worden met de nieuwe instelling. Dat gebeurt op
// een achtergrond-thread (geen hapering); de grafiek wisselt als het klaar is.
// Groepen houden hun Id/kleur over metingen heen (koppelen aan dichtstbijzijnde vorige groep).
// Kan een groep markeren in de wereld (rest wordt gedimd), voor de modus die nu getoond wordt.
public partial class Clusters : Node
{
	public enum Modus { Plek, Geur, Dna }
	[Export] public Modus Wijze = Modus.Plek;   // welke modus het stats-scherm toont / markeert
	[Export] public int MinGroep = 5;           // minimum joekels om als groep te tellen...
	[Export] public float MinGroepAandeel = 0.02f; // ...of dit deel van de populatie, als dat meer is
	[Export] public int KuddeMinimum = 4;       // min. joekels (incl. jezelf) in je buurt om een kudde te beginnen
	[Export] public float KoppelPlek = 400f;    // px: kudde-centrum mag zoveel verschuiven en toch "dezelfde" blijven
	[Export] public int GeurMinimum = 3;
	[Export] public float KoppelGeur = 0.15f;   // gemiddelde geur mag zoveel verschuiven en toch "dezelfde" groep blijven
	[Export] public float KoppelAfstand = 1.0f; // max afstand (in genen) om een DNA-groep als "dezelfde" te zien
	[Export] public float Interval = 2f;
	[Export] public int MaxK = 6;
	[Export] public int MaxPunten = 600;

	static readonly Color[] Kleuren = { Colors.Orange, Colors.SkyBlue, Colors.LightGreen, Colors.HotPink, Colors.Yellow, Colors.Violet, Colors.Coral, Colors.Aquamarine, Colors.Khaki, Colors.White };

	public class Groep
	{
		public int Id;
		public Color Kleur;
		public float[] Centrum;
		public Vector2 Plek; // sleutel om groepen over tijd te koppelen: gem. positie (Plek) of (gem. geur, 0) (Geur)
		public int Aantal;
		public Color[] Banden = new Color[3];
		public string Kenmerken = "";
	}

	// één meetmoment: genoeg om de indeling later opnieuw te doen (onveranderlijk, dus veilig voor threads)
	class Momentopname
	{
		public Vector2[] Pos;
		public float[] Geur;
		public float[][] Dna;
	}

	class Herberekening
	{
		public Dictionary<int, List<float>> Historie;
		public List<Groep> Groepen;
		public int VolgendeId, Momenten, Halveringen;
	}

	// alles wat per modus bijgehouden wordt
	class Staat
	{
		public readonly List<Groep> Groepen = new();
		public readonly Dictionary<int, List<float>> GrootteHistorie = new();
		public readonly Dictionary<Joekel, int> Indeling = new();
		public int? Gemarkeerd;
		public int Zwervers;
		public int Metingen, VolgendeId;
		public int HistorieVersie;          // telt op als de historie vervangen is (grafiek moet lijnen opnieuw ophalen)
		public Task<Herberekening> Bezig;   // achtergrond-herberekening na slider
		// Losheid 0..1 (slider): hoger = fijner. Plek: buurafstand 200..40 px. Geur: geurgat 0.08..0.02.
		// DNA: extra groep mag als spreiding < 78%..92% van ervoor (random DNA geeft ~88%, dus helemaal rechts splitst ruis).
		public float Losheid = 0.5f;
	}

	readonly Dictionary<Modus, Staat> _staten = new() { [Modus.Plek] = new(), [Modus.Geur] = new(), [Modus.Dna] = new() };
	readonly List<Momentopname> _momenten = new();
	int _halveringen;
	Staat S => _staten[Wijze];

	// publieke kant toont altijd de huidige modus
	public List<Groep> Groepen => S.Groepen;
	public Dictionary<int, List<float>> GrootteHistorie => S.GrootteHistorie;
	public Dictionary<Joekel, int> Indeling => S.Indeling;
	public int? Gemarkeerd => S.Gemarkeerd;
	public int Zwervers => S.Zwervers;
	public int HistorieVersie => S.HistorieVersie;
	public bool Herberekent => S.Bezig != null;
	public float Losheid { get => S.Losheid; set => S.Losheid = value; }

	readonly Random _random = new();
	float _timer;

	public static Color KleurVan(int id) => Kleuren[id % Kleuren.Length];

	public override void _Process(double delta)
	{
		VerwerkHerberekeningen();

		_timer -= (float)delta;
		if (_timer > 0) return;
		_timer = Interval;
		Werk();
	}

	// bewaar = true: alle modi indelen + een punt in elke historie (+ momentopname).
	// false: alleen de huidige modus opnieuw indelen (bv. direct na de slider), zonder historie-punt.
	public void Werk(bool bewaar = true)
	{
		var (joekels, moment) = Meet();
		if (bewaar) _momenten.Add(moment);

		foreach (var (modus, staat) in _staten)
		{
			if (!bewaar && modus != Wijze) continue;
			DeelLive(modus, staat, joekels, moment);
			if (bewaar) Bewaar(staat.GrootteHistorie, ref staat.Metingen, staat.Groepen);
		}

		if (bewaar && _momenten.Count >= MaxPunten)
		{
			foreach (var staat in _staten.Values)
			{
				foreach (var lijst in staat.GrootteHistorie.Values) Dun(lijst);
				staat.Metingen = staat.GrootteHistorie.Values.FirstOrDefault()?.Count ?? 0;
			}
			Dun(_momenten);
			_halveringen++;
			Interval *= 2;
		}
		PasMarkeringToe();
	}

	(List<Joekel>, Momentopname) Meet()
	{
		var joekels = new List<Joekel>();
		foreach (var n in GetParent().GetChildren())
			if (n is Joekel j && j.Dna != null) joekels.Add(j);
		var moment = new Momentopname
		{
			Pos = joekels.Select(j => j.GlobalPosition).ToArray(),
			Geur = joekels.Select(j => j.Geur).ToArray(),
			Dna = joekels.Select(Genen).ToArray(),
		};
		return (joekels, moment);
	}

	// indeling van de levende joekels: groepen + per-joekel indeling + beschrijving (kenmerken, banden)
	void DeelLive(Modus modus, Staat staat, List<Joekel> joekels, Momentopname moment)
	{
		var (labels, k) = Indelen(modus, staat.Losheid, moment, staat.Groepen, _random);
		var groepen = Groepeer(modus, moment, labels, k, staat.Groepen, ref staat.VolgendeId);

		staat.Indeling.Clear();
		staat.Zwervers = 0;
		for (int i = 0; i < joekels.Count; i++)
		{
			if (labels[i] >= 0) staat.Indeling[joekels[i]] = groepen[labels[i]].Id;
			else staat.Zwervers++;
		}

		BeschrijfGroepen(groepen, joekels, labels);
		groepen.Sort((a, b) => b.Aantal.CompareTo(a.Aantal)); // grootste bovenaan
		staat.Groepen.Clear();
		staat.Groepen.AddRange(groepen);
		if (staat.Gemarkeerd is int m && !staat.Groepen.Exists(g => g.Id == m)) staat.Gemarkeerd = null;
	}

	// ---- slider: héle geschiedenis opnieuw indelen, op een achtergrond-thread ----

	public void HerberekenHistorie()
	{
		var staat = S;
		var modus = Wijze;
		float losheid = staat.Losheid;
		var momenten = _momenten.ToArray();
		int halveringen = _halveringen;
		staat.Bezig = Task.Run(() =>
		{
			var r = Herbereken(modus, losheid, momenten, new Random(12345));
			r.Halveringen = halveringen;
			return r;
		});
	}

	Herberekening Herbereken(Modus modus, float losheid, IList<Momentopname> momenten, Random rng)
	{
		var groepen = new List<Groep>();
		var historie = new Dictionary<int, List<float>>();
		int volgendeId = 0, metingen = 0;
		foreach (var moment in momenten)
		{
			var (labels, k) = Indelen(modus, losheid, moment, groepen, rng);
			groepen = Groepeer(modus, moment, labels, k, groepen, ref volgendeId);
			Bewaar(historie, ref metingen, groepen);
		}
		return new Herberekening { Historie = historie, Groepen = groepen, VolgendeId = volgendeId, Momenten = momenten.Count };
	}

	// ook aangeroepen vanuit het stats-scherm (dat draait tijdens pauze door), zodat het ook gepauzeerd werkt
	public void VerwerkHerberekeningen()
	{
		foreach (var (modus, staat) in _staten)
		{
			if (staat.Bezig is not { IsCompleted: true } taak) continue;
			staat.Bezig = null;
			if (taak.IsFaulted) { GD.PushError($"Clusters: herberekenen mislukt: {taak.Exception}"); continue; }

			var r = taak.Result;
			if (r.Halveringen != _halveringen) // tussendoor uitgedund: indexen kloppen niet meer, opnieuw
			{
				var huidige = Wijze;
				Wijze = modus;
				HerberekenHistorie();
				Wijze = huidige;
				continue;
			}

			// metingen die binnenkwamen terwijl de thread rekende: hier nog even bijwerken
			var groepen = r.Groepen;
			int volgendeId = r.VolgendeId, metingen = r.Momenten;
			for (int i = r.Momenten; i < _momenten.Count; i++)
			{
				var (labels, k) = Indelen(modus, staat.Losheid, _momenten[i], groepen, _random);
				groepen = Groepeer(modus, _momenten[i], labels, k, groepen, ref volgendeId);
				Bewaar(r.Historie, ref metingen, groepen);
			}

			staat.GrootteHistorie.Clear();
			foreach (var (id, lijst) in r.Historie) staat.GrootteHistorie[id] = lijst;
			staat.Metingen = metingen;
			staat.VolgendeId = volgendeId;
			staat.Groepen.Clear();
			staat.Groepen.AddRange(groepen);
			staat.HistorieVersie++;

			var (joekels, moment) = Meet();
			DeelLive(modus, staat, joekels, moment);
			PasMarkeringToe();
		}
	}

	// ---- de eigenlijke indeling (puur rekenwerk op een momentopname, ook bruikbaar vanaf een thread) ----

	// labels per joekel (-1 = zwerver) en aantal groepen; te kleine groepen worden zwervers
	(int[] labels, int k) Indelen(Modus modus, float losheid, Momentopname moment, List<Groep> vorige, Random rng)
	{
		int n = moment.Pos.Length;
		int[] labels;
		int k;
		if (modus == Modus.Dna)
		{
			labels = KiesEnCluster(moment.Dna, losheid, vorige, rng);
			k = labels.Length == 0 ? 0 : labels.Max() + 1;
		}
		else if (modus == Modus.Geur)
			(labels, k) = Dbscan(moment.Geur.Select(g => new Vector2(g, 0)).ToArray(), Mathf.Lerp(0.08f, 0.02f, losheid), GeurMinimum);
		else
			(labels, k) = Dbscan(moment.Pos, Mathf.Lerp(200f, 40f, losheid), KuddeMinimum);

		int minimum = Math.Max(MinGroep, (int)(MinGroepAandeel * n));
		var telling = new int[k];
		foreach (var l in labels) if (l >= 0) telling[l]++;
		var nieuwNummer = new int[k];
		int over = 0;
		for (int c = 0; c < k; c++) nieuwNummer[c] = telling[c] >= minimum ? over++ : -1;
		for (int i = 0; i < n; i++) if (labels[i] >= 0) labels[i] = nieuwNummer[labels[i]];
		return (labels, over);
	}

	// maakt groepen (centrum, sleutel, aantal) en koppelt ze aan de vorige; volgorde = labelnummer
	List<Groep> Groepeer(Modus modus, Momentopname moment, int[] labels, int k, List<Groep> vorige, ref int volgendeId)
	{
		var centra = new float[k][];
		var sleutels = new Vector2[k];
		var aantal = new int[k];
		for (int c = 0; c < k; c++) centra[c] = new float[9];
		for (int i = 0; i < labels.Length; i++)
		{
			int c = labels[i];
			if (c < 0) continue;
			aantal[c]++;
			sleutels[c] += modus == Modus.Geur ? new Vector2(moment.Geur[i], 0) : moment.Pos[i];
			for (int d = 0; d < 9; d++) centra[c][d] += moment.Dna[i][d];
		}
		for (int c = 0; c < k; c++)
		{
			sleutels[c] /= Math.Max(1, aantal[c]);
			for (int d = 0; d < 9; d++) centra[c][d] /= Math.Max(1, aantal[c]);
		}

		var groepen = Koppel(modus, vorige, centra, sleutels, ref volgendeId);
		for (int c = 0; c < k; c++) groepen[c].Aantal = aantal[c];
		return groepen;
	}

	// geeft nieuwe groepen het Id/kleur van de dichtstbijzijnde vorige groep (gretig, dichtste paar eerst).
	// Plek/Geur koppelen op hun sleutel, DNA op gemiddeld DNA.
	List<Groep> Koppel(Modus modus, List<Groep> vorige, float[][] centra, Vector2[] sleutels, ref int volgendeId)
	{
		float max = modus switch { Modus.Plek => KoppelPlek, Modus.Geur => KoppelGeur, _ => KoppelAfstand };
		float maxD2 = max * max;
		var paren = new List<(float d, int nieuw, Groep oud)>();
		for (int i = 0; i < centra.Length; i++)
			foreach (var oud in vorige)
				paren.Add((modus == Modus.Dna ? Afstand2(centra[i], oud.Centrum) : sleutels[i].DistanceSquaredTo(oud.Plek), i, oud));
		paren.Sort((a, b) => a.d.CompareTo(b.d));

		var nieuwe = new Groep[centra.Length];
		var gebruikt = new HashSet<Groep>();
		foreach (var (d, i, oud) in paren)
		{
			if (nieuwe[i] != null || gebruikt.Contains(oud) || d > maxD2) continue;
			nieuwe[i] = new Groep { Id = oud.Id, Kleur = oud.Kleur, Centrum = centra[i], Plek = sleutels[i] };
			gebruikt.Add(oud);
		}
		for (int i = 0; i < centra.Length; i++)
		{
			if (nieuwe[i] != null) continue;
			int id = volgendeId++;
			nieuwe[i] = new Groep { Id = id, Kleur = KleurVan(id), Centrum = centra[i], Plek = sleutels[i] };
		}
		return nieuwe.ToList();
	}

	// DBSCAN: punten binnen afstand zijn buren; wie genoeg buren heeft begint/verlengt een groep.
	// Label -1 = zwerver (hoort bij geen groep).
	static (int[] labels, int aantal) Dbscan(Vector2[] pos, float afstand, int minimum)
	{
		int n = pos.Length;
		float r2 = afstand * afstand;

		var buren = new List<int>[n];
		for (int i = 0; i < n; i++) buren[i] = new List<int>();
		for (int i = 0; i < n; i++)
			for (int j = i + 1; j < n; j++)
				if (pos[i].DistanceSquaredTo(pos[j]) <= r2) { buren[i].Add(j); buren[j].Add(i); }

		var labels = Enumerable.Repeat(-1, n).ToArray();
		var bezocht = new bool[n];
		int groep = 0;
		for (int i = 0; i < n; i++)
		{
			if (bezocht[i]) continue;
			bezocht[i] = true;
			if (buren[i].Count + 1 < minimum) continue; // (voorlopig) zwerver

			labels[i] = groep;
			var rij = new Queue<int>(buren[i]);
			while (rij.Count > 0)
			{
				int q = rij.Dequeue();
				if (labels[q] == -1) labels[q] = groep;
				if (bezocht[q]) continue;
				bezocht[q] = true;
				if (buren[q].Count + 1 >= minimum)
					foreach (var b in buren[q]) rij.Enqueue(b);
			}
			groep++;
		}
		return (labels, groep);
	}

	int[] KiesEnCluster(float[][] punten, float losheid, List<Groep> vorige, Random rng)
	{
		if (punten.Length == 0) return Array.Empty<int>();

		float drempel = Mathf.Lerp(0.78f, 0.92f, losheid);
		var beste = KMeans(punten, 1, vorige, rng);
		for (int k = 2; k <= Math.Min(MaxK, punten.Length); k++)
		{
			var poging = KMeans(punten, k, vorige, rng);
			if (poging.sse > beste.sse * drempel) break;
			beste = poging;
		}
		return beste.labels;
	}

	static (int[] labels, float sse) KMeans(float[][] punten, int k, List<Groep> vorige, Random rng)
	{
		// start: vorige centra als k gelijk is (stabieler), anders k-means++
		float[][] centra;
		if (vorige.Count == k) centra = vorige.Select(g => (float[])g.Centrum.Clone()).ToArray();
		else centra = KMeansPlusPlus(punten, k, rng);

		var labels = new int[punten.Length];
		float sse = 0;
		for (int iter = 0; iter < 20; iter++)
		{
			bool veranderd = false;
			sse = 0;
			for (int i = 0; i < punten.Length; i++)
			{
				int besteC = 0; float besteD = float.MaxValue;
				for (int c = 0; c < k; c++)
				{
					float d = Afstand2(punten[i], centra[c]);
					if (d < besteD) { besteD = d; besteC = c; }
				}
				if (labels[i] != besteC || iter == 0) { veranderd |= labels[i] != besteC; labels[i] = besteC; }
				sse += besteD;
			}

			var som = new float[k][];
			var aantal = new int[k];
			for (int c = 0; c < k; c++) som[c] = new float[9];
			for (int i = 0; i < punten.Length; i++)
			{
				aantal[labels[i]]++;
				for (int d = 0; d < 9; d++) som[labels[i]][d] += punten[i][d];
			}
			for (int c = 0; c < k; c++)
				if (aantal[c] > 0)
					for (int d = 0; d < 9; d++) centra[c][d] = som[c][d] / aantal[c];

			if (!veranderd && iter > 0) break;
		}
		return (labels, sse);
	}

	static float[][] KMeansPlusPlus(float[][] punten, int k, Random rng)
	{
		var centra = new List<float[]> { (float[])punten[rng.Next(punten.Length)].Clone() };
		var d2 = new float[punten.Length];
		while (centra.Count < k)
		{
			float totaal = 0;
			for (int i = 0; i < punten.Length; i++)
			{
				d2[i] = centra.Min(c => Afstand2(punten[i], c));
				totaal += d2[i];
			}
			float r = (float)rng.NextDouble() * totaal;
			int gekozen = punten.Length - 1;
			for (int i = 0; i < punten.Length; i++) { r -= d2[i]; if (r <= 0) { gekozen = i; break; } }
			centra.Add((float[])punten[gekozen].Clone());
		}
		return centra.ToArray();
	}

	// ---- hulpjes ----

	// andere modus tonen: niks wordt gewist, elke modus heeft zijn eigen groepen en historie
	public void ZetModus(Modus wijze)
	{
		Wijze = wijze;
		PasMarkeringToe();
	}

	static float[] Genen(Joekel j)
	{
		var g = new float[9];
		for (int r = 0; r < 3; r++)
			for (int c = 0; c < 3; c++)
				g[r * 3 + c] = j.Dna[r, c];
		return g;
	}

	static float Afstand2(float[] a, float[] b)
	{
		float s = 0;
		for (int i = 0; i < a.Length; i++) { float d = a[i] - b[i]; s += d * d; }
		return s;
	}

	static void Bewaar(Dictionary<int, List<float>> historie, ref int metingen, List<Groep> groepen)
	{
		metingen++;
		foreach (var g in groepen)
			if (!historie.ContainsKey(g.Id))
				historie[g.Id] = Enumerable.Repeat(0f, metingen - 1).ToList();
		foreach (var (id, lijst) in historie)
			lijst.Add(groepen.FirstOrDefault(g => g.Id == id)?.Aantal ?? 0);
	}

	static void Dun<T>(List<T> lijst)
	{
		for (int i = lijst.Count - 1; i >= 0; i -= 2) lijst.RemoveAt(i);
	}

	static void BeschrijfGroepen(List<Groep> groepen, List<Joekel> joekels, int[] labels)
	{
		var traits = Karaktereigenschap.Alle;
		var waarden = new float[joekels.Count, traits.Count];
		for (int i = 0; i < joekels.Count; i++)
			for (int t = 0; t < traits.Count; t++)
				waarden[i, t] = traits[t].Van(joekels[i].Dna);

		var gem = new float[traits.Count];
		var std = new float[traits.Count];
		for (int t = 0; t < traits.Count; t++)
		{
			for (int i = 0; i < joekels.Count; i++) gem[t] += waarden[i, t];
			gem[t] /= Math.Max(1, joekels.Count);
			for (int i = 0; i < joekels.Count; i++) std[t] += (waarden[i, t] - gem[t]) * (waarden[i, t] - gem[t]);
			std[t] = Mathf.Sqrt(std[t] / Math.Max(1, joekels.Count));
		}

		for (int c = 0; c < groepen.Count; c++)
		{
			var g = groepen[c];
			for (int r = 0; r < 3; r++)
				g.Banden[r] = new Color((g.Centrum[r * 3] + 1) / 2, (g.Centrum[r * 3 + 1] + 1) / 2, (g.Centrum[r * 3 + 2] + 1) / 2);

			var zs = new List<(string naam, float z)>();
			for (int t = 0; t < traits.Count; t++)
			{
				if (std[t] < 1e-5f) continue;
				float som = 0; int n = 0;
				for (int i = 0; i < joekels.Count; i++) if (labels[i] == c) { som += waarden[i, t]; n++; }
				if (n == 0) continue;
				zs.Add((traits[t].Naam, (som / n - gem[t]) / std[t]));
			}
			var top = zs.Where(z => Mathf.Abs(z.z) >= 0.5f).OrderByDescending(z => Mathf.Abs(z.z)).Take(3)
				.Select(z => $"{z.naam} {(z.z > 0 ? "+" : "")}{z.z:0.0}σ");

			var types = new Dictionary<string, int>();
			int leden = 0;
			for (int i = 0; i < joekels.Count; i++)
				if (labels[i] == c) { types[joekels[i].StuiterNaam] = types.GetValueOrDefault(joekels[i].StuiterNaam) + 1; leden++; }
			var typeTekst = leden > 0 ? types.OrderByDescending(kv => kv.Value).Select(kv => $"{kv.Key} {100 * kv.Value / leden}%").First() : "";

			g.Kenmerken = string.Join("  ·  ", new[] { typeTekst }.Concat(top));
		}
	}

	// null = geen markering. Zelfde id nog een keer = uit. Geldt voor de modus die nu getoond wordt.
	public void Markeer(int? id)
	{
		S.Gemarkeerd = id;
		PasMarkeringToe();
	}

	void PasMarkeringToe()
	{
		var gedimd = new Color(1, 1, 1, 0.15f);
		foreach (var n in GetParent().GetChildren())
			if (n is Joekel j)
				j.Modulate = S.Gemarkeerd == null || (S.Indeling.TryGetValue(j, out var id) && id == S.Gemarkeerd) ? Colors.White : gedimd;
	}
}
