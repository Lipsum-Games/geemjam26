using Godot;

// Ranzigheid: de aura
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
				joekels[i].AuraFase = (joekels[i].AuraFase + dt) % (joekels[i].AuraPauze + joekels[i].AuraPulsDuur);
				// scale de aura per time delta
				joekels[i].AuraTijd += dt;
				if (joekels[i].AuraTijd >= AuraInterval) {
					RanzigeAura(i, joekels[i].AuraTijd); // schade over de echt verstreken tijd
					joekels[i].AuraTijd = 0;
				}
			}
		}
	}

	// hoe sterk de aura nu is (0..1): 0 tijdens de pauze, daarna een belcurve over de puls met de piek precies halverwege
	float AuraPuls(int i)
	{
		float t = joekels[i].AuraFase - joekels[i].AuraPauze; // tijd sinds de puls begon
		if (t < 0) return 0;
		float midden = joekels[i].AuraPulsDuur / 2;
		float breedte = joekels[i].AuraPulsDuur / 6; // dan is hij aan het begin en eind van de puls ~0
		return Mathf.Exp(-(t - midden) * (t - midden) / (2 * breedte * breedte));
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
			joekels[ander].HuidigeGezonheid -= joekels[i].RanzigKracht * AuraPuls(i) * kan_ik_je_ruiken_hoeveelheid * eigenlijk_toch_niet_zo_ranzigheid * dt;
			joekels[ander].HuidigeGezonheid = Mathf.Min(joekels[ander].HuidigeGezonheid, joekels[ander].MaximaleGezondheid);
		}
	}
}
