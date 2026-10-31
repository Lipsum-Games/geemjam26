using Godot;

// Eén ingang voor alle slop (stats-scherm, clusters). De artisanale code roept alleen
// Slop.Installeer(this) aan in World._Ready. Wil je de slop kwijt: haal die regel weg + deze map.
public static class Slop
{
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
