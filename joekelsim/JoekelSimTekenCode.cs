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
	MultiMesh joekelMultiMesh;

	// Een pass die een MultiMesh maakt om getekend te worden, wordt maar een keer gecalled en daarna alleen nog maar aangepast.
	void MaakJoekelTekening()
	{
		joekelMultiMesh = new MultiMesh
		{
			TransformFormat = MultiMesh.TransformFormatEnum.Transform2D, // voor 2d space, bepaalt hoe de buffer deruut ziet
			Mesh = MaakCirkel(16), // onze mesh, dikke cirkel deluxe
			InstanceCount = MaxJoekels, // maximale hoeveelheid joekeltjes
		};
		// voeg natuurlijk toe aan de scene tree zodat het getekend kan worden
		AddChild(new MultiMeshInstance2D { Name = "JoekelTekening", Multimesh = joekelMultiMesh });
	}

	// Update de MultiMesh zodat het overeenkomt met hoe de joekels nu zijn (in de wereld)
	void TekenJoekels()
	{
	    // teken alleen de levende
		joekelMultiMesh.VisibleInstanceCount = momenteelLevendeJoekels;
		for (int i = 0; i < momenteelLevendeJoekels; i++)
		{
			// update de mesh met nieuwe waardes van de joekel.
			joekelMultiMesh.SetInstanceTransform2D(i, new Transform2D(rotation: 0,
			scale: new Vector2(10, 10),
			skew: 0,
			position: joekelPosities[i]));
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
