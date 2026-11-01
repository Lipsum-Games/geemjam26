using Godot;

// De hele joekel lifecycle, geboorte en dood wordt hier geregeld.
public partial class JoekelSim
{
	// Maak een nieuwe joekel op deze plek. Hij komt achteraan in de array (op plek momenteelLevendeJoekels).
	// Als de array vol is, dan gebeurt er niets.
	// Geeft true terug als er echt een joekel is gemaakt
	bool MaakJoekel(Vector2 positie, Dna dna)
	{
		if (momenteelLevendeJoekels >= MaxJoekels) return false;

		// struct op de plaats van de joekel
		joekels[momenteelLevendeJoekels] = new JoekelData
		{
			Positie = positie,
			// willekeurige start, anders springen ze allemaal in dezelfde tick
			SpringTimer = GD.Randf() * SpringInterval,

			// Hier worden de eigenschappen initialized uit DNA.
  			Dna = dna,
			Kachow = Karaktereigenschappen.Kachow.Van(dna),
			Straal = Karaktereigenschappen.Chonkheid.Van(dna),
		};
		momenteelLevendeJoekels++;
		return true;
	}

	void VerwijderJoekel(int i)
	{
		if (i < 0 || i >= momenteelLevendeJoekels) { GD.PushError($"VerwijderJoekel({i}), maar er leven er maar {momenteelLevendeJoekels}"); return; }
		momenteelLevendeJoekels--;
		// Als joekel i dood gaat, plaats de laatste joekel gewoon op zijn plek (dikke swap), is lekker efficient.
		joekels[i] = joekels[momenteelLevendeJoekels];
	}
}
