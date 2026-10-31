using Godot;
using static Activations;

public static class Karaktereigenschappen
{
	// speed (oftewel kachow) is een sigmoid van de eerste rij over de range van 40-200 gestretched.
	public static readonly Karaktereigenschap Kachow = new("Kachow", d => Range(40, 200, Sigmoid(d.Row(0))));

	// turn jitter is een some van de tweede rij * 2
	public static readonly Karaktereigenschap TurnJitter = new("TurnJitter", d => d.Row(1) * 2);
}
