using Godot;
using Geemjam26ShipsumEvolve.genetica;

// Behaviour: wat wil een joekel?
public partial class JoekelSim
{
    // Check om de zoveel ticks voor behaviour.
	const int CheckBehaviourEvery = 5;

    // Dit bepaald dus wat de joekel gaat doen komende tijd.
	(float richting, float kracht) DoeDingen(int i)
	{
		joekels[i].BehaviourCheckCounter--;
		if (joekels[i].BehaviourCheckCounter <= 0)
		{
			joekels[i].BehaviourCheckCounter = CheckBehaviourEvery;
			BesluitPlan(i);
		}

		// By default chillen, nu ff standaard klein sprongetje naar rechts.
		if (joekels[i].CurrentPlan == Behaviour.Plan.CHILL)
			return (0f, 0.1f);

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
			}
		}

		if (j.CurrentPlan == Behaviour.Plan.EAT)
		{
			j.TargetGok = IndexVanFoodSource(j.TargetId, j.TargetGok);
			if (j.TargetGok < 0 || foodSources[j.TargetGok].ResourceCount <= 0) StopPlan(ref j);
		}
	}

	// terug naar chillen, zonder doel
	static void StopPlan(ref JoekelData j)
	{
		j.CurrentPlan = Behaviour.Plan.CHILL;
		j.TargetPosition = null;
		j.TargetId = 0;
	}
}
