using Godot;

// Snelle joekel sim waar alle joekels in arrays zitten ipv eigen nodes
// elke joekel is eigenlijk een i in een index van een array (of column van ee matrix hoe je het wilt zien)
// Wordt stap voor stap opgebouwd.
public partial class JoekelSim : Node2D
{
    // de hoeveelheid starterjoekels
	[Export] public int StarterJoekels = 300;

    // dit is de array van joekelposities. i is dus de index van de joekel
	Vector2[] joekelPosities;

	public override void _Ready()
	{
		joekelPosities = new Vector2[StarterJoekels];
		for (int i = 0; i < StarterJoekels; i++)
		    // plaats die joekeltjes lekker random!
			joekelPosities[i] = new Vector2(GD.Randf() * World.WorldSize.X, GD.Randf() * World.WorldSize.Y);
	}
}
