using Godot;

// Ranzigheid: de aura. = RanzigeAura en LynxEffect uit de oude Joekel.cs
public partial class JoekelSim
{
	// geur exports voor wanneer heal en plateau enzo
	const float GeurHealZone = 0.1f;
	const float GeurPlateau = 0.05f;
	const float GeurVolleSchade = 0.5f;
	const float GeurHealKracht = 0.3f;

	// optimization, niet elke frame aura applying is goedkoper. Dit gaat per frame.
	const float AuraInterval = 16f / 60f;

	void Aura(float dt)
	{
		for (int i = 0; i < momenteelLevendeJoekels; i++)
		{
			if (joekels[i].HuidigeGezonheid <= 0) continue;
			if (joekels[i].RanzigKracht > 0) {
				// scale de aura per time delta
				joekels[i].AuraTijd += dt;
				if (joekels[i].AuraTijd >= AuraInterval) {
					RanzigeAura(i, joekels[i].AuraTijd); // schade over de echt verstreken tijd
					joekels[i].AuraTijd = 0;
				}
			}
		}
	}

	private float LynxEffect(float verschil)
		{
			// de lynx effect scalet geur damage/heal accoridng to exported values helemaal boven
			if (verschil < GeurHealZone)
				return -GeurHealKracht * (1 - verschil / GeurHealZone);

			float start = GeurHealZone + GeurPlateau;
			return Mathf.Max(0f, (verschil - start) / (GeurVolleSchade - start));
		}

	private void RanzigeAura(int i, float dt)
	{
		int aantal = BurenVan(i, joekels[i].RanzigBereik);
		for (int k = 0; k < aantal; k++)
		{
			int ander = burenBuffer[k];

			float afstand = joekels[i].Position.DistanceTo(joekels[ander].Position);
			if (afstand >= joekels[i].RanzigBereik) continue; // niet ranzig genoeg

			// tot op welke hoogte je geroken kan worden, je geurkracht ofzo
			float kan_ik_je_ruiken_hoeveelheid = 1 - afstand / joekels[i].RanzigBereik;
			// als jouw geur lijkt op die van de ander dan bentei ei misschien toch nie zo vies als ie dacht hé
			float eigenlijk_toch_niet_zo_ranzigheid = LynxEffect(Mathf.Abs(joekels[i].Geur - joekels[ander].Geur));
			// pas jouw ranzigheid toe op de gezondheid van de wederpartij
			joekels[ander].HuidigeGezonheid -= joekels[i].RanzigKracht * kan_ik_je_ruiken_hoeveelheid * eigenlijk_toch_niet_zo_ranzigheid * dt;
			joekels[ander].HuidigeGezonheid = Mathf.Min(joekels[ander].HuidigeGezonheid, joekels[ander].MaximaleGezondheid);
		}
	}
}
