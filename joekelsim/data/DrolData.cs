using Godot;

// een drol die naar achter is geschoten
public struct DrolData
{
	public Vector2 Position;
	public Vector2 Velocity;
	// hoeveel px hij nog mag vliegen
	public float Resterend;
	public float Schade;
	// wie hem schoot (die raakt hij niet)
	public int SchutterId;
}
