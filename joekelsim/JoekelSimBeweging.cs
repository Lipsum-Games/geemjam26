using Godot;

// Hoe bewegen de joekels? Snelheid, wrijving, positie etc. Wordt in de physics process aangeroepen.
public partial class JoekelSim
{

    // frictie is afremsnelheid
	const float Frictie = 1f;
	// zodra joekel langzamer dan dit gaat, staan ze instantly stil (optimization)
	const float RustDrempel = 10f;

	// TIJDELIJK: tot ze kunnen springen (stap 2c) krijgt elke joekel bij de start één duw in een willekeurige richting
	void GeefStartDuwtje()
	{
		for (int i = 0; i < momenteelLevendeJoekels; i++)
		{
			joekelSnelheden[i] = Vector2.FromAngle(GD.Randf() * Mathf.Tau) * GD.Randf() * 200f;
		}
	}

	// één tick: elke joekel remt af en schuift een stukje op
	void BeweegJoekels(float dt)
	{
        // apply the frictie niet linear natuurlijk
		float afremming = Mathf.Exp(-Frictie * dt);

        // apply voor elke joekel
		for (int i = 0; i < momenteelLevendeJoekels; i++)
		{
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
