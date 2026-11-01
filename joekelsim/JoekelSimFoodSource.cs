using Godot;

// FoodSources maken en op een timer laten verschijnen.
public partial class JoekelSim
{
    // Hoeveel een nieuwe source aan resource mee begint.
	const float StartResourceCount = 50f;

	// Om de hoeveel seconde een nieuwe voedsel.
	public float VoedselInterval = 2f;
	float voedselTimer;

	// Spawn nieuwe voedsel in op positie.
	void SpawnFoodSource(Vector2 position)
	{
		if (momenteelFoodSources >= MaxFoodSources) return; // niks toen als we op de harde max zitten
		// voeg toe aan de centrally managed array.
		foodSources[momenteelFoodSources] = new FoodSourceData { Position = position, ResourceCount = StartResourceCount };
		momenteelFoodSources++;
	}

    // eet een bepaalde voedsel, waar de index f van is.
	float Eat(int f, float dt, float maximaalWilOfKanEten)
	{
		float hoeveelTeEten = Mathf.Min(10 * dt, foodSources[f].ResourceCount);
		hoeveelTeEten = Mathf.Min(hoeveelTeEten, maximaalWilOfKanEten);
		foodSources[f].ResourceCount -= hoeveelTeEten;
		return hoeveelTeEten;
	}

	// welke FoodSource raakt joekel i? (cirkel van chonkheid + 2, zoals de Area2D van Joekel, tegen de groene cirkel). -1 = geen
	// TIJDELIJK: kijkt naar alles at once, optimizalation komt zo wel
	int RaaktFoodSource(int i)
	{
		// alleen de FoodSources in de cel van de joekel en de 8 cellen eromheen checken
		int cel = CelVan(joekels[i].Position);
		int celX = cel % GridBreedte
		int celY = cel / GridBreedte;
		for (int y = celY - 1; y <= celY + 1; y++)
		{
			for (int x = celX - 1; x <= celX + 1; x++)
			{
				if (x < 0 || y < 0 || x >= GridBreedte || y >= GridHoogte) continue; // buiten de wereld, dat checken we niet ofcourse.
				for (int f = celEersteFoodSource[y * GridBreedte + x]; f >= 0; f = volgendeFoodSourceInCel[f])
				{
				    // Is het deze tick al opgepeuzeld? Zo ja verder.
					if (foodSources[f].ResourceCount <= 0) continue;
					// Check daarna of dit voedsel in onze voedselrange zit. Zo ja return voedsel index f
					float afstand = joekels[i].Chonkheid + 2 + FoodSourceStraal;
					if (joekels[i].Position.DistanceSquaredTo(foodSources[f].Position) < afstand * afstand) return f;
				}
			}
		}
		return -1;
	}

	// lege FoodSources opruimen.
	void RuimLegeFoodSourcesOp()
	{
	    // Achterstevoren + swap-remove, net als RuimDodenOp
		for (int f = momenteelFoodSources - 1; f >= 0; f--)
		{
			if (foodSources[f].ResourceCount > 0) continue;
			momenteelFoodSources--;
			foodSources[f] = foodSources[momenteelFoodSources];
		}
	}

	// Beweeg de timer zodat nieuwe voedsel spawnt uiteindelijk
	void UpdateVoedselTimer(float dt)
	{
	    // per dt natuurlijk
		voedselTimer -= dt;
		if (voedselTimer > 0) return;
		voedselTimer = VoedselInterval;
		SpawnFoodSource(new Vector2(GD.Randf() * World.WorldSize.X, GD.Randf() * World.WorldSize.Y));
	}
}
