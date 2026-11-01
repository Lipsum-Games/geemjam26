using Godot;

// Alles wat een joekel is basically.. joekels[i] in JoekelSim is deze struct.
// Nieuwe eigenschap voor een joekel? HIERRR toevoegen.
public struct JoekelData
{
	// dit is de array van joekelposities. i is dus de index van de joekel
	public Vector2 Position;

	// snelheid van elke joekel in pixels per seconde voor de [x, y] axis.
	public Vector2 Velocity;

	// hoeveel seconden tot de volgende sprong van elke joekel
	public float SpringTimer;

	// het DNA (3x3 matrix van genen).
	public Dna Dna;

	// sprongkracht in px/s
	public float Kachow;

	// straal in px. Voor het tekenen en de muren (obviously)
	public float Chonkheid;

	// 1 (vol) tot 10 (uitgehongerd)
	public float Hunger;

	// op 0 gaat hij dik DOOD
	public float HuidigeGezonheid;
	public float MaximaleGezondheid;

	// trait uit het DNA: [-1..1]: boven de 0.5 eet hij geen planten meer
	public float Vleeslustigheid;
}
