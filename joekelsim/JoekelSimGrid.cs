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
}
