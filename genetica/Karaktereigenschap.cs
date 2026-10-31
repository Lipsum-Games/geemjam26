using Godot;
using System;

public partial class Karaktereigenschap
{
	
	public readonly string Naam;
	readonly Func<Dna, float> _expressie;


	public Karaktereigenschap(string naam, Func<Dna, float> expressie)
	{
		Naam = naam;
		_expressie = expressie;
	}

	public float Van(Dna d) => _expressie(d);
}
