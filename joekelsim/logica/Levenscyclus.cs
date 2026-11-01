using Godot;
using System;

// De hele joekel lifecycle, geboorte en dood wordt hier geregeld.
public partial class JoekelSim
{
	int IndexVanJoekel(int id, int gok = -1)
	{
        // gok is vorige plek waar we die zagen. als het hit is de lookup sneller.
		if (gok >= 0 && gok < momenteelLevendeJoekels && joekels[gok].Id == id) return gok;
        // zo niet zoek hem op
		for (int i = 0; i < momenteelLevendeJoekels; i++)
			if (joekels[i].Id == id) return i;
        // en niet gevonden is -1
		return -1;
	}

	// Maak een nieuwe joekel op deze plek. Hij komt achteraan in de array (op plek momenteelLevendeJoekels).
	// Als de array vol is, dan gebeurt er niets.
	// Geeft true terug als er echt een joekel is gemaakt
	bool MaakJoekel(Vector2 positie, Dna dna)
	{
		if (momenteelLevendeJoekels >= MaxJoekels) return false;

		// struct op de plaats van de joekel
		joekels[momenteelLevendeJoekels] = new JoekelData
		{
			Id = volgendId++,
			CurrentPlan = Geemjam26ShipsumEvolve.genetica.Behaviour.Plan.CHILL,
			Position = positie,
			// willekeurige start, anders springen ze allemaal in dezelfde tick
			SpringTimer = GD.Randf() * SpringInterval,

			// Hier worden de eigenschappen initialized uit DNA.
  			Dna = dna,
			Kachow = Karaktereigenschappen.Kachow.Van(dna),
			Chonkheid = Karaktereigenschappen.Chonkheid.Van(dna),
			MaximaleGezondheid = Karaktereigenschappen.DoodgaansSnelheidEnAlgemeneGezondheid.Van(dna),
			HuidigeGezonheid = Karaktereigenschappen.DoodgaansSnelheidEnAlgemeneGezondheid.Van(dna), // begint vol
			Hunger = (HungerMin + HungerMax) / 2, // begint half hongerig
			Vleeslustigheid = Karaktereigenschappen.Vleeslustigheid.Van(dna),
			Perception = Karaktereigenschappen.Perception.Van(dna),
			Introversie = Karaktereigenschappen.Introversie.Van(dna),
			Oehlalalaigheid = Karaktereigenschappen.Oehlalalaigheid.Van(dna),
			ChillSpeed = Karaktereigenschappen.ChillSpeed.Van(dna),
			Heading = GD.Randf() * Mathf.Tau,
			Trek = Karaktereigenschappen.Trek.Van(dna) * (HungerMax - HungerMin) + HungerMin,
		};

		// meteen in de grid plaatsen, zodat wie hierna geboren wordt hem al ziet
		int cel = CelVan(positie);
		volgendeJoekelInCel[momenteelLevendeJoekels] = celEersteJoekel[cel];
		celEersteJoekel[cel] = momenteelLevendeJoekels;

		momenteelLevendeJoekels++;
		return true;
	}

	void SpawnJoekel(Vector2 Position, Dna Dna){
		var radius = Karaktereigenschappen.Chonkheid.Van(Dna);

		var success = false;
		var finalPosition = Position;

        // Probeer 5x een joekel te spawnen in de buurt.
		for (int i = 0; i < 5; i++){
			var dir = GD.Randf() * Math.Tau;
			var candidatePosition = Position + new Vector2(((float) Math.Cos(dir)) * (radius * 2.1f), ((float) Math.Sin(dir)) * (radius * 2.1F));
			if (IsPlekVrij(candidatePosition, radius)){
				finalPosition = candidatePosition;
				success = true;
				break;
			}
		}
		if (success) {
			MaakJoekel(finalPosition, Dna);
		}
	}

	void VerwijderJoekel(int i)
	{
		if (i < 0 || i >= momenteelLevendeJoekels) { GD.PushError($"VerwijderJoekel({i}), maar er leven er maar {momenteelLevendeJoekels}"); return; }
		momenteelLevendeJoekels--;
		// Als joekel i dood gaat, plaats de laatste joekel gewoon op zijn plek (dikke swap), is lekker efficient.
		joekels[i] = joekels[momenteelLevendeJoekels];
	}

	// alle joekels met 0 gezonheid gaan DOOD
	void RuimDodenOp()
	{
	    // achterstevoren zodat de joekel die naar plek i verhuist van achteren komt dus al is bekekem
		for (int i = momenteelLevendeJoekels - 1; i >= 0; i--)
		{
			if (joekels[i].HuidigeGezonheid <= 0) VerwijderJoekel(i);
		}
	}
}
