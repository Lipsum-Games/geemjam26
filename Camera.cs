using Godot;
using System;

public partial class Camera : Camera2D
{
	 void ZoomAt(float factor)
		{
			var oldZoom = Zoom;
			Zoom = Zoom * factor;

			var offset = GetViewport().GetMousePosition() - GetViewportRect().Size / 2;
			Position += offset / oldZoom - offset / Zoom;
		}
		
	public override void _UnhandledInput(InputEvent e)
		{
			if (e is InputEventMouseMotion m && (m.ButtonMask & MouseButtonMask.Left) != 0)
				Position -= m.Relative / Zoom;
			else if (e is InputEventMouseButton b && b.Pressed && b.ButtonIndex == MouseButton.WheelUp)
				ZoomAt(1.1f);
			else if (e is InputEventMouseButton b2 && b2.Pressed && b2.ButtonIndex == MouseButton.WheelDown)
				ZoomAt(1 / 1.1f);
			else if (e is InputEventMagnifyGesture g)
				ZoomAt(g.Factor);
			else if (e is InputEventPanGesture p)
				ZoomAt(1f - p.Delta.Y * 0.05f);
		}
}
