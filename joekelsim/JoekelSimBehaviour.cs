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
	}
}
