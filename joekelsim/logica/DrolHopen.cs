using Godot;

// Gelande drollen worden hopen. Landt er een vlakbij een bestaande hoop, dan groeit die hoop.
public partial class JoekelSim
{
	const int MaxDrolHopen = 4096;
	// max CelGrootte (64), want we zoeken alleen in de 9 vakjes om de landingsplek
	const float HoopSamenvoegStraal = 60f;

	// elke seconde vergaat dit deel van elke hoop
	const float HoopVergaanPerSeconde = 0.01f;

	DrolHoopData[] drolHopen = new DrolHoopData[MaxDrolHopen];
	int momenteelDrolHopen;
	int volgendDrolHoopId;

	// hoeveel honger een drol wegneemt (minder dan schijten kost, anders maak je gratis energie), en hoe snel een strontvreter eet (honger per seconde)
	const float StrontVoedingPerDrol = 0.8f;
	const float StrontEetSnelheid = 10f;
	const float StrontvreterPlantenMalus = 0.1f;

	// zelfde soort grid als voor joekels en voedsel. elke tick opnieuw (na het vergaan), nieuwe hopen schuiven we er meteen in.
	int[] celEersteDrolHoop = CelGridLeeg();
	int[] volgendeDrolHoopInCel = new int[MaxDrolHopen];

	static int[] CelGridLeeg()
	{
		var cellen = new int[GridBreedte * GridHoogte];
		System.Array.Fill(cellen, -1);
		return cellen;
	}

	void VergaDrolHopen(float dt)
	{
		// achterstevoren voor de swap-remove
		for (int h = momenteelDrolHopen - 1; h >= 0; h--)
		{
			drolHopen[h].Hoeveelheid -= drolHopen[h].Hoeveelheid * HoopVergaanPerSeconde * dt;
			if (drolHopen[h].Hoeveelheid < 0.5f) drolHopen[h] = drolHopen[--momenteelDrolHopen];
		}

		System.Array.Fill(celEersteDrolHoop, -1);
		for (int h = 0; h < momenteelDrolHopen; h++)
		{
			int cel = CelVan(drolHopen[h].Position);
			volgendeDrolHoopInCel[h] = celEersteDrolHoop[cel];
			celEersteDrolHoop[cel] = h;
		}
	}

	// strontvreters die een hoop aanraken eten ervan.
	void EetDrolHopen(float dt)
	{
		for (int i = 0; i < momenteelLevendeJoekels; i++)
		{
			if (joekels[i].Strontvreterij <= 0.05f || joekels[i].HuidigeGezonheid <= 0) continue;
			int h = DichtsteDrolHoop(joekels[i].Position, joekels[i].Chonkheid + 2);
			if (h < 0) continue;
			float honger = Mathf.Min(StrontEetSnelheid * joekels[i].Strontvreterij * dt, joekels[i].Hunger - HungerMin);
			honger = Mathf.Min(honger, drolHopen[h].Hoeveelheid * StrontVoedingPerDrol);
			if (honger <= 0) continue;
			drolHopen[h].Hoeveelheid -= honger / StrontVoedingPerDrol;
			joekels[i].Hunger -= honger;
		}
	}

	// dichtste hoop binnen straal (gemeten tot de rand van de hoop). -1 = geen
	int DichtsteDrolHoop(Vector2 positie, float straal)
	{
		int beste = -1;
		float besteAfstand = float.MaxValue;
		int ringen = (int)Mathf.Ceil(straal / CelGrootte) + 1;
		int cel = CelVan(positie);
		int celX = cel % GridBreedte, celY = cel / GridBreedte;
		for (int y = celY - ringen; y <= celY + ringen; y++)
			for (int x = celX - ringen; x <= celX + ringen; x++)
			{
				if (x < 0 || y < 0 || x >= GridBreedte || y >= GridHoogte) continue;
				for (int h = celEersteDrolHoop[y * GridBreedte + x]; h >= 0; h = volgendeDrolHoopInCel[h])
				{
					if (drolHopen[h].Hoeveelheid < 0.5f) continue;
					float afstand = positie.DistanceTo(drolHopen[h].Position) - DrolStraal * Mathf.Sqrt(drolHopen[h].Hoeveelheid);
					if (afstand < straal && afstand < besteAfstand) { besteAfstand = afstand; beste = h; }
				}
			}
		return beste;
	}

	int IndexVanDrolHoop(int id, int gok = -1)
	{
		if (gok >= 0 && gok < momenteelDrolHopen && drolHopen[gok].Id == id) return gok;
		for (int h = 0; h < momenteelDrolHopen; h++)
			if (drolHopen[h].Id == id) return h;
		return -1;
	}

	void LandDrol(Vector2 positie)
	{
		// ligt er al een hoop vlakbij? check mijn vakje en de 8 eromheen
		int cel = CelVan(positie);
		int celX = cel % GridBreedte, celY = cel / GridBreedte;
		for (int y = celY - 1; y <= celY + 1; y++)
			for (int x = celX - 1; x <= celX + 1; x++)
			{
				if (x < 0 || y < 0 || x >= GridBreedte || y >= GridHoogte) continue;
				for (int h = celEersteDrolHoop[y * GridBreedte + x]; h >= 0; h = volgendeDrolHoopInCel[h])
				{
					if (drolHopen[h].Position.DistanceTo(positie) >= HoopSamenvoegStraal) continue;
					drolHopen[h].Hoeveelheid += 1;
					return;
				}
			}

		// als de kaart vol is weg met die drol.
		if (momenteelDrolHopen == MaxDrolHopen) return;
		drolHopen[momenteelDrolHopen] = new DrolHoopData { Id = volgendDrolHoopId++, Position = positie, Hoeveelheid = 1 };
		volgendeDrolHoopInCel[momenteelDrolHopen] = celEersteDrolHoop[cel];
		celEersteDrolHoop[cel] = momenteelDrolHopen;
		momenteelDrolHopen++;
	}
}
