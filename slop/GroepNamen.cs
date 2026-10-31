using System;

// OpenTTD-achtige Nederlandse plaatsnamen voor groepen: voorvoegsel + stam + achtervoegsel.
// Vast per (modus, groepnummer): de naam volgt uit het nummer, dus blijft gelijk over tijd en na herberekenen.
public static class GroepNamen
{
	static readonly string[] Voor = { "", "", "", "Groot-", "Klein-", "Oud-", "Nieuw-", "Sint-", "Ter ", "Boven-", "Neder-", "Oost-", "West-" };
	static readonly string[] Stam =
	{
		"Joekel", "Henkie", "Kachow", "Snolle", "Ranzig", "Stuiter", "Fumo", "Garfield", "Gondola", "Kei",
		"Mist", "Plons", "Truus", "Kees", "Bertje", "Dikke", "Blubber", "Geurt", "Chonk", "Zwemmer",
	};
	static readonly string[] Na = { "dorp", "hoven", "broek", "veen", "drecht", "woude", "sum", "ingen", "kerk", "zijl", "wijk", "horst", "rade", "burg", "meer" };

	public static string Naam(int id, Clusters.Modus modus)
	{
		var rng = new Random(id * 7919 + (int)modus * 104729 + 17);
		return Voor[rng.Next(Voor.Length)] + Stam[rng.Next(Stam.Length)] + Na[rng.Next(Na.Length)];
	}
}
