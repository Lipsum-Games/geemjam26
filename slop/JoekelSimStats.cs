using Godot;
using System.Collections.Generic;

// Slop: de snelle sim (joekelsim/) laat zich zien aan het stats-scherm. Zelfde class (partial), maar in de slop-map,
// zodat joekelsim/ zelf schoon blijft.
public partial class JoekelSim
{
	// alle levende joekels als lijst voor Statistiek/Clusters. Sleutel = Id (blijft hetzelfde, ook na swap-remove)
	public List<JoekelInfo> Infos()
	{
		var lijst = new List<JoekelInfo>(momenteelLevendeJoekels);
		for (int i = 0; i < momenteelLevendeJoekels; i++)
			lijst.Add(new JoekelInfo(joekels[i].Id, joekels[i].Dna, joekels[i].Position, Karaktereigenschappen.RanzigheidAuraGeur.Van(joekels[i].Dna)));
		return lijst;
	}

	// groep markeren (Groepen-tab): wie niet in de set zit wordt doorzichtig. null = iedereen weer normaal.
	// Werkt via de alpha van de kleurbanden; nieuwe joekels zijn standaard zichtbaar tot de volgende meting (elke 2 s).
	public void ZetMarkering(HashSet<object> zichtbaar)
	{
		for (int i = 0; i < momenteelLevendeJoekels; i++)
		{
			float a = zichtbaar == null || zichtbaar.Contains(joekels[i].Id) ? 1f : 0.15f;
			joekels[i].Kleur0.A = a;
			joekels[i].Kleur1.A = a;
			joekels[i].Kleur2.A = a;
		}
	}
}
