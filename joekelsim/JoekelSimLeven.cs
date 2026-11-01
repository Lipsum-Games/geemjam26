using Godot;

// De hele joekel lifecycle, geboorte en dood wordt hier geregeld.
public partial class JoekelSim
{
	// Maak een nieuwe joekel op deze plek. Hij komt achteraan in de array (op plek momenteelLevendeJoekels).
	// Als de array vol is, dan gebeurt er niets.
	void MaakJoekel(Vector2 positie)
	{
		if (momenteelLevendeJoekels >= MaxJoekels) return;

		// struct op de plaats van de joekel
		joekels[momenteelLevendeJoekels] = new JoekelData
		{
			Positie = positie,
			// willekeurige start, anders springen ze allemaal in dezelfde tick
			SpringTimer = GD.Randf() * SpringInterval,
		};
		momenteelLevendeJoekels++;
	}
}
