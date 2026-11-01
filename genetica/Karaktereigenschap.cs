using Godot;
using System;
using System.Collections.Generic;

public partial class Karaktereigenschap
{
	public static readonly List<Karaktereigenschap> Alle = new();
	
	public readonly string Naam;
	// de formule zoals hij in de code staat (vult de compiler zelf in, voor het stats-scherm)
	public readonly string Formule;
	readonly Func<Dna, float> _expressie;


	public Karaktereigenschap(string naam, Func<Dna, float> expressie, [System.Runtime.CompilerServices.CallerArgumentExpression("expressie")] string formule = "")
	{
		Naam = naam;
		Formule = formule;
		_expressie = expressie;
		Alle.Add(this);
	}

	public float Van(Dna d) => _expressie(d);
}
