using Geemjam26ShipsumEvolve.neuralnets;
using Godot;

// Behaviour: wat wil een joekel?
public partial class JoekelSim
{

    // Dit bepaald dus wat de joekel gaat doen komende tijd.
	(float richting, float kracht) DoeDingen(int i, float dt)
	{
		if (joekels[i].NeuralNet != null && GD.Randf() < joekels[i].NetControl)
		{
			return DoeNeuralNetDingen(i, dt);
		}

		BesluitPlan(i, dt);

		if (joekels[i].CurrentPlan == Plan.REPRODUCE)
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

		if (joekels[i].CurrentPlan == Plan.CHILL)
		{
			float headingNew = joekels[i].Heading + (GD.Randf() - 0.5f) * 0.1f;
			Vector2 checkVec = joekels[i].Position  + Vector2.FromAngle(headingNew) * joekels[i].Kachow;
			float r = joekels[i].Chonkheid;
			if (checkVec.Clamp(new Vector2(r, r), World.WorldSize - new Vector2(r, r)) != checkVec)
			{
				// Als we de wereld uit dreigen te gaan, draai naar het midden
				headingNew = joekels[i].Position.AngleToPoint(World.WorldSize / 2);
			}


			return (headingNew, joekels[i].ChillSpeed);
		}

		// Als er een doel is, spring ernaartoe.
		if (joekels[i].TargetPosition is Vector2 doel)
		{
		    // Dichtbij springt hij zachter, zodat hij er niet voorbij schiet
			float afstand = doel.DistanceTo(joekels[i].Position);
			float maxVel = GetVelocityVal(i);
			float kracht = afstand < maxVel ? Mathf.Clamp(1.0f * afstand / maxVel, 0, 1) : 1f;
			return (joekels[i].Position.AngleToPoint(doel), kracht);
		}
		return (0f, 0f);
	}

	private (float richting, float kracht) DoeNeuralNetDingen(int i, float dt)
	{
		JoekelData joekel = joekels[i];
		int closestJoekel = ClosestJoekel(i);
		int prooi = BesteProoi(i, joekel);
		int f = DichtsteFoodSource(i, joekel.Perception);
		
		var data = new SimpleNetInputData()
		{
			WorldPosition	= joekel.Position,
			Velocity        = joekel.Velocity,
			Temperature     = 0.0f, // TODO: Waar de **** komt dit nou weet vandaan? 
			Heading         = joekel.Heading, 
			Hunger          = joekel.Hunger, 
			
			BestMate        = closestJoekel > 0 ? joekels[closestJoekel] : null,
			BestPrey        = prooi > 0 ? joekels[prooi] : null,
			BestFood        = f > 0 ? foodSources[f] : null,
			
		};
		(float rd, float kracht) = joekels[i].NeuralNet.Process(data);
		return (joekels[i].Heading + rd * dt, kracht);
	}

	// Voor nu nog leeg.
	void BesluitPlan(int i, float dt)
	{
		ref JoekelData j = ref joekels[i];

		// Kijk eerst of we in een huidig plan zitten
		if (j.CurrentPlan is Plan.EAT or Plan.HUNT or Plan.EAT_STRONT || (j.CurrentPlan != Plan.CHILL && j.Hunger < j.Trek))
		{
			// No need to act, only handle "end" conditions
			if (j.CurrentPlan is Plan.EAT or Plan.HUNT or Plan.EAT_STRONT && j.Hunger < HungerMin * 1.1f)
			{
				StopPlan(ref j);
			} else if (j.CurrentPlan == Plan.RUN_AWAY)
			{
				if (j.RunAwaySeconds > 0)
				{
					j.RunAwaySeconds -= dt;
				}
				else
				{
					j.CurrentPlan = Plan.CHILL;
					j.TargetPosition = null;
				}
			}
		}
		else
		{
			// Plan the next move

			// Prio 1: Voedsel is heeeeeeel belangrijk
			if (j.CurrentPlan is not (Plan.EAT or Plan.HUNT or Plan.EAT_STRONT) && j.Hunger > j.Trek)
			{
				if (j.Vleeslustigheid + 0.2f * (GD.Randf() - 0.5) > JaagDrempel)
				{
					// prooi vinden
					int prooi = BesteProoi(i, j);
					if (prooi >= 0)
					{
						j.CurrentPlan = Plan.HUNT;
						j.TargetPosition = joekels[prooi].Position;
						j.TargetId = joekels[prooi].Id;
						j.TargetGok = prooi;
					}
				}
				else
				{
					int f = DichtsteFoodSource(i, j.Perception);
					// strontvreters kijken ook naar drolhopen, en gaan naar wat het dichtst is
					int h = j.Strontvreterij > StrontvreterDrempel ? DichtsteDrolHoop(j.Position, j.Perception) : -1;
					if (h >= 0 && (f < 0 || j.Position.DistanceTo(drolHopen[h].Position) < j.Position.DistanceTo(foodSources[f].Position)))
					{
						j.TargetPosition = drolHopen[h].Position;
						j.TargetId = drolHopen[h].Id;
						j.TargetGok = h;
						j.CurrentPlan = Plan.EAT_STRONT;
					}
					else if (f >= 0)
					{
						// Go to foodsource
						j.TargetPosition = foodSources[f].Position;
						j.TargetId = foodSources[f].Id;
						j.TargetGok = f;
						j.CurrentPlan = Plan.EAT;
					}
				}
			} else if (j.CurrentPlan == Plan.CHILL)
			{
				// We doen niks, laten we wat gaan den

				(float drukteIndex, Vector2 target) = _compute_introversie_values(i);

				// Is het te druk?
				if (drukteIndex > GD.Randf())
				{
					j.CurrentPlan = Plan.RUN_AWAY;
					j.TargetPosition = target;
					j.RunAwaySeconds = j.IntroversieWegrenTijd;
				}
				else if (j.Oehlalalaigheid > GD.Randf())
				{
					// Maatje vinden
					int mate = ClosestJoekel(i);
					if (mate >= 0)
					{
						j.CurrentPlan = Plan.REPRODUCE;
						j.TargetPosition = joekels[mate].Position;
						j.TargetId = joekels[mate].Id;
						j.TargetGok = mate;
					}
				}
			}
		}

		if (j.CurrentPlan == Plan.EAT)
		{
			j.TargetGok = IndexVanFoodSource(j.TargetId, j.TargetGok);
			if (j.TargetGok < 0 || foodSources[j.TargetGok].ResourceCount <= 0) StopPlan(ref j);
		}
		if (j.CurrentPlan == Plan.EAT_STRONT)
		{
			j.TargetGok = IndexVanDrolHoop(j.TargetId, j.TargetGok);
			if (j.TargetGok < 0) StopPlan(ref j);
		}
		if (j.CurrentPlan is Plan.REPRODUCE or Plan.HUNT)
		{
			j.TargetGok = IndexVanJoekel(j.TargetId, j.TargetGok);
			if (j.TargetGok < 0) StopPlan(ref j);
		}
	}
	private int BesteProoi(int i, JoekelData joekelData)
	{
		int buren = BurenVan(i, joekelData.Perception);
		if (buren == 0)
		{
			return -1;
		}

		JoekelData? prooi = null;
		int? joekelIndex = null;
		float? bestScore = null;

		for (int bi = 0; bi < buren; bi++)
		{
			int joi = burenBuffer[bi];
			JoekelData jo = joekels[joi];
			if (jo.Vleeslustigheid <= -0.0)
			{
				float jscore = jo.Position.DistanceTo(joekelData.Position);
				if (bestScore is null || jscore < bestScore.Value)
				{
					bestScore = jscore;
					prooi = jo;
					joekelIndex = joi;
				}
			}
		}

		if (prooi is not null)
		{
			return joekelIndex.Value;
		}
		return -1;
	}

	private (float drukteIndex, Vector2 target) _compute_introversie_values(int i)
	{
		// pak de joekels om ons heen
		int aantal = BurenVan(i, joekels[i].Perception + grootsteChonkheid);
		int count = 0;
		Vector2 som = Vector2.Zero;
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
			
			// Check ff of we niet bijna de wereld uit stormen (of in de buurt komen)
			Vector2 checkVec = joekels[i].Position - relPos * joekels[i].Kachow * 0.1f;
			float r = joekels[i].Chonkheid;
			if (checkVec.Clamp(new Vector2(r, r), World.WorldSize - new Vector2(r, r)) != checkVec)
			{
				// Als we de wereld uit zouden raken, doe het dan niet, en beweeg richting het midden
				return (drukteIndex, (World.WorldSize/2 - joekels[i].Position));	
			}

			return (drukteIndex, -relPos * 1_000);
		}
		else
		{
			// Kans op drukte opzoeken, meer extraversie en minder joekels maken de kans hoger om andere joekels te zoeken
			drukteIndex = Mathf.Abs(joekels[i].Introversie) / count;

			return (drukteIndex, avgLoc);
		}
	}

	// terug naar chillen, zonder doel
	// pas vanaf deze vleeslustigheid gaat een joekel jagen
	const float JaagDrempel = 0.3f;

	// vanaf deze strontvreterij gaat een joekel ook naar drolhopen toe
	const float StrontvreterDrempel = 0.3f;

	static void StopPlan(ref JoekelData j)
	{
		j.CurrentPlan = Plan.CHILL;
		j.TargetPosition = null;
		j.TargetId = 0;
	}
}
