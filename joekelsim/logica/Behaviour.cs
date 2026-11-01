using Godot;
using Geemjam26ShipsumEvolve.genetica;

// Behaviour: wat wil een joekel?
public partial class JoekelSim
{

    // Dit bepaald dus wat de joekel gaat doen komende tijd.
	(float richting, float kracht) DoeDingen(int i)
	{

		BesluitPlan(i);

		if (joekels[i].CurrentPlan == Behaviour.Plan.REPRODUCE)
		{
			joekels[i].TargetGok = IndexVanJoekel(joekels[i].TargetId, joekels[i].TargetGok);
			if (joekels[i].TargetGok >= 0)
			{
				joekels[i].TargetPosition = joekels[joekels[i].TargetGok].Position;
			}
			else
			{
				StopPlan(ref joekels[i]);
			}
		}

		// By default chillen, nu ff standaard klein sprongetje naar rechts.
		if (joekels[i].CurrentPlan == Behaviour.Plan.CHILL)
			return (joekels[i].Heading + (GD.Randf() - 0.5f) * 0.1f, joekels[i].ChillSpeed);

        // Als er een doel is, spring ernaartoe.
		if (joekels[i].TargetPosition is Vector2 doel)
		{
		    // Dichtbij springt hij zachter, zodat hij er niet voorbij schiet
			float afstand = doel.DistanceTo(joekels[i].Position);
			float maxVel = joekels[i].Kachow; // TIJDELIJK: main gebruikt GetVelocityVal (kachow x honger x tegel), komt later
			float kracht = afstand < maxVel ? Mathf.Clamp(0.5f * afstand / maxVel, 0, 1) : 1f;
			return (joekels[i].Position.AngleToPoint(doel), kracht);
		}
		return (0f, 0f);
	}

	// Voor nu nog leeg.
	void BesluitPlan(int i)
	{
		ref JoekelData j = ref joekels[i];

		// Kijk eerst of we in een huidig plan zitten
		if (j.CurrentPlan == Behaviour.Plan.EAT || (j.CurrentPlan != Behaviour.Plan.CHILL && j.Hunger < j.Trek))
		{
			// No need to act, only handle "end" conditions
			if (j.CurrentPlan == Behaviour.Plan.EAT && j.Hunger < HungerMin * 1.1f)
			{
				StopPlan(ref j);
			} else if (j.CurrentPlan == Behaviour.Plan.RUN_AWAY)
			{
				if (j.RunAwayTicks > 0)
				{
					j.RunAwayTicks -= 1;
				}
				else
				{
					j.CurrentPlan = Behaviour.Plan.CHILL;
					j.TargetPosition = null;
				}
			}
		}
		else
		{
			// Plan the next move

			// Prio 1: Voedsel is heeeeeeel belangrijk
			if (j.CurrentPlan != Behaviour.Plan.EAT && j.Hunger > j.Trek)
			{
				int f = DichtsteFoodSource(i, j.Perception);

				if (f >= 0)
				{
					// Go to foodsource
					j.TargetPosition = foodSources[f].Position;
					j.TargetId = foodSources[f].Id;
					j.TargetGok = f;
					j.CurrentPlan = Behaviour.Plan.EAT;
				}
				else
				{
					// TODO: New "looking for food" behaviour?
				}
			} else if (j.CurrentPlan == Behaviour.Plan.CHILL)
			{
				// We doen niks, laten we wat gaan den

				(float drukteIndex, Vector2 target) = _compute_introversie_values(i);

				// Is het te druk?
				if (drukteIndex > GD.Randf())
				{
					j.CurrentPlan = Behaviour.Plan.RUN_AWAY;
					j.TargetPosition = target;
					j.RunAwayTicks = 3;
				}
				else if (j.Oehlalalaigheid > GD.Randf())
				{
					// Maatje vinden
					int mate = ClosestJoekel(i);
					if (mate >= 0)
					{
						j.CurrentPlan = Behaviour.Plan.REPRODUCE;
						j.TargetPosition = joekels[mate].Position;
						j.TargetId = joekels[mate].Id;
						j.TargetGok = mate;
					}
				}
			}
		}

		if (j.CurrentPlan == Behaviour.Plan.EAT)
		{
			j.TargetGok = IndexVanFoodSource(j.TargetId, j.TargetGok);
			if (j.TargetGok < 0 || foodSources[j.TargetGok].ResourceCount <= 0) StopPlan(ref j);
		}
		if (j.CurrentPlan == Behaviour.Plan.REPRODUCE)
		{
			j.TargetGok = IndexVanJoekel(j.TargetId, j.TargetGok);
			if (j.TargetGok < 0) StopPlan(ref j);
		}
	}

	private (float drukteIndex, Vector2 target) _compute_introversie_values(int i)
	{
		// pak de joekels om ons heen
		int aantal = BurenVan(i, joekels[i].Perception + Karaktereigenschappen.MaxChonkheid);
		int count = 1;
		Vector2 som = joekels[i].Position;
		for (int k = 0; k < aantal; k++)
		{
			int b = burenBuffer[k];
			if (joekels[i].Position.DistanceTo(joekels[b].Position) >= joekels[i].Perception + joekels[b].Chonkheid) continue;
			som += joekels[b].Position;
			count++;
		}

		if (count <= 1)
		{
			return (0, Vector2.Zero);
		}

		// Hoe "erg" drukte is
		float drukteIndex;
		Vector2 target;

		Vector2 avgLoc = som / count;

		if (joekels[i].Introversie > 0)
		{
			// Kans op wegbewegen, schaalt met #joekels introversie
			drukteIndex = 0.5f * (count - 1.0f) * joekels[i].Introversie;

			Vector2 relPos = (avgLoc - joekels[i].Position);

			if (relPos.IsZeroApprox())
			{
				return (0, Vector2.Zero);
			}

			target = -relPos * 1_000;
		}
		else
		{
			// Kans op drukte opzoeken, meer extraversie en minder joekels maken de kans hoger om andere joekels te zoeken
			drukteIndex = Mathf.Abs(joekels[i].Introversie) / count;

			target = avgLoc;
		}

		return (drukteIndex, target);
	}

	// terug naar chillen, zonder doel
	static void StopPlan(ref JoekelData j)
	{
		j.CurrentPlan = Behaviour.Plan.CHILL;
		j.TargetPosition = null;
		j.TargetId = 0;
	}
}
