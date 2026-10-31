using Godot;
using static Activations;

public static class Karaktereigenschappen
{
	public static readonly (int, int) DNA_CELL_DOODGAANS_SNELHEID_EN_ALGEMENE_GEZONDHEID = (2, 2);

	// speed (oftewel kachow) is een sigmoid van de eerste rij over de range van 40-200 gestretched.
	public static readonly Karaktereigenschap Kachow = new("Kachow", d => Range(200, 1000, Sigmoid(d.Row(0))));

	// turn jitter is een some van de tweede rij * 2
	public static readonly Karaktereigenschap TurnJitter = new("TurnJitter", d => d.Row(1) * 2);

	// hoe goed dit joekeltje kan zwemmen (of misschien kan hij het niet)
	public static readonly Karaktereigenschap Zwemdiplomas = new("Zwemdiplomas", dna => {
		const float ZWEM_MAX = 3.0f; // je kan niet beter dan 3 floats zijn in zwemmen (elke 1f is een zwemdiploma).
		
		float aanleg = dna.Cells((2,1), (0,2), DNA_CELL_DOODGAANS_SNELHEID_EN_ALGEMENE_GEZONDHEID, (1, 1));
		float hoeveel_zwemdiplomas = Range(0, ZWEM_MAX, Sigmoid(aanleg));

		bool kanJeZwemmen = hoeveel_zwemdiplomas >= 1f; // je moet ten minste 1 zwemdiploma hebben om te kunnen zwemmen.

		return kanJeZwemmen ? hoeveel_zwemdiplomas : 0.0f;
	});

	// ja je moet toch ooit dood gaan?
	public static readonly Karaktereigenschap DoodgaansSnelheidEnAlgemeneGezondheid = new("DoodgaansSnelheidEnAlgemeneGezondheid", d => d.Cells(DNA_CELL_DOODGAANS_SNELHEID_EN_ALGEMENE_GEZONDHEID));

	// de chonkheid van de joeekel, hoe fatsig bente ei???
	public static readonly Karaktereigenschap Chonkheid = new("Chonkheid", d => Range(5, 15, d.Diag() / 3 - 0.5f * d[0,1]));

	// de murigheid bepaalt hoe de joekel met de muur interact
	public static readonly Karaktereigenschap Murigheid = new("Murigheid", d => d.Cells((0, 2), (1, 1), (2, 0), (0, 1)) / 4);

}
