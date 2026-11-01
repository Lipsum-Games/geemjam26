using Godot;

// drollen: bruine cirkeltjes, zelfde MultiMesh-truc als het voedsel
public partial class JoekelSim
{
	MultiMesh drolMultiMesh;

	void MaakDrolTekening()
	{
		drolMultiMesh = new MultiMesh
		{
			TransformFormat = MultiMesh.TransformFormatEnum.Transform2D,
			Mesh = MaakCirkel(8),
			InstanceCount = MaxDrollen,
			CustomAabb = new Aabb(new Vector3(-20, -20, 0), new Vector3(World.WorldSize.X + 40, World.WorldSize.Y + 40, 0)),
		};
		AddChild(new MultiMeshInstance2D { Name = "DrolTekening", Multimesh = drolMultiMesh, Modulate = new Color(0.45f, 0.28f, 0.1f), PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off });
	}

	void TekenDrollen()
	{
		drolMultiMesh.VisibleInstanceCount = momenteelDrollen;
		for (int d = 0; d < momenteelDrollen; d++)
			drolMultiMesh.SetInstanceTransform2D(d, new Transform2D(0, new Vector2(DrolStraal, DrolStraal), 0, drollen[d].Position));
	}
}
