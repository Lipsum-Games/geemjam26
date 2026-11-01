using Godot;

// FoodSources maken en op een timer laten verschijnen.
public partial class JoekelSim
{
    // Hoeveel een nieuwe source aan resource mee begint.
	const float StartResourceCount = 50f;

	// Om de hoeveel seconde een nieuwe voedsel. (hangt af van het seizoen, zie Seizoenen.cs)
	public float VoedselInterval => SeizoenVoedselInterval;
	float voedselTimer;

	// Spawn nieuwe voedsel in op positie.
	void SpawnFoodSource(Vector2 position)
	{
		if (momenteelFoodSources >= MaxFoodSources) return; // niks toen als we op de harde max zitten
		// voeg toe aan de centrally managed array.
		foodSources[momenteelFoodSources] = new FoodSourceData { Id = volgendId++, Position = position, ResourceCount = StartResourceCount };
		momenteelFoodSources++;
	}

	int IndexVanFoodSource(int id, int gok = -1)
	{
	    // gok is vorige plek waar we die zagen. als het hit is de lookup sneller.
		if (gok >= 0 && gok < momenteelFoodSources && foodSources[gok].Id == id) return gok;
		// zo niet zoek hem op
		for (int f = 0; f < momenteelFoodSources; f++)
			if (foodSources[f].Id == id) return f;
        // en niet gevonden is -1
		return -1;
	}

    // eet een bepaalde voedsel, waar de index f van is.
	float Eat(int f, float dt, float maximaalWilOfKanEten)
	{
		float hoeveelTeEten = Mathf.Min(10 * dt, foodSources[f].ResourceCount);
		hoeveelTeEten = Mathf.Min(hoeveelTeEten, maximaalWilOfKanEten);
		foodSources[f].ResourceCount -= hoeveelTeEten;
		return hoeveelTeEten;
	}

    // Vind de dichtste foodsource rond een joekel in een straal.
	int DichtsteFoodSource(int i, float straal)
	{
		int beste = -1;
		float besteScore = float.MaxValue;
		int ringen = (int)Mathf.Ceil(straal / CelGrootte);
		int cel = CelVan(joekels[i].Position);
		int celX = cel % GridBreedte;
		int celY = cel / GridBreedte;
		for (int y = celY - ringen; y <= celY + ringen; y++)
			for (int x = celX - ringen; x <= celX + ringen; x++)
			{
				if (x < 0 || y < 0 || x >= GridBreedte || y >= GridHoogte) continue;
				for (int f = celEersteFoodSource[y * GridBreedte + x]; f >= 0; f = volgendeFoodSourceInCel[f])
				{
					float afstand = joekels[i].Position.DistanceTo(foodSources[f].Position);
					if (foodSources[f].ResourceCount <= 0 || afstand > straal + FoodSourceStraal) continue; // leeg of niet te zien
					float score = afstand + GD.Randf() * 100f;
					if (score < besteScore) { besteScore = score; beste = f; }
				}
			}
		return beste;
	}

    // welke FoodSource raakt joekel i? (cirkel van chonkheid + 2, zoals de Area2D van Joekel, tegen de groene cirkel). -1 = geen
	// TIJDELIJK: kijkt naar alles at once, optimizalation komt zo wel
	int RaaktFoodSource(int i)
	{
		// alleen de FoodSources in de cel van de joekel en de 8 cellen eromheen checken
		int cel = CelVan(joekels[i].Position);
		int celX = cel % GridBreedte;
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
		SpawnFoodSource(VoedselPlek()); // waar het aangenaam warm is (zie Seizoenen.cs)
	}
}
