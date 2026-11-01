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

	// willekeurige start, anders springen ze allemaal in dezelfde tick
	void ZetStartSpringTimers()
	{
		for (int i = 0; i < momenteelLevendeJoekels; i++)
		{
			joekelSpringTimers[i] = GD.Randf() * SpringInterval;
		}
	}

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
			joekelSpringTimers[i] -= dt;
			if (joekelSpringTimers[i] <= 0)
			{
				var (richting, kracht) = KiesSprong(i);
				joekelSnelheden[i] += Vector2.FromAngle(richting) * kracht * TijdelijkeKachow;
				joekelSpringTimers[i] = SpringInterval;
			}

		    // de afremming
			joekelSnelheden[i] *= afremming;

            // als rustdrempel, zet stil, anders gewoon bewegen die hap
			if (joekelSnelheden[i].Length() > RustDrempel)
				joekelPosities[i] += joekelSnelheden[i] * dt;
			else
				joekelSnelheden[i] = Vector2.Zero;
		}
	}
}
