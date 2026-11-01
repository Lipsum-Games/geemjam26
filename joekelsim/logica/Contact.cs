using Godot;
using Geemjam26ShipsumEvolve.genetica;

// Contact: wat er gebeurt als twee joekels elkaar aanraken.
// Nu alleen aanvallen. Voortplanten komt erbij als Behaviour het REPRODUCE-plan heeft.
public partial class JoekelSim
{
	// contact elke 4 ticks voor optimilization.
	const int ContactElkeNStappen = 4;

	// alleen voortplanten als honger lager is dan dit
	const float ReproductionHunger = 2f;
	const float ReproductionHungerCost = 4f;
	const float AgeOfConsent = 200f / 60f;

	void Contact(float dt)
	{
		for (int i = 0; i < momenteelLevendeJoekels; i++)
		{
			if (joekels[i].HuidigeGezonheid <= 0) continue; // deze tick doodgegaan? nix meer doen
		    // eerst math om te checken of we contact moeten checken.
			joekels[i].ContactTijd += dt;
			if (++joekels[i].ContactTeller < ContactElkeNStappen) continue;
			float contactDt = joekels[i].ContactTijd;
			joekels[i].ContactTeller = 0;
			joekels[i].ContactTijd = 0;

			// wie raak ik? check om me heen.
			int aantal = BurenVan(i, joekels[i].Chonkheid + 2 + Karaktereigenschappen.MaxChonkheid);
			for (int k = 0; k < aantal; k++)
			{
			    // j is dus de andere joekel index
				int j = burenBuffer[k];
				float raakAfstand = joekels[i].Chonkheid + 2 + joekels[j].Chonkheid;
				if (joekels[i].Position.DistanceSquaredTo(joekels[j].Position) >= raakAfstand * raakAfstand) continue; // net niet? continue

				// hoe verschillend is ons DNA?
				float verschil = joekels[i].Dna.Similarity(joekels[i].Dna, joekels[j].Dna);
				if (CheckReproduce(i) && verschil < 6)
				{
					Reproduce(i, j);
				}
				// groot verschil = batel
				else if (joekels[i].CurrentPlan == Behaviour.Plan.HUNT && verschil > 1)
				{
					joekels[j].HuidigeGezonheid -= joekels[i].Chonkheid * 60f * contactDt;
					joekels[i].Hunger = Mathf.Max(0.0f, joekels[i].Hunger - Mathf.Max(joekels[i].Vleeslustigheid + 0.3f, 0f) * 10f * contactDt);
				}
			}
		}
	}

	bool CheckReproduce(int i)
	{
		if (joekels[i].CurrentPlan == Behaviour.Plan.REPRODUCE)
		{
			joekels[i].CurrentPlan = Behaviour.Plan.CHILL;
			return true;
		} else
		{
			return false;
		}
	}

	void Reproduce(int i, int j)
	{
		if (joekels[i].Age < AgeOfConsent || joekels[i].Hunger > ReproductionHunger)
			return;
		if(joekels[i].Dna.Similarity(joekels[i].Dna, joekels[j].Dna) < 4 && GD.Randf() < 0.6f)
			return;
		SpawnJoekel(joekels[i].Position, Dna.Combine(joekels[i].Dna, joekels[j].Dna));
		joekels[i].Hunger += ReproductionHungerCost;
	}
}
