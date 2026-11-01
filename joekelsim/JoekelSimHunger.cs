using Godot;

// De honger mangement
public partial class JoekelSim
{
	const float HungerMin = 1f;  // helemaal dik vol (garfield)
	const float HungerMax = 10f; // helemaal uitgehongerd (floatmaxxing)

    // voor elke dt maak ze meer/minder hongerig liggend aan of er voedsel is wettewel
	void CalculateHunger(float dt)
	{
		for (int i = 0; i < momenteelLevendeJoekels; i++)
		{
			// honger loopt op met de tijd, dikke natuurlijk sneller
			float hongerOploopFactor = joekels[i].Chonkheid * dt / 100
			joekels[i].Hunger = Mathf.Min(joekels[i].Hunger + hongerOploopFactor, HungerMax);

			// honger van 0 tot 1, als het boven 0.7 gaat gaat er gezondheid af, hoe hongeriger hoe harder (exponentially)
			float honger = (joekels[i].Hunger - HungerMin) / (HungerMax - HungerMin);
			if (honger > 0.7f) joekels[i].HuidigeGezonheid -= 5f * dt * honger * honger;
		}
	}
}
