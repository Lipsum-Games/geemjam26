using Godot;

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

	// boven dit DNA-verschil zijn het eigenlijk andere soorten
	const float SoortGrens = 40f;
	const float SoortGrensMiskraamKans = 0.5f;
	// onder dit DNA-verschil is het inteelt (bijna-klonen)
	const float InteeltGrens = -1.5f;

	// hoeveel honger een kill wegneemt per chonk van de prooi
	const float VleesPerChonk = 1.2f;

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
			int aantal = BurenVan(i, joekels[i].Chonkheid + 2 + grootsteChonkheid);
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
				// groot verschil = batel (alleen als het een planteneter is)
				else if (joekels[i].CurrentPlan == Plan.HUNT && verschil > 1)
				{
					bool leefde = joekels[j].HuidigeGezonheid > 0;
					joekels[j].HuidigeGezonheid -= joekels[i].Chonkheid * 20f * contactDt;
					joekels[i].Hunger = Mathf.Max(0.0f, joekels[i].Hunger - joekels[j].Chonkheid * contactDt);

					// doodgemaakt? opeten! hoe dikker de prooi hoe meer vlees, hoe vleeslustiger hoe meer je eruit haalt
					// en dan is de jacht klaar: lekker rondhangen tot hij weer honger heeft
					if (leefde && joekels[j].HuidigeGezonheid <= 0)
					{
						joekels[i].Hunger = Mathf.Max(HungerMin, joekels[i].Hunger - Mathf.Max(joekels[i].Vleeslustigheid, 0) * joekels[j].Chonkheid * VleesPerChonk);
						StopPlan(ref joekels[i]);
					}
				}
			}
		}
	}

	bool CheckReproduce(int i)
	{
		if (joekels[i].CurrentPlan == Plan.REPRODUCE)
		{
			joekels[i].CurrentPlan = Plan.CHILL;
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
		// inteelt: bijna-klonen krijgen vaak een miskraam
		if(joekels[i].Dna.Similarity(joekels[i].Dna, joekels[j].Dna) < InteeltGrens && GD.Randf() < 0.6f)
			return;
		// soortgrens: te verschillend DNA = vaak een miskraam, zo blijven soorten die uit elkaar gegroeid zijn apart
		if(joekels[i].Dna.Similarity(joekels[i].Dna, joekels[j].Dna) > SoortGrens && GD.Randf() < SoortGrensMiskraamKans)
			return;
		SpawnJoekel(
			joekels[i].Position, 
			Dna.Combine(joekels[i].Dna, joekels[j].Dna, joekels[i].ReproductionValues),
			joekels[i].NeuralNet.Reproduce(joekels[j].NeuralNet, joekels[i].ReproductionValues)
		);
		joekels[i].Hunger += ReproductionHungerCost;
	}
}
