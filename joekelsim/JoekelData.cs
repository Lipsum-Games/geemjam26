using Godot;

// Alles wat een joekel is basically.. joekels[i] in JoekelSim is deze struct.
// Nieuwe eigenschap voor een joekel? HIERRR toevoegen.
public struct JoekelData
{
	// dit is de array van joekelposities. i is dus de index van de joekel
	public Vector2 Positie;

	// snelheid van elke joekel in pixels per seconde voor de [x, y] axis.
	public Vector2 Snelheid;

	// hoeveel seconden tot de volgende sprong van elke joekel
	public float SpringTimer;

	// het DNA (3x3 matrix van genen).
	public Dna Dna;

	// sprongkracht in px/s
	public float Kachow;

	// straal in px. Voor het tekenen en de muren (obviously)
	public float Chonkheid;
}
