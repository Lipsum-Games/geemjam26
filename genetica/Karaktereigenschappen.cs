using Godot;
using static Activations;

public static class Karaktereigenschappen
{
	public static readonly (int, int) DNA_CELL_DOODGAANS_SNELHEID_EN_ALGEMENE_GEZONDHEID = (2, 2);

	// speed (oftewel kachow) is een sigmoid van de eerste rij over de range van 40-200 gestretched.
	public static readonly Karaktereigenschap Kachow = new("Kachow", d => Range(40, 200, Sigmoid(d.Row(0))));

	// turn jitter is dikke cirkel wiskunde weettewel
	public static readonly Karaktereigenschap TurnJitter = new("TurnJitter", d => Range(0, Mathf.Pi / 2, Sigmoid(d.Row(1))));
	
	// Hoen hekel Joekels hebben aan andere Joekels
	public static readonly Karaktereigenschap Introversie = new("Introversie", d => Sigmoid(d.Col(2) * 2 - 2 * d.AntiDiag()));
	
	// Hoe snel de joekels besluiten dat de honger teveel is en ze moeten vreten
	public static readonly Karaktereigenschap Trek = new("Trek", d => d.Col(2) * 2 - 2 * d.AntiDiag());

	// Hoe enthousiast the joekels zijn om, uhhh, "gezellige" dingen te doen die mogelijk kunnen leiden tot de creatie van nageslacht
	public static readonly Karaktereigenschap Oehlalalaigheid = new("Oehlalalaigheid", d => Sigmoid(1.0f * d.Col(0) - 0.5f * (d.Col(1) + d.Col(2))));

	// hoe graag willen ze vlees eten
	public static readonly Karaktereigenschap Vleeslustigheid = new("Vleeslustigheid", d => Mathf.Min(1, Mathf.Max(-1, d.AntiDiag() / 3f)));

	// hoe goed dit joekeltje kan zwemmen (of misschien kan hij het niet)
	public static readonly Karaktereigenschap Zwemdiplomas = new("Zwemdiplomas", dna => {
		const float ZWEM_MAX = 3.0f; // je kan niet beter dan 3 floats zijn in zwemmen (elke 1f is een zwemdiploma).
		
		// range [-4, 4]
		float aanleg = dna.Cells((2,1), (0,2), DNA_CELL_DOODGAANS_SNELHEID_EN_ALGEMENE_GEZONDHEID, (1, 1));
		float aanlegNormaal = (aanleg + 4.0f) / 8.0f;

		float hoeveel_zwemdiplomas = aanlegNormaal * ZWEM_MAX;

		bool kanJeZwemmen = hoeveel_zwemdiplomas >= 1f; // je moet ten minste 1 zwemdiploma hebben om te kunnen zwemmen.

		return kanJeZwemmen ? hoeveel_zwemdiplomas : 0.0f;
	});

	// ja je moet toch ooit dood gaan?
	public static readonly Karaktereigenschap DoodgaansSnelheidEnAlgemeneGezondheid = new("DoodgaansSnelheidEnAlgemeneGezondheid", d => 
	Range(20, 100, Sigmoid(d.Cells(DNA_CELL_DOODGAANS_SNELHEID_EN_ALGEMENE_GEZONDHEID) * 3)));

	// de chonkheid van de joeekel, hoe fatsig bente ei???
	public static readonly Karaktereigenschap Chonkheid = new("Chonkheid", d => Range(5, 15, Sigmoid(d.Diag() - 0.5f * d[0, 1])));

	// de murigheid bepaalt hoe de joekel met de muur interact
	public static readonly Karaktereigenschap Murigheid = new("Murigheid", d => d.Cells((0, 2), (1, 1), (2, 0), (0, 1)) / 4);

	// RANZIG???? OUTTA HEREEEE
	public static readonly Karaktereigenschap RanzigheidAuraBereik = new("RanzigheidAuraBereik", d => Range(0, 150, Sigmoid((d[1, 0] + d[2, 1]) / 2 * 3)));
	public static readonly Karaktereigenschap RanzigheidAuraKracht = new("RanzigheidAuraKracht", d => Relu(d[0, 2]) * 8);
	public static readonly Karaktereigenschap RanzigheidAuraGeur = new("RanzigheidAuraGeur", d => d[1, 1]);

}
