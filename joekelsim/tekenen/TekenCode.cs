using Godot;

// Hier worden alle joekels getekend, met behulp van een Multimesh, heel simpel gewoon hun vorm: de cirkel!
// Maar deze cirkel is niet perfect, het is gemaakt van 12 driehoeken dus lekker efficient :WICKED:
public partial class JoekelSim
{
	// https://docs.godotengine.org/en/stable/classes/class_multimesh.html
	// De MultiMesh van alle joekels, wat zit hierin? Een paar belangrijke variables:
	//  De mesh - dit is een ArrayMesh gemaakt door MaakCirkel(). is 48 groot want 16 driehoeken x 3.
	//  De TransformFormat - dit bepaalt hoe precies ze getekent moeten worden in 2d space.
	//  De InstanceCount - de maximale units die in de mesh kunnen zotten, gelijk aan max joekels.
	MultiMesh[] joekelBanden = new MultiMesh[3]; // 3 MultiMeshes, één per kleurband

	// Een pass die een MultiMesh maakt om getekend te worden, wordt maar een keer gecalled en daarna alleen nog maar aangepast.
	void MaakJoekelTekening()
	{
		// 3 stukken taart, band 1 is het pijltje (punt vooraan, wijst naar waar hij heen springt)
		float s = Mathf.DegToRad(150);
		Vector2[][] banden = { Boog(0, s), [new Vector2(1, 0), .. Boog(s, Mathf.Tau - s)], Boog(Mathf.Tau - s, Mathf.Tau) };
		for (int b = 0; b < 3; b++)
		{
			joekelBanden[b] = new MultiMesh
			{
				TransformFormat = MultiMesh.TransformFormatEnum.Transform2D, // voor 2d space, bepaalt hoe de buffer deruut ziet
				UseColors = true, // elke joekel zijn eigen kleur (moet ook vóór InstanceCount)
				Mesh = MaakPolygoon(banden[b]),
				InstanceCount = MaxJoekels, // maximale hoeveelheid joekeltjes
				// maak een aanbb (Axis-Aligned Bounding Box) om de wereld heen. Godot wil alleen de multimesh tekenen als dit in beeld is.
				// Dit voorkomt dat joekeltjes soms niet worden getekend.
				CustomAabb = new Aabb(new Vector3(-20, -20, 0), new Vector3(World.WorldSize.X + 40, World.WorldSize.Y + 40, 0)),
			};
			// voeg natuurlijk toe aan de scene tree zodat het getekend kan worden
			// geen interpolatie want dat fucked als we indexen swappen als iets weggaat/doodgaat.
			AddChild(new MultiMeshInstance2D { Name = $"JoekelBand{b}", Multimesh = joekelBanden[b], PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off });
		}
	}

	// Standaard straal van 1 (de MultiMesh schaalt met Chonkheid)
	static Vector2[] Boog(float a0, float a1)
	{
		const int n = 16;
		var pts = new Vector2[n + 1];
		for (int k = 0; k <= n; k++)
			pts[k] = Vector2.FromAngle(Mathf.Lerp(a0, a1, k / (float)n));
		return pts;
	}

	static ArrayMesh MaakPolygoon(Vector2[] punten)
	{
		var hoeken = new Vector2[(punten.Length - 2) * 3];
		for (int k = 1; k < punten.Length - 1; k++)
		{
			hoeken[(k - 1) * 3] = punten[0];
			hoeken[(k - 1) * 3 + 1] = punten[k];
			hoeken[(k - 1) * 3 + 2] = punten[k + 1];
		}
		var arrays = new Godot.Collections.Array();
		arrays.Resize((int)Mesh.ArrayType.Max);
		arrays[(int)Mesh.ArrayType.Vertex] = hoeken;
		var mesh = new ArrayMesh();
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
		return mesh;
	}

	// Update de MultiMesh zodat het overeenkomt met hoe de joekels nu zijn (in de wereld)
	void TekenJoekels()
	{
		// teken alleen de levende
		for (int b = 0; b < 3; b++) joekelBanden[b].VisibleInstanceCount = momenteelLevendeJoekels;
		for (int i = 0; i < momenteelLevendeJoekels; i++)
		{
			// update de mesh met nieuwe waardes van de joekel.
			var transform = new Transform2D(
				rotation: joekels[i].Heading, // (gedraaid naar zijn laatste sprong)
				scale: new Vector2(joekels[i].Chonkheid, joekels[i].Chonkheid),
				skew: 0,
				origin: joekels[i].Position);
			joekelBanden[0].SetInstanceTransform2D(i, transform);
			joekelBanden[1].SetInstanceTransform2D(i, transform);
			joekelBanden[2].SetInstanceTransform2D(i, transform);
			joekelBanden[0].SetInstanceColor(i, joekels[i].Kleur0);
			joekelBanden[1].SetInstanceColor(i, joekels[i].Kleur1);
			joekelBanden[2].SetInstanceColor(i, joekels[i].Kleur2);
		}
	}

	// FoodSources is ook een MultiMesh voor snelle tekenen.
	MultiMesh foodSourceMultiMesh;

	// Hoe groot is het voedsel? Draw en collision.
	const float FoodSourceStraal = 20f;

	void MaakFoodSourceTekening()
	{
		foodSourceMultiMesh = new MultiMesh
		{
			TransformFormat = MultiMesh.TransformFormatEnum.Transform2D,
			Mesh = MaakCirkel(16),
			InstanceCount = MaxFoodSources,
			// AABB hier ook nodig zodat ze altijd getekent worden.
			CustomAabb = new Aabb(new Vector3(-20, -20, 0), new Vector3(World.WorldSize.X + 40, World.WorldSize.Y + 40, 0)),
		};
		// Alles groen
		Color groen = new Color(0.2f, 0.7f, 0.2f);
        // geen interpolatie want dat fucked als we indexen swappen als iets weggaat/doodgaat.
		AddChild(new MultiMeshInstance2D { Name = "FoodSourceTekening", Multimesh = foodSourceMultiMesh, Modulate = groen, PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off }); // zelfde als bij joekels: geen spookvoedsel
	}

	void TekenFoodSources()
	{
		foodSourceMultiMesh.VisibleInstanceCount = momenteelFoodSources;
		for (int f = 0; f < momenteelFoodSources; f++)
		{
			foodSourceMultiMesh.SetInstanceTransform2D(f, new Transform2D(
				rotation: 0,
				scale: new Vector2(FoodSourceStraal, FoodSourceStraal),
				skew: 0,
				origin: foodSources[f].Position));
		}
	}

	// cirkel met straal van 1 (default). punten is de hoeveelheid taartpuntjes.
	static ArrayMesh MaakCirkel(int punten)
	{
		// tis wel een driehoek he
		var hoeken = new Vector2[punten * 3];
		// bepaal de 3 punten van de driehoek
		for (int k = 0; k < punten; k++)
		{
			hoeken[k * 3] = Vector2.Zero;
			hoeken[k * 3 + 1] = Vector2.FromAngle(Mathf.Tau * k / punten);
			hoeken[k * 3 + 2] = Vector2.FromAngle(Mathf.Tau * (k + 1) / punten);
		}

		// Prepareer onze meshes in een array voor het tekenen.
		var arrays = new Godot.Collections.Array();
		arrays.Resize((int)Mesh.ArrayType.Max);
		arrays[(int)Mesh.ArrayType.Vertex] = hoeken;

		// en prep de driehoekjes voor tekenen
		var mesh = new ArrayMesh();
		mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
		return mesh;
	}
}
