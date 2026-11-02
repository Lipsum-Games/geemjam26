using Godot;
using static Activations;

public static class Karaktereigenschappen
{
	public static readonly (int, int) DNA_CELL_DOODGAANS_SNELHEID_EN_ALGEMENE_GEZONDHEID = (2, 2);

	// speed (oftewel kachow) is een sigmoid van de eerste rij over de range van 40-100 gestretched.
	public static readonly Karaktereigenschap Kachow = new("Kachow", d => Mathf.Clamp(40f + 60f * Softplus(d.Row(0)), 40f, 100f));
	
	// Hoen hekel Joekels hebben aan andere Joekels
	public static readonly Karaktereigenschap Introversie = new("Introversie", d => d.Row(2) * 2 - 2 * d.Col(1));
	
	// How lang we aan het wegrennen zijn, in seconden
	public static readonly Karaktereigenschap IntroversieWegrenTijd = new("IntroversieWegrenTijd", d => Range(1f, 5f,Sigmoid(d[2,2] - d[0,0] + d[1,2] - d[2,1] - 2.0f)));
	
	// Hoe snel de joekels besluiten dat de honger teveel is en ze moeten vreten
	public static readonly Karaktereigenschap Trek = new("Trek", d => Range(2f, 10f, Sigmoid(d.Col(2) * 2.0f - 2.0f * d.AntiDiag())));

	// Hoe enthousiast the joekels zijn om, uhhh, "gezellige" dingen te doen die mogelijk kunnen leiden tot de creatie van nageslacht
	public static readonly Karaktereigenschap Oehlalalaigheid = new("Oehlalalaigheid", d => Sigmoid(1.0f * d.Col(0) - 0.5f * (d.Col(1) + d.Col(2))));

	// hoe graag willen ze vlees eten
	public static readonly Karaktereigenschap Vleeslustigheid = new("Vleeslustigheid", d => Tanh(d.AntiDiag() - 0.2f * d[0,0] - 0.5f)); // Tanh(d[1,1] - 0.5f));

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
	public const float MinChonkheid = 5f;
	public const float MaxChonkheid = 30f;
	public static readonly Karaktereigenschap Chonkheid = new("Chonkheid", d => Mathf.Min(MinChonkheid + 6f * Softplus(d.Diag() - 0.5f * d[0, 1]), MaxChonkheid));

	// de murigheid bepaalt hoe de joekel met de muur interact
	public static readonly Karaktereigenschap Murigheid = new("Murigheid", d => d.Cells((0, 2), (1, 1), (2, 0), (0, 1)) / 4);

	// RANZIG???? OUTTA HEREEEE
	public static readonly Karaktereigenschap RanzigheidAuraBereik = new("RanzigheidAuraBereik", d => Range(0, 150, Sigmoid((d[1, 0] + d[2, 1]) / 2 * 3)));
	public static readonly Karaktereigenschap RanzigheidAuraKracht = new("RanzigheidAuraKracht", d => Relu(d[0, 2]) * 8);
	public static readonly Karaktereigenschap RanzigheidAuraGeur = new("RanzigheidAuraGeur", d => d[1, 1]);

	public static readonly Karaktereigenschap Perception = new("Perception", d => Range(100, 300, Sigmoid(-2.5f * d[1, 2] + 0.3f * d[0, 0] - 1.2f * d[2, 0])));
	
	public static readonly Karaktereigenschap ChillSpeed = new("ChillSpeed", d =>  Range(0.1f, 1,Sigmoid(d.Col(2) - 3 * d[1,1])));

	// de aura ademt. pauze is tijd tot pulse van pulseduur.
	public static readonly Karaktereigenschap AuraPauze = new("AuraPauze", d => Range(0.5f, 6f, Sigmoid(d[0, 2] - 1.2f * d[2, 0] + 0.5f * d[1, 0])));
	public static readonly Karaktereigenschap AuraPulsDuur = new("AuraPulsDuur", d => Range(0.3f, 3f, Sigmoid(1.5f * d[1, 2] - d[0, 1] + 0.5f * d[2, 2])));

	// hoe scheef een joekel springt (radialen, de spreiding van de fout op elke sprong).
	public static readonly Karaktereigenschap Drunkness = new("Drunkness", d => 0.1f * Relu(0.8f * (d[0, 0] + d[0, 2]) - 0.7f * d[2, 1] + 0.4f * d[1, 2] + 1.5f) * Relu(0.8f * (d[0, 0] + d[0, 2]) - 0.7f * d[2, 1] + 0.4f * d[1, 2] + 1.5f));

	// drollen per seconde naar achter. zelfde input als Chonkheid (+ een eigen gen), dus vooral dikke joekels schijten
	public static readonly Karaktereigenschap Schijtlust = new("Schijtlust", d => Relu(d.Diag() - 0.5f * d[0, 1] + 0.7f * d[1, 0]));

	// 0-1: vacht
	public static readonly Karaktereigenschap Vacht = new("Vacht", d => Sigmoid(1.5f * (d[0, 2] + d[2, 0] - d[0, 0] - d[0, 1]) - 0.5f));

	// 0-1: hoeveel je van stront houdt. tegen de diagonaal in (dikke schutters zijn het zelden) + en een bonus gen
	public static readonly Karaktereigenschap Strontvreterij = new("Strontvreterij", d => Sigmoid(2.5f * d[2, 0] - d.Diag() - 1.5f));


}
