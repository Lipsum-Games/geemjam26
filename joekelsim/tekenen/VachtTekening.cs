using Godot;

// vacht: een lichtgrijze rand onder de joekel, dikker en zichtbaarder met meer vacht
public partial class JoekelSim
{
	MultiMesh vachtMultiMesh;

	void MaakVachtTekening()
	{
		vachtMultiMesh = new MultiMesh
		{
			TransformFormat = MultiMesh.TransformFormatEnum.Transform2D,
			UseColors = true,
			Mesh = MaakCirkel(16),
			InstanceCount = MaxJoekels,
			CustomAabb = new Aabb(new Vector3(-40, -40, 0), new Vector3(World.WorldSize.X + 80, World.WorldSize.Y + 80, 0)),
		};
		AddChild(new MultiMeshInstance2D { Name = "VachtTekening", Multimesh = vachtMultiMesh, PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off });
	}

	void TekenVachten()
	{
		vachtMultiMesh.VisibleInstanceCount = momenteelLevendeJoekels;
		for (int i = 0; i < momenteelLevendeJoekels; i++)
		{
			float straal = joekels[i].Chonkheid * (1 + 0.4f * joekels[i].Vacht);
			vachtMultiMesh.SetInstanceTransform2D(i, new Transform2D(0, new Vector2(straal, straal), 0, joekels[i].Position));
			var kleur = new Color(0.85f, 0.85f, 0.8f, joekels[i].Vacht * 0.9f);
			vachtMultiMesh.SetInstanceColor(i, kleur);
		}
	}
}
