using Godot;

// Een raster (grid) over de wereld, zodat we altijd snel kunnen checken wie bij ons in de buurt is (big optimization)
// De wereld is verdeeld in cellen van x*y px. Elke tick noteren we per cel welke joekels erin zitten.
// Voor buren hoef je dan alleen je eigen cel en de 8 eromheen te bekijken, ipv alle joekels :WICKED:
public partial class JoekelSim
{
	const float CelGrootte = 64f;
	static readonly int GridBreedte = (int)(World.WorldSize.X / CelGrootte) + 1;
	static readonly int GridHoogte = (int)(World.WorldSize.Y / CelGrootte) + 1;

	// per cel: de eerste joekel die erin zit (-1 = leeg)
	int[] celEersteJoekel = new int[GridBreedte * GridHoogte];
	// per joekel: de volgende joekel in dezelfde cel (-1 = geen meer).
	// Zo is elke cel een ketting: celEersteJoekel[cel] -> volgendeJoekelInCel[..] -> ... -> -1
	int[] volgendeJoekelInCel = new int[MaxJoekels];

	// in welke cel deze world coord valt. De cellen zijn rij voor rij genummerd: cel = y * breedte + x (niet een matrix ofzo)
	int CelVan(Vector2 positie)
	{
		int x = Mathf.Clamp((int)(positie.X / CelGrootte), 0, GridBreedte - 1);
		int y = Mathf.Clamp((int)(positie.Y / CelGrootte), 0, GridHoogte - 1);
		return y * GridBreedte + x;
	}

	// We bouwen dit elke tick opnieuw (is vrij snel, scalet linearly)
	void BouwJoekelGrid()
	{
		System.Array.Fill(celEersteJoekel, -1);
		for (int i = 0; i < momenteelLevendeJoekels; i++)
		{
			int cel = CelVan(joekels[i].Position);
			volgendeJoekelInCel[i] = celEersteJoekel[cel]; // wie er al zat komt achter mij
			celEersteJoekel[cel] = i;                      // en ik ben nu de eerste
		}
	}

	// zelfde grid als hierboven en dan voor voedsel.
	int[] celEersteFoodSource = new int[GridBreedte * GridHoogte];
	int[] volgendeFoodSourceInCel = new int[MaxFoodSources];

	void BouwFoodSourceGrid()
	{
		System.Array.Fill(celEersteFoodSource, -1);
		for (int f = 0; f < momenteelFoodSources; f++)
		{
			int cel = CelVan(foodSources[f].Position);
			volgendeFoodSourceInCel[f] = celEersteFoodSource[cel];
			celEersteFoodSource[cel] = f;
		}
	}

	// De BurenVan functie beantwoord: wie staat er binnen zoveel pixels van mij?
	//
	// Wij hebben dus deze Grids hierboven om checken efficient te maken. IPV alle joekels checken we alleen in de vakjes om ons heen.
	// En we kijken zover vakjes weg als dat de straal is dat we checken
    // Als joekels close zijn hebben we ook een buffer waar we ze inschrijven, lekker snel.
	//
	// Gebruik:
	//     int aantal = BurenVan(i, 100);                 // hoeveel buren binnen 100 px?
	//     for (int k = 0; k < aantal; k++)
	//     {
	//         int j = burenBuffer[k];                    // j is een buurjoekel
	//         ... doe iets met joekels[j] ...
	//     }
	//
	// Let op: burenBuffer is een gedeeld kladblaadje. De volgende BurenVan wist het weer,
	// dus roep BurenVan niet aan binnen zo'n loop (maar ik weet dat je niet zoiets gaat doen hmmm??)
	int[] burenBuffer = new int[MaxJoekels];

	int BurenVan(int i, float straal)
	{
		int aantal = 0;

		// hoeveel vakjes ver moeten er gekeken worden? Straal 64 of minder: 1 vakje (de 8 eromheen). Straal 100: 2 vakjes ver, etc. etc.
		int ringen = (int)Mathf.Ceil(straal / CelGrootte);

		// in welk vakje sta ik? (vakjenummer terug naar kolom x en rij y)
		int cel = CelVan(joekels[i].Position);
		int celX = cel % GridBreedte;
		int celY = cel / GridBreedte;

		// alle vakjes in een vierkant om mij heen checken
		for (int y = celY - ringen; y <= celY + ringen; y++)
		{
			for (int x = celX - ringen; x <= celX + ringen; x++)
			{
				if (x < 0 || y < 0 || x >= GridBreedte || y >= GridHoogte) continue; // vakje bestaat niet (buiten de wereld)

				// alle joekels in dit vakje: begin bij de eerste, volg de ketting tot -1
				for (int j = celEersteJoekel[y * GridBreedte + x]; j >= 0; j = volgendeJoekelInCel[j])
				{
					// niet mezelf, en echt dichtbij genoeg?
					if (j != i && joekels[i].Position.DistanceSquaredTo(joekels[j].Position) < straal * straal)
						burenBuffer[aantal++] = j; // opschrijven, en de teller ++'en
				}
			}
		}
		return aantal;
	}

    // Dichtsbijzijnde andere joekel in de wereld vinden.
	// Zoekt met BurenVan in een cirkel die steeds 2x groter wordt tot er iemand in zit
	int ClosestJoekel(int i)
	{
		for (float straal = CelGrootte; straal < 2 * (World.WorldSize.X + World.WorldSize.Y); straal *= 2)
		{
			int aantal = BurenVan(i, straal);
			if (aantal == 0) continue;
			int beste = burenBuffer[0];
			for (int k = 1; k < aantal; k++)
				if (joekels[i].Position.DistanceSquaredTo(joekels[burenBuffer[k]].Position) < joekels[i].Position.DistanceSquaredTo(joekels[beste].Position))
					beste = burenBuffer[k];
			return beste;
		}
		return -1;
	}

	// Helper functie voor birth, past er een joekel op deze nieuwe plek?
	bool IsPlekVrij(Vector2 positie, float straal)
	{
	    // Zit joekel in een muur?
		if (positie.X - straal < 0 || positie.Y - straal < 0 || positie.X + straal > World.WorldSize.X || positie.Y + straal > World.WorldSize.Y)
			return false;

        // Zit joekel in een andere joekel?
		int ringen = (int)Mathf.Ceil((straal + Karaktereigenschappen.MaxChonkheid) / CelGrootte);
		int cel = CelVan(positie);
		int celX = cel % GridBreedte;
		int celY = cel / GridBreedte;
		for (int y = celY - ringen; y <= celY + ringen; y++)
			for (int x = celX - ringen; x <= celX + ringen; x++)
			{
				if (x < 0 || y < 0 || x >= GridBreedte || y >= GridHoogte) continue;
				for (int j = celEersteJoekel[y * GridBreedte + x]; j >= 0; j = volgendeJoekelInCel[j])
					if (positie.DistanceTo(joekels[j].Position) < straal + joekels[j].Chonkheid)
						return false;
			}
		return true;
	}
}
