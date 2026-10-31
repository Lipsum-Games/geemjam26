using Godot;
using System;

public partial class Camera : Camera2D
{

	[Export] public float VolgZoom = 2f;
	Joekel _volg;
	Vector2? _zoomDoel;
	Vector2 _volgOffset;

	public void Volg(Joekel joekel)
	{
		_volgOffset = Vector2.Zero;
		_volg = joekel;
		_zoomDoel = new Vector2(VolgZoom, VolgZoom);
	}

	 void ZoomAt(float factor)
		{
			var oldZoom = Zoom;
			Zoom = Zoom * factor;

			var offset = GetViewport().GetMousePosition() - GetViewportRect().Size / 2;
			var verschuiving = offset / oldZoom - offset / Zoom;
			Position += verschuiving;
			if (_volg != null) _volgOffset += verschuiving;
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

		this.Position = new Vector2(
			System.Math.Clamp(Position.X, 0, WorldTiles.WORLD_TILE_SIZE_X * WorldTiles.WORLD_AMOUNT_TILES_X),
			System.Math.Clamp(Position.Y, 0, WorldTiles.WORLD_TILE_SIZE_Y * WorldTiles.WORLD_AMOUNT_TILES_Y)
		);
	}

	public override void _Process(double delta)
	{
		if (_volg == null) return;
		if (!IsInstanceValid(_volg)) { _volg = null; return; }

		float dt = (float)delta;
		Position = Position.Lerp(_volg.GlobalPosition, 1 - Mathf.Exp(-5 * dt));

		if (_zoomDoel is Vector2 doel)
		{
			Zoom = Zoom.Lerp(doel, 1 - Mathf.Exp(-3 * dt));
			if (Zoom.DistanceTo(doel) < 0.02f) _zoomDoel = null;
		}
	}

}
