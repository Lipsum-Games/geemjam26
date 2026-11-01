using Godot;

// Slop: een joekel in de snelle sim aanklikken en zijn info in het DNA-paneel tonen (= Joekel.Selecteer/Toon in de oude game).
// We onthouden het Id (niet de index), want na een swap-remove staat er een andere joekel op die plek.
public partial class JoekelSim
{
	int geselecteerdId;        // 0 = niemand geselecteerd
	int geselecteerdGok = -1;  // waar hij vorige keer stond (voor IndexVanJoekel)
	readonly Node2D volgpunt = new(); // de camera volgt dit punt; staat elke frame op de geselecteerde joekel

	public Node2D Volgpunt => geselecteerdId == 0 ? null : volgpunt;

	// klik op wereldpositie p: de joekel waar je op klikt (dichtstbijzijnde als ze overlappen). false = niks geraakt
	public bool Kies(Vector2 p)
	{
		int beste = -1;
		float besteAfstand = float.MaxValue;
		for (int i = 0; i < momenteelLevendeJoekels; i++)
		{
			float afstand = p.DistanceTo(joekels[i].Position);
			if (afstand <= joekels[i].Chonkheid && afstand < besteAfstand) { beste = i; besteAfstand = afstand; }
		}
		if (beste < 0) return false;
		if (volgpunt.GetParent() == null) AddChild(volgpunt);
		geselecteerdId = joekels[beste].Id;
		geselecteerdGok = beste;
		ToonGeselecteerde();
		return true;
	}

	public void Deselecteer() => geselecteerdId = 0;

	// elke frame (vanuit DnaPaneel): tekst bijwerken. Is hij dood, dan paneel dicht (= Joekel.MaakDoodEnRuimOp)
	public void ToonGeselecteerde()
	{
		if (geselecteerdId == 0) return;
		var label = GetTree().CurrentScene.GetNode<Label>("%DnaLabel");
		geselecteerdGok = IndexVanJoekel(geselecteerdId, geselecteerdGok);
		if (geselecteerdGok < 0)
		{
			geselecteerdId = 0;
			label.GetParent<Control>().Visible = false;
			(GetViewport().GetCamera2D() as Camera)?.StopVolgen(); // anders blijft de camera vastzitten aan het (stilstaande) volgpunt
			return;
		}

		var j = joekels[geselecteerdGok];
		volgpunt.Position = j.Position;

		// = Joekel.Toon (Murigheid als getal: er zijn geen stuitertypes meer)
		var s = "DNA\n";
		for (int r = 0; r < Dna.N; r++)
			s += j.Dna[r, 0].ToString("F2") + "  " + j.Dna[r, 1].ToString("F2") + "  " + j.Dna[r, 2].ToString("F2") + "\n";
		s += "\n";
		foreach (var k in Karaktereigenschap.Alle)
			s += k.Naam + ": " + k.Van(j.Dna) + "\n";
		s += "Gezond: " + j.HuidigeGezonheid + "/" + j.MaximaleGezondheid + "\n";
		s += "Honger: " + j.Hunger.ToString("F1") + "   Plan: " + j.CurrentPlan + "\n";

		label.Text = s;
		label.GetParent<Control>().Visible = true;
	}
}
