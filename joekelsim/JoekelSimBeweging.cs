using Godot;

// Hoe bewegen de joekels? Snelheid, wrijving, positie etc. Wordt in de physics process aangeroepen.
public partial class JoekelSim
{

    // frictie is afremsnelheid
	const float Frictie = 1f;
	// zodra joekel langzamer dan dit gaat, staan ze instantly stil (optimization)
	const float RustDrempel = 10f;

	// om de hoeveel tijd de joekel sprint. voor nu 2.6 fixed.
	const float SpringInterval = 2.6f;
	// TIJDELIJK een fixed kachow tot dna erin zit.
	const float TijdelijkeKachow = 120f;

	// TIJDELIJK: straal van de joekel komt van chonkheid van DNA later.
	const float TijdelijkeStraal = 10f;

	// welke kant op en hoe hard springt joekel i? Hier komt later behaviour.
	// TIJDELIJK: willekeurige richting, volle kracht.
	(float richting, float kracht) KiesSprong(int i)
	{
	    // guaranteed to be random (not 4)
		return (GD.Randf() * Mathf.Tau, 1f);
	}

	// één tick: elke joekel remt af en schuift een stukje op
	void BeweegJoekels(float dt)
	{
        // apply the frictie niet linear natuurlijk
		float afremming = Mathf.Exp(-Frictie * dt);

        // apply voor elke joekel
		for (int i = 0; i < momenteelLevendeJoekels; i++)
		{
			// springen: timer aftellen, op 0 een duw erbij (+=, niet vervangen) en timer opnieuw. Vóór de afremming, net als Joekel.cs
			joekels[i].SpringTimer -= dt;
			if (joekels[i].SpringTimer <= 0)
			{
				var (richting, kracht) = KiesSprong(i);
				joekels[i].Snelheid += Vector2.FromAngle(richting) * kracht * TijdelijkeKachow;
				joekels[i].SpringTimer = SpringInterval;
			}

		    // de afremming
			joekels[i].Snelheid *= afremming;

            // als rustdrempel, zet stil, anders gewoon bewegen die hap
			if (joekels[i].Snelheid.Length() > RustDrempel)
				joekels[i].Positie += joekels[i].Snelheid * dt;
			else
				joekels[i].Snelheid = Vector2.Zero;

			// muren: na het bewegen terugzetten binnen de wereld, (de straal van de joekel afstand van de muur). Stuiter is weg, veel simpeler zo.
			float r = TijdelijkeStraal;
			// gewoon clampen binnen de muren, ezpz
			joekels[i].Positie = joekels[i].Positie.Clamp(new Vector2(r, r), World.WorldSize - new Vector2(r, r));
		}
	}
}
