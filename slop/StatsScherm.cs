using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// Stats-venster met tabs: een Godot Window, dus slepen (titelbalk), resizen (randen) en sluiten gratis.
// Opent op 80% van het scherm, half-transparant.
// Grafieken wijzen naar dezelfde lijsten die Statistiek vult, dus ze updaten live zonder kopiëren.
// Per tab een lijst checkboxes om lijnen aan/uit te zetten.
public partial class StatsScherm : Window
{
	static readonly Color[] Kleuren ={ Colors.White, Colors.Orange, Colors.SkyBlue, Colors.LightGreen, Colors.HotPink, Colors.Yellow, Colors.Violet, Colors.Coral, Colors.Aquamarine, Colors.Khaki };

	Statistiek _stats;
	Clusters _clusters;
	Grafiek _populatie, _groepGroottes;
	(Grafiek grafiek, VBoxContainer keuzes) _traits;
	GenHistogrammen _histogrammen;
	ClusterLijst _clusterLijst;

	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Visible = false;
		Title = "Stats";
		Transparent = true;
		TransparentBg = true;
		CloseRequested += Hide;

		var achtergrond = new PanelContainer();
		achtergrond.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		achtergrond.AddThemeStyleboxOverride("panel", new StyleBoxFlat { BgColor = new Color(0.05f, 0.05f, 0.08f, 0.85f) });
		AddChild(achtergrond);

		var tabs = new TabContainer();
		achtergrond.AddChild(tabs);

		_populatie = new Grafiek { Name = "Populatie" };
		tabs.AddChild(_populatie);
		_traits = MaakTab(tabs, "Traits (gemiddeld)", elkEigenSchaal: true);
		_traits.grafiek.GebruikVast = true;
		var zoom = new CheckBox { Text = "Zoom: eigen schaal per lijn", FocusMode = Control.FocusModeEnum.None, TooltipText = "Uit: elke lijn op 0–100% van zijn theoretisch mogelijke bereik (onderling vergelijkbaar).\nAan: elke lijn ingezoomd op zijn eigen min–max (kleine trends zichtbaar)." };
		var traitsGrafiek = _traits.grafiek;
		zoom.Toggled += aan => traitsGrafiek.GebruikVast = !aan;
		_traits.keuzes.AddChild(zoom);
		_traits.keuzes.AddChild(new HSeparator());

		_histogrammen = new GenHistogrammen { Name = "DNA genen" };
		tabs.AddChild(_histogrammen);

		var groepenTab = new VBoxContainer { Name = "Groepen" };
		tabs.AddChild(groepenTab);

		var keuzeRij = new HBoxContainer();
		keuzeRij.AddChild(new Label { Text = "Groepeer op:" });
		var wijze = new OptionButton { FocusMode = Control.FocusModeEnum.None };
		wijze.AddItem("DNA (alle 9 genen)", (int)Clusters.Modus.Dna);
		wijze.AddItem("Plek (kuddes)", (int)Clusters.Modus.Plek);
		wijze.AddItem("Geur (mistkleur)", (int)Clusters.Modus.Geur);
		wijze.SetItemTooltip(0, "Soorten die in veel genen tegelijk verschillen. Kleine verschillen (1-3 genen) zie je hier niet.");
		wijze.SetItemTooltip(1, "Joekels die bij elkaar in de buurt lopen. Losse joekels = zwervers.");
		wijze.SetItemTooltip(2, "Joekels met (bijna) dezelfde geur = dezelfde mistkleur, ook als ze door elkaar lopen.");
		wijze.ItemSelected += index =>
		{
			if (_clusters == null) return;
			_clusters.ZetModus((Clusters.Modus)wijze.GetItemId((int)index));
			_groepGroottes.Lijnen.Clear();              // lijnen van de nieuwe modus komen er in _Process weer bij (historie blijft)
			_losheid.SetValueNoSignal(_clusters.Losheid); // slider toont de stand van deze modus
		};
		keuzeRij.AddChild(wijze);

		keuzeRij.AddChild(new Label { Text = "   Grof" });
		var losheid = new HSlider
		{
			MinValue = 0, MaxValue = 1, Step = 0.05, Value = 0.5,
			CustomMinimumSize = new Vector2(200, 0),
			SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
			FocusMode = Control.FocusModeEnum.None,
			TooltipText = "Hoe fijn groepen worden afgebakend. Wordt toegepast als je de slider loslaat;\n" +
				"groepen worden dan opnieuw afgebakend maar zoveel mogelijk gekoppeld aan de bestaande (zelfde nummer/kleur),\n" +
				"dus de grafiek loopt door. Elke modus onthoudt zijn eigen slider-stand.\n" +
				"Plek: hoe dicht joekels bij elkaar moeten lopen. Fijn = strakker: kuddes splitsen,\n" +
				"   maar meer joekels worden zwerver, dus het aantal groepen kan ook dálen.\n" +
				"Geur: hoe klein een gat in geurwaarde mag zijn om twee groepen te scheiden (zelfde effect).\n" +
				"DNA: hoeveel een extra groep de genetische spreiding moet verlagen. Fijn = meer soorten;\n" +
				"   helemaal rechts splitst ook pure ruis op.",
		};
		losheid.ValueChanged += v => _sliderWacht = 0.4f; // pas toepassen als je even stilstaat
		_losheid = losheid;
		keuzeRij.AddChild(losheid);
		keuzeRij.AddChild(new Label { Text = "Fijn" });
		groepenTab.AddChild(keuzeRij);

		var groepen = new VSplitContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill };
		groepenTab.AddChild(groepen);
		var scroll = new ScrollContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill };
		_clusterLijst = new ClusterLijst { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
		scroll.AddChild(_clusterLijst);
		groepen.AddChild(scroll);
		_groepGroottes = new Grafiek { VerbergNulInLegenda = true, CustomMinimumSize = new Vector2(0, 160), SizeFlagsVertical = Control.SizeFlags.ExpandFill };
		groepen.AddChild(_groepGroottes);
	}

	// checkbox-labels tonen live de huidige waarde (de legenda in de grafiek staat uit op deze tabs)
	readonly List<(CheckBox vinkje, string naam, List<float> data, Grafiek grafiek)> _labels = new();

	HSlider _losheid;
	float _sliderWacht = -1;
	(Clusters.Modus, int) _gezieneHistorie;

	public override void _Process(double delta)
	{
		if (!Visible || _clusters == null) return;
		_clusters.VerwerkHerberekeningen();

		// historie vervangen (herberekend) of andere modus: grafieklijnen opnieuw ophalen
		var historie = (_clusters.Wijze, _clusters.HistorieVersie);
		if (historie != _gezieneHistorie) { _groepGroottes.Lijnen.Clear(); _gezieneHistorie = historie; }

		// slider: pas toepassen als hij ~0.4 s (echte tijd) stil staat, dan schoon opnieuw indelen
		if (_sliderWacht > 0)
		{
			_sliderWacht -= (float)delta / (float)Engine.TimeScale;
			if (_sliderWacht <= 0)
			{
				_clusters.Losheid = (float)_losheid.Value;
				_clusters.Werk(bewaar: false);   // direct: huidige indeling met de nieuwe stand
				_clusters.HerberekenHistorie();  // achtergrond: héle geschiedenis opnieuw met de nieuwe stand
			}
		}

		foreach (var (vinkje, naam, data, grafiek) in _labels)
			vinkje.Text = $"{grafiek.Status(naam, data)}   {naam}";
		foreach (var (id, data) in _clusters.GrootteHistorie)
			if (!_groepGroottes.Lijnen.Exists(l => l.data == data))
				_groepGroottes.Lijnen.Add(($"{GroepNamen.Naam(id, _clusters.Wijze)} ({id})", Clusters.KleurVan(id), data));
	}

	static (Grafiek, VBoxContainer) MaakTab(TabContainer tabs, string naam, bool elkEigenSchaal)
	{
		var rij = new HBoxContainer { Name = naam };
		tabs.AddChild(rij);

		var scroll = new ScrollContainer { CustomMinimumSize = new Vector2(260, 0), HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled };
		var keuzes = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
		scroll.AddChild(keuzes);
		rij.AddChild(scroll);

		var grafiek = new Grafiek { ElkEigenSchaal = elkEigenSchaal, ToonLegenda = false, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
		rij.AddChild(grafiek);
		return (grafiek, keuzes);
	}

	public void Wissel()
	{
		Visible = !Visible;
		if (!Visible) return;

		if (_stats == null)
		{
			var scherm = GetParent().GetViewport().GetVisibleRect().Size;
			// 70% met ruime marge: een embedded venster kan niet buiten het spelvenster, dus laat ruimte om te slepen/groter te maken
			Size = (Vector2I)(scherm * 0.7f);
			Position = (Vector2I)(scherm * 0.15f);
		}

		if (_stats == null)
		{
			_stats = GetTree().CurrentScene.GetNode<Statistiek>("Statistiek");
			_populatie.Lijnen.Add(("Joekels", Colors.White, _stats.Populatie));
			_clusters = GetTree().CurrentScene.GetNode<Clusters>("Clusters");
			_histogrammen.Bron = _clusters;
			_clusterLijst.Bron = _clusters;

			_traits.grafiek.VasteBereiken = _stats.TheoretischBereik;
			Func<float> statsTijd = () => _stats.Populatie.Count * _stats.Interval;
			_populatie.Tijdsduur = statsTijd;
			_traits.grafiek.Tijdsduur = statsTijd;
			_groepGroottes.Tijdsduur = () => (_clusters.GrootteHistorie.Count > 0 ? _clusters.GrootteHistorie.First().Value.Count : 0) * _clusters.Interval;
		}
		Vul(_traits, _stats.Traits, standaardAan: 3);
	}

	// voegt reeksen toe die er nog niet zijn (bv. als het scherm geopend werd vóór de eerste meting)
	void Vul((Grafiek grafiek, VBoxContainer keuzes) tab, Dictionary<string, List<float>> bron, int standaardAan)
	{
		foreach (var (naam, data) in bron)
		{
			if (tab.grafiek.Lijnen.Exists(l => l.naam == naam)) continue;

			int i = tab.grafiek.Lijnen.Count;
			var kleur = Kleuren[i % Kleuren.Length];
			tab.grafiek.Lijnen.Add((naam, kleur, data));

			bool aan = i < standaardAan;
			if (!aan) tab.grafiek.Verborgen.Add(naam);

			var vinkje = new CheckBox
			{
				Text = naam,
				TooltipText = naam,
				ButtonPressed = aan,
				FocusMode = Control.FocusModeEnum.None,
				ClipText = true,
				TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
			};
			_labels.Add((vinkje, naam, data, tab.grafiek));
			vinkje.AddThemeColorOverride("font_color", kleur);
			vinkje.AddThemeColorOverride("font_pressed_color", kleur);
			vinkje.AddThemeColorOverride("font_hover_color", kleur);
			vinkje.AddThemeColorOverride("font_hover_pressed_color", kleur);
			vinkje.Toggled += staatAan =>
			{
				if (staatAan) tab.grafiek.Verborgen.Remove(naam);
				else tab.grafiek.Verborgen.Add(naam);
			};
			tab.keuzes.AddChild(vinkje);
		}
	}
}
