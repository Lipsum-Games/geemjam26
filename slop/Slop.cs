using Godot;

// Eén ingang voor alle slop (stats-scherm, clusters). De artisanale code roept alleen
// Slop.Installeer(this) aan in World._Ready. Wil je de slop kwijt: haal die regel weg + deze map.
// Wat de stats van een joekel willen weten. Sleutel = het Id van de joekel in JoekelSim.
public record JoekelInfo(object Sleutel, Dna Dna, Vector2 Pos, float Geur);

public static class Slop
{
	// alle levende joekels als lijst (uit JoekelSim)
	public static System.Collections.Generic.List<JoekelInfo> Joekels(Node wereld)
		=> wereld.GetNodeOrNull<JoekelSim>("JoekelSim")?.Infos() ?? new();

	public static void Installeer(World wereld)
	{
		wereld.AddChild(new Statistiek { Name = "Statistiek" });
		wereld.AddChild(new Clusters { Name = "Clusters" });

		var knopjes = wereld.GetNode<HBoxContainer>("CanvasLayer/SimKnopjes");
		var scherm = new StatsScherm();
		knopjes.GetParent().CallDeferred(Node.MethodName.AddChild, scherm);

		var knop = new Button { Text = "Stats", FocusMode = Control.FocusModeEnum.None };
		knop.Pressed += scherm.Wissel;
		knopjes.CallDeferred(Node.MethodName.AddChild, knop);
	}
}
