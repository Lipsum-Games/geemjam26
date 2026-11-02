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

	// één tick: elke joekel remt af en schuift een stukje op
	void BeweegJoekels(float dt)
	{
		// apply the frictie niet linear natuurlijk
		float afremming = Mathf.Exp(-Frictie * dt);

		// apply voor elke joekel
		for (int i = 0; i < momenteelLevendeJoekels; i++)
		{
			joekels[i].Age += dt;
			// springen: timer aftellen, op 0 een duw erbij (+=, niet vervangen) en timer opnieuw. Vóór de afremming
			var (richting, kracht) = DoeDingen(i, dt);
			joekels[i].SpringTimer -= dt;
			if (joekels[i].SpringTimer <= 0)
			{
				richting += joekels[i].Drunkness * (float)GD.Randfn(0, 1); // meestal een beetje mis, soms flink als je zeer dronken bent
				joekels[i].Heading = richting;
				joekels[i].Velocity += Vector2.FromAngle(richting) * kracht * GetVelocityVal(i);
				joekels[i].SpringTimer = SpringInterval;
			}

			// de afremming
			joekels[i].Velocity *= afremming;

			// als rustdrempel, zet stil, anders gewoon bewegen die hap
			if (joekels[i].Velocity.Length() > RustDrempel)
				joekels[i].Position += joekels[i].Velocity * dt;
			else
				joekels[i].Velocity = Vector2.Zero;

			// botsen met andere joekels, alleen als hij beweegt (net als move and slide)
			if (joekels[i].Velocity != Vector2.Zero) BotsMetAndereJoekels(i);

			// muren: na het bewegen terugzetten binnen de wereld, (de straal van de joekel afstand van de muur). Stuiter is weg, veel simpeler zo.
			float r = joekels[i].Chonkheid;
			// gewoon clampen binnen de muren, ezpz
			joekels[i].Position = joekels[i].Position.Clamp(new Vector2(r, r), World.WorldSize - new Vector2(r, r));
		}
	}

	// Wat MoveAndSlide tegen andere joekels doet: overlap ik iemand, dan schuif ik mezelf eruit (de ander blijft staan). glibberig om iemand heen glijden jwz.
	void BotsMetAndereJoekels(int i)
	{
		int aantal = BurenVan(i, joekels[i].Chonkheid + grootsteChonkheid);
		for (int k = 0; k < aantal; k++)
		{
			// j is de index van de andere joekel
			int j = burenBuffer[k];
			float raakAfstand = joekels[i].Chonkheid + joekels[j].Chonkheid;
			Vector2 vanHemNaarMij = joekels[i].Position - joekels[j].Position;
			float afstand = vanHemNaarMij.Length();
			// geen overlap dan gaan we gewoon verder.
			if (afstand >= raakAfstand || afstand == 0) continue;

			// de weg vector, zodat we glibberig langs glijden.
			Vector2 weg = vanHemNaarMij / afstand;
			joekels[i].Position = joekels[j].Position + weg * raakAfstand;

			// < 0: ik beweeg (deels) zijn kant op
			float naarHemToe = joekels[i].Velocity.Dot(weg);
			// dat stuk eraf en de rest van de beweging blijft
			if (naarHemToe < 0) joekels[i].Velocity -= weg * naarHemToe;
		}
	}

	float GetVelocityVal(int i)
	{
		float result = joekels[i].Kachow;
		float honger = (joekels[i].Hunger - HungerMin) / (HungerMax - HungerMin);
		// 100-50% gevuld is geen penatly, daarna linear van 100% naar 70% speed
		result *= 1f - 0.3f * Mathf.Clamp((honger - 0.5f) / 0.5f, 0f, 1f);
		var joekelTile = WorldTiles.GetTileAt(joekels[i].Position);
		if (joekelTile != null) {
			// modifier from the tile the joekel is on
			result *= joekelTile.GetJoekelSpeedMultiplier(joekels[i].Zwemdiplomas);
		}
		return result;
	}
}
