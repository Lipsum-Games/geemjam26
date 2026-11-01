using Godot;

public partial class World : Node2D
{
	public static readonly Vector2 WorldSize = new(3000, 2000);

	public override void _Draw()
	{
		DrawRect(new Rect2(Vector2.Zero, WorldSize), new Color(0.78f, 1, 1));
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Slop.Installeer(this); // slop: stats-scherm + clusters, zie slop/

		// alle joekels en het voedsel zitten in één JoekelSim-node (arrays), zie joekelsim/
		AddChild(new JoekelSim { Name = "JoekelSim" });
	}
}
