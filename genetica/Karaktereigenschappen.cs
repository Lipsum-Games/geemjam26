using Godot;
using static Activations;

public static class Karaktereigenschappen
{
	// speed (oftewel kachow) is een sigmoid van de eerste rij over de range van 40-200 gestretched.
	public static readonly Karaktereigenschap Kachow = new("Kachow", d => Range(40, 200, Sigmoid(d.Row(0))));

	// turn jitter is een some van de tweede rij * 2
	public static readonly Karaktereigenschap TurnJitter = new("TurnJitter", d => d.Row(1) * 2);
	
	// Hoen hekel Joekels hebben aan andere Joekels
	public static readonly Karaktereigenschap Introversie = new("Introversie", d => Sigmoid(d.Col(2) * 2 - 2 * d.AntiDiag()));
	
	// Hoe snel de joekels besluiten dat de honger teveel is en ze moeten vreten
	public static readonly Karaktereigenschap Trek = new("Trek", d => d.Col(2) * 2 - 2 * d.AntiDiag());

	// Hoe enthousiast the joekels zijn om, uhhh, "gezellige" dingen te doen die mogelijk kunnen leiden tot de creatie van nageslacht
	public static readonly Karaktereigenschap Oehlalalaigheid = new("Oehlalalaigheid", d => Sigmoid(1.0f * d.Col(0) - 0.5f * (d.Col(1) + d.Col(2))));
}
