using Godot;
using System;
using System.Collections.Generic;

public partial class Karaktereigenschap
{
	public static readonly List<Karaktereigenschap> Alle = new();
	
	public readonly string Naam;
	readonly Func<Dna, float> _expressie;


	public Karaktereigenschap(string naam, Func<Dna, float> expressie)
	{
		Naam = naam;
		_expressie = expressie;
		Alle.Add(this);
	}

	public float Van(Dna d) => _expressie(d);
}
