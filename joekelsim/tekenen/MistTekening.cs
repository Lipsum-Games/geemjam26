using Godot;

// De ranzigheidsmist om joekels met een aura
public partial class JoekelSim
{
	MultiMesh mistMultiMesh;

	void MaakMistTekening()
	{
		mistMultiMesh = new MultiMesh
		{
			TransformFormat = MultiMesh.TransformFormatEnum.Transform2D,
			UseColors = true,
			Mesh = MaakTextuurVierkant(),
			InstanceCount = MaxJoekels,
			CustomAabb = new Aabb(new Vector3(-160, -160, 0), new Vector3(World.WorldSize.X + 320, World.WorldSize.Y + 320, 0)),
		};
		AddChild(new MultiMeshInstance2D { Name = "MistTekening", Multimesh = mistMultiMesh, Texture = MaakRanzigheidsMist(), PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off });
	}

	void TekenMist()
	{
		mistMultiMesh.VisibleInstanceCount = momenteelLevendeJoekels;
		for (int i = 0; i < momenteelLevendeJoekels; i++)
		{
			float grootte = joekels[i].RanzigKracht > 0 ? joekels[i].RanzigBereik : 0;
			mistMultiMesh.SetInstanceTransform2D(i, new Transform2D(0, new Vector2(grootte, grootte), 0, joekels[i].Position));

			var geurKleur = Color.FromHsv((joekels[i].Geur + 1) / 2 * 0.8f, 0.7f, 1f);
			geurKleur.A = joekels[i].RanzigKracht / 8 * 0.5f;
			mistMultiMesh.SetInstanceColor(i, geurKleur);
		}
	}

	static ArrayMesh MaakTextuurVierkant()
	{
		var arrays = new Godot.Collections.Array();
		arrays.Resize((int)Mesh.ArrayType.Max);
		arrays[(int)Mesh.ArrayType.Vertex] = new Vector2[] { new(-1, -1), new(1, -1), new(1, 1), new(-1, -1), new(1, 1), new(-1, 1) };
		arrays[(int)Mesh.ArrayType.TexUV] = new Vector2[] { new(0, 0), new(1, 0), new(1, 1), new(0, 0), new(1, 1), new(0, 1) };
		var mesh = new ArrayMesh();
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
		return mesh;
	}

	static GradientTexture2D MaakRanzigheidsMist()
	{
		var g = new Gradient();
		g.Offsets = [0f, 0.25f, 0.5f, 0.75f, 1f];
		g.Colors = [new Color(1, 1, 1, 1f), new Color(1, 1, 1, 0.82f), new Color(1, 1, 1, 0.46f), new Color(1, 1, 1, 0.17f), new Color(1, 1, 1, 0f)];
		return new GradientTexture2D
		{
			Gradient = g,
			Fill = GradientTexture2D.FillEnum.Radial,
			FillFrom = new Vector2(0.5f, 0.5f),
			FillTo = new Vector2(1f, 0.5f),
			Width = 128,
			Height = 128,
		};
	}
}
