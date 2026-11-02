using Godot;

// Drolhopen stinken. Wie te dichtbij staat verliest gezondheid, hoe groter de hoop hoe verder en harder.
public partial class JoekelSim
{
	// stank niet elke tick (classic optimization)
	const float StankInterval = 0.1f;
	// bereik = basis + per wortel van de hoeveelheid
	const float StankBasisBereik = 30f;
	const float StankBereikPerWortel = 10f;
	// schade per seconde per wortel van de hoeveelheid
	const float StankSchadePerWortel = 0.5f;

	float stankTimer;

	// warmte laat stank verder reiken: winter 0.7x, zomer 1.3x
	float StankBereik(float wortel) => (StankBasisBereik + StankBereikPerWortel * wortel) * (0.7f + 0.6f * Warmte);

	void Stank(float dt)
	{
		stankTimer += dt;
		if (stankTimer < StankInterval) return;
		float stankDt = stankTimer;
		stankTimer = 0;

		for (int h = 0; h < momenteelDrolHopen; h++)
		{
			float wortel = Mathf.Sqrt(drolHopen[h].Hoeveelheid);
			float bereik = StankBereik(wortel);
			// BurenRond kijkt naar middelpunten, dus de dikste joekel erbij voor wie er net met zijn rand in staat
			int aantal = BurenRond(drolHopen[h].Position, bereik + grootsteChonkheid);
			for (int k = 0; k < aantal; k++)
			{
				int j = burenBuffer[k];
				// strontvreters zijn immuum dus die skippen we
				if (drolHopen[h].Position.DistanceTo(joekels[j].Position) >= bereik + joekels[j].Chonkheid) continue;
				joekels[j].HuidigeGezonheid -= StankSchadePerWortel * wortel * stankDt * (1 - joekels[j].Strontvreterij);
			}
		}
	}
}
