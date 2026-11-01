using Godot;

// drolhopen: donkerbruine cirkels, groter naarmate er meer in zit
public partial class JoekelSim
{
	MultiMesh drolHoopMultiMesh;
	// bruine stankwolk om elke hoop
	MultiMesh stankMultiMesh;

	void MaakDrolHoopTekening()
	{
		drolHoopMultiMesh = new MultiMesh
		{
			TransformFormat = MultiMesh.TransformFormatEnum.Transform2D,
			Mesh = MaakCirkel(12),
			InstanceCount = MaxDrolHopen,
			CustomAabb = new Aabb(new Vector3(-20, -20, 0), new Vector3(World.WorldSize.X + 40, World.WorldSize.Y + 40, 0)),
		};
		AddChild(new MultiMeshInstance2D { Name = "DrolHoopTekening", Multimesh = drolHoopMultiMesh, Modulate = new Color(0.32f, 0.2f, 0.07f), PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off });

		stankMultiMesh = new MultiMesh
		{
			TransformFormat = MultiMesh.TransformFormatEnum.Transform2D,
			UseColors = true,
			Mesh = MaakTextuurVierkant(),
			InstanceCount = MaxDrolHopen,
			CustomAabb = new Aabb(new Vector3(-200, -200, 0), new Vector3(World.WorldSize.X + 400, World.WorldSize.Y + 400, 0)),
		};
		AddChild(new MultiMeshInstance2D { Name = "StankTekening", Multimesh = stankMultiMesh, Texture = MaakRanzigheidsMist(), PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off });
	}

	void TekenDrolHopen()
	{
		drolHoopMultiMesh.VisibleInstanceCount = momenteelDrolHopen;
		stankMultiMesh.VisibleInstanceCount = momenteelDrolHopen;
		for (int h = 0; h < momenteelDrolHopen; h++)
		{
			// oppervlakte groeit met de hoeveelheid (wortel)
			float straal = DrolStraal * Mathf.Sqrt(drolHopen[h].Hoeveelheid);
			drolHoopMultiMesh.SetInstanceTransform2D(h, new Transform2D(0, new Vector2(straal, straal), 0, drolHopen[h].Position));

			// stankwolk: zelfde bereik als in Stank.cs, en hoe groter de hoop hoe dikker de wolk
			float wortel = Mathf.Sqrt(drolHopen[h].Hoeveelheid);
			float bereik = StankBereik(wortel);
			stankMultiMesh.SetInstanceTransform2D(h, new Transform2D(0, new Vector2(bereik, bereik), 0, drolHopen[h].Position));
			stankMultiMesh.SetInstanceColor(h, new Color(0.4f, 0.26f, 0.05f, Mathf.Min(0.4f + wortel * 0.06f, 0.85f)));
		}
	}
}
