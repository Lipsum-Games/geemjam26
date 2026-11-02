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
			if (joekels[i].HuidigeGezonheid <= 0) continue; // deze tick doodgegaan: kan niet meer eten en herleven

            // als we voedsel aanraken (een valida f index voor voedsel)
			int f = RaaktFoodSource(i);
			if (f >= 0)
			{
			    float maximaalWilOfKanEten = (joekels[i].Hunger - HungerMin) * Mathf.Max((joekels[i].Vleeslustigheid - 0.5f) * -1, 0);
				// strontvreters eten planten iets minder goed
				maximaalWilOfKanEten *= 1 - StrontvreterPlantenMalus * joekels[i].Strontvreterij;
				float eating = Eat(f, dt, maximaalWilOfKanEten);
				joekels[i].HuidigeGezonheid = Mathf.Min(joekels[i].HuidigeGezonheid + 2f * eating, joekels[i].MaximaleGezondheid);
				joekels[i].Hunger -= eating;
			}

			// honger loopt op met de tijd, dikke natuurlijk sneller
			float hongerOploopFactor = joekels[i].Chonkheid * dt / 100;
			// kou en hitte kosten ook honger (zie Seizoenen.cs)
			hongerOploopFactor += (KouHonger(i) + HitteHonger(i)) * dt;
			joekels[i].Hunger = Mathf.Min(joekels[i].Hunger + hongerOploopFactor, HungerMax);

			// honger van 0 tot 1, als het boven 0.7 gaat gaat er gezondheid af, hoe hongeriger hoe harder (exponentially)
			float honger = (joekels[i].Hunger - HungerMin) / (HungerMax - HungerMin);
			if (honger > 0.7f) joekels[i].HuidigeGezonheid -= 5f * dt * honger * honger;
		}
	}
}
