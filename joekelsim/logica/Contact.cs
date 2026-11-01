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
				// groot verschil = batel
				if (verschil > 6)
				{
					joekels[j].HuidigeGezonheid -= joekels[i].Chonkheid * 60f * contactDt;
					joekels[i].Hunger -= Mathf.Max(joekels[i].Vleeslustigheid - 0.5f, 0) * 60f * contactDt;
				}
			}
		}
	}
}
