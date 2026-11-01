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
