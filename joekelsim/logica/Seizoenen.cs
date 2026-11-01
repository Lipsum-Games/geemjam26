using Godot;

// Een jaar met 4 seizoenen. De temperatuur loopt als een golf: midden winter het koudst, midden zomer het warmst.
public partial class JoekelSim
{
	const float JaarDuur = 16 * 60f; // seconden sim-tijd dat een jaar duurt
	const float KoudsteTemperatuur = -5f;
	const float WarmsteTemperatuur = 30f;
	static readonly string[] SeizoenNamen = { "Winter", "Lente", "Zomer", "Herfst" };

	// we beginnen midden in de lente
	float jaarTijd = JaarDuur * 0.25f;

	// 0 = midden winter, 0.5 = midden zomer
	float JaarFase => jaarTijd % JaarDuur / JaarDuur;
	int Jaar => (int)(jaarTijd / JaarDuur) + 1;

	public float Temperatuur => Mathf.Lerp(KoudsteTemperatuur, WarmsteTemperatuur, (1 - Mathf.Cos(JaarFase * Mathf.Tau)) / 2);

	// winter loopt van fase 0.875 tot 0.125, lente daarna, etc.
	string Seizoen => SeizoenNamen[(int)((JaarFase + 0.125f) * 4) % 4];

	// 0 = koudst, 1 = warmst
	float Warmte => Mathf.InverseLerp(KoudsteTemperatuur, WarmsteTemperatuur, Temperatuur);

	// zomer = overvloed, winter = schaars
	const float ZomerVoedselInterval = 0.25f;
	const float WinterVoedselInterval = 0.8f;
	float SeizoenVoedselInterval => Mathf.Lerp(WinterVoedselInterval, ZomerVoedselInterval, Warmte);

	// onder deze temperatuur kost kou extra honger per seconde, het meest bij de koudste temperatuur
	const float KouGrens = 10f;
	const float MaxKouHonger = 0.25f;

	float KouHonger(int i)
	{
		float kou = Mathf.Clamp((KouGrens - Temperatuur) / (KouGrens - KoudsteTemperatuur), 0, 1);
		// isolatie: chonk 10 houdt de helft van de kou tegen, dikker nog meer
		float isolatie = joekels[i].Chonkheid / (joekels[i].Chonkheid + 10);
		return MaxKouHonger * kou * (1 - isolatie);
	}

	void UpdateSeizoen(float dt) => jaarTijd += dt;
}
