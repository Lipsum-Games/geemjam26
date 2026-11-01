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

	// dit is de array van joekelposities. i is dus de index van de joekel
	Vector2[] joekelPosities = new Vector2[MaxJoekels];

	// snelheid van elke joekel in pixels per seconde voor de [x, y] axis.
	Vector2[] joekelSnelheden = new Vector2[MaxJoekels];

	// hoeveel seconden tot de volgende sprong van elke joekel
	float[] joekelSpringTimers = new float[MaxJoekels];

	public override void _Ready()
	{
		for (int i = 0; i < StarterJoekels; i++)
			// plaats die joekeltjes lekker random!
			joekelPosities[i] = new Vector2(GD.Randf() * World.WorldSize.X, GD.Randf() * World.WorldSize.Y);
		momenteelLevendeJoekels = StarterJoekels;

		// zet de timers voor de joekel sprongetjes
		ZetStartSpringTimers();

		MaakJoekelTekening();
		TekenJoekels();
	}

	public override void _PhysicsProcess(double delta)
	{
		BeweegJoekels((float)delta);
	}

	public override void _Process(double delta)
	{
		TekenJoekels();
	}
}
