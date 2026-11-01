using Godot;

// Snelle joekel sim waar alle joekels in arrays zitten ipv eigen nodes
// elke joekel is eigenlijk een i in een index van een array (of column van ee matrix hoe je het wilt zien)
// Wordt stap voor stap opgebouwd.
// Deze class is partial, want niet de hele sim staat erin. hier gewoon de data en joekelsimtekencode bijvoorbeeld alle teken en mesh dingen.
public partial class JoekelSim : Node2D
{
	// kennen egt niet meer dan zoveel joekels zijn
	const int MaxJoekels = 4096;

	// de hoeveelheid starterjoekels
	public int StarterJoekels = 300;

	// de huidige hoeveelheid levende joekels
	int momenteelLevendeJoekels;

	// alle joekels: joekels[i] is joekel i. Wat een joekel is staat in JoekelData.cs, lekker centralized
	JoekelData[] joekels = new JoekelData[MaxJoekels];

	// FoodSources werken precies zoals joekels: een array van structs en een teller
	const int MaxFoodSources = 4096;
	public int StarterFoodSources = 50;
	int momenteelFoodSources;

	// alle food sources, net als joekels data array hierboven.
	FoodSourceData[] foodSources = new FoodSourceData[MaxFoodSources];

	public override void _Ready()
	{
		for (int i = 0; i < StarterJoekels; i++)
			// plaats die joekeltjes lekker random!
			MaakJoekel(new Vector2(GD.Randf() * World.WorldSize.X, GD.Randf() * World.WorldSize.Y), Dna.Random());
		for (int f = 0; f < StarterFoodSources; f++)
		    // en ook maar lekker random dat eten plaatsen!
			SpawnFoodSource(new Vector2(GD.Randf() * World.WorldSize.X, GD.Randf() * World.WorldSize.Y));

        // teken food eerst zodat joekels eroverheen getekent worden
		MaakFoodSourceTekening();
		MaakJoekelTekening();
		TekenJoekels();
	}

	public override void _PhysicsProcess(double delta)
	{
		RuimDodenOp();
		// botsen gebruikt BurenVan, en na RuimDodenOp kloppen de indexen in het oude grid niet meer, dus we doen dit nu ff 2x (kan ook 1x maybe maar voor nu is het fine)
		BouwJoekelGrid();
		UpdateVoedselTimer((float)delta);
		BeweegJoekels((float)delta);
		// het grid moet wel elke tick geupdate worden. is snel hoor
		BouwJoekelGrid();
		// voedsel grid moet na de timer maar wel voor het eten anders kunnen we nabiijheid niet checken
		BouwFoodSourceGrid();
		// dit moet na BouwJoekelGrid: gebruikt BurenVan (vandaar)
		Contact((float)delta);
		CalculateHunger((float)delta);
		RuimLegeFoodSourcesOp();
	}

	public override void _Process(double delta)
	{
		TekenJoekels();
		TekenFoodSources();
	}
}
