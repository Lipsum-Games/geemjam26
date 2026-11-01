using Godot;

// Dikke joekels schieten drollen naar achter. De eerste die geraakt wordt krijgt schade, hoe dikker de schutter hoe harder.
public partial class JoekelSim
{
	const int MaxDrollen = 2048;
	const float DrolSnelheid = 200f;
	const float DrolBereik = 200f;
	const float DrolStraal = 6f;
	const float DrolSchadePerChonk = 1.5f;
	// schijten kost voedsel: een drol is verteerd eten
	const float DrolHongerKosten = 1f;

	DrolData[] drollen = new DrolData[MaxDrollen];
	int momenteelDrollen;

	void Schijten(float dt)
	{
		// afvuren
		for (int i = 0; i < momenteelLevendeJoekels; i++)
		{
			if (joekels[i].Schijtlust <= 0) continue;
			joekels[i].SchijtTimer += joekels[i].Schijtlust * dt;
			if (joekels[i].SchijtTimer < 1) continue;
			joekels[i].SchijtTimer -= 1;
			if (momenteelDrollen == MaxDrollen) continue;
			joekels[i].Hunger = Mathf.Min(joekels[i].Hunger + DrolHongerKosten, HungerMax);

			// recht naar achter (tegenover zijn laatste sprong), net buiten zijn eigen lijf
			Vector2 achter = Vector2.FromAngle(joekels[i].Heading + Mathf.Pi);
			drollen[momenteelDrollen++] = new DrolData
			{
				Position = joekels[i].Position + achter * (joekels[i].Chonkheid + DrolStraal),
				Velocity = achter * DrolSnelheid,
				Resterend = DrolBereik,
				Schade = joekels[i].Chonkheid * DrolSchadePerChonk,
				SchutterId = joekels[i].Id,
			};
		}

		// vliegen en raken. achterstevoren zodat swap-remove geen drol overslaat
		for (int d = momenteelDrollen - 1; d >= 0; d--)
		{
			drollen[d].Position += drollen[d].Velocity * dt;
			drollen[d].Resterend -= DrolSnelheid * dt;
			bool weg = drollen[d].Resterend <= 0;

			int aantal = weg ? 0 : BurenRond(drollen[d].Position, DrolStraal + grootsteChonkheid);
			for (int k = 0; k < aantal; k++)
			{
				int j = burenBuffer[k];
				if (joekels[j].Id == drollen[d].SchutterId) continue;
				if (drollen[d].Position.DistanceTo(joekels[j].Position) >= DrolStraal + joekels[j].Chonkheid) continue;
				joekels[j].HuidigeGezonheid -= drollen[d].Schade;
				weg = true;
				break;
			}

			// geraakt of uitgevlogen: hij valt op de grond
			if (weg)
			{
				LandDrol(drollen[d].Position);
				drollen[d] = drollen[--momenteelDrollen];
			}
		}
	}
}
