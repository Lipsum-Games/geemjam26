using Godot;

// Alles wat een joekel is basically.. joekels[i] in JoekelSim is deze struct.
// Nieuwe eigenschap voor een joekel? HIERRR toevoegen.
public struct JoekelData
{
	// unique id that never changes
    public int Id;

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

	// contact wordt elke 4 ticks gemeten, vandaar deze variabelen.
	public int ContactTeller;
	public float ContactTijd;

	// De behaviour fields.
	public Plan CurrentPlan;
	public Vector2? TargetPosition;
	// id van voedsel of joekel van de target
	public int TargetId;
	// waar het doel vorige keer stond voor fast lookup.
	public int TargetGok;

	// hoe ver zien (px)
	public float Perception;
	// wanneer honger (1..10)
	public float Trek;
	public float Introversie;
	public float Oehlalalaigheid;
	public int RunAwayTicks;
	public float Heading;    // richting van de laatste sprong
	public float ChillSpeed;

	// hoe oud in seconden
	public float Age;

	// de 3 kleurbanden, DNA-rij r als kleur
	public Color Kleur0, Kleur1, Kleur2;

	// kente ei wel zwemmen?? hmmMM^^^??!?!?!!
	public float Zwemdiplomas;

	// geurtje (aura): hoe ver, hoe hard, welke geur, en tijd sinds de laatste aura-tick
	public float RanzigBereik;
	public float RanzigKracht;
	public float Geur;
	public float AuraTijd;
}
