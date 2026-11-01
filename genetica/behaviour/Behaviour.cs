#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace Geemjam26ShipsumEvolve.genetica;

public class Behaviour
{
	private const int _check_behaviour_every = 5;
	private Random _random = new Random();
	internal Plan CurrentPlan { get; set; } = Plan.CHILL; 
	private Vector2? _targetPosition = null;
	private Node? _targetNode = null;
	private int _behaviourCheckCounter = 0;
	private int _runAwayTicks = 0;



	// Besluit wat de joekel gaat doen en returnt een tuple van richting (radialen) en gewenste snelheid
	public Tuple<float, float> DoeDingen(Joekel joekel, double dt)
	{

		_behaviourCheckCounter -= 1;
		if (_behaviourCheckCounter <= 0)
		{
			_behaviourCheckCounter = _check_behaviour_every;
			_besluitPlan(joekel, dt);

		}
		
		float direction = 0.0f;
		float speed = 0.0f;

		if (CurrentPlan == Plan.REPRODUCE)
		{
			if (_targetNode is not null && GodotObject.IsInstanceValid(_targetNode))
			{
				_targetPosition = ((Joekel)_targetNode).GetPosition();
			}
			else
			{
				CurrentPlan = Plan.CHILL;
				_targetNode = null;
				_targetPosition = null;
			}
		}
		
		if (CurrentPlan == Plan.CHILL)
		{
			speed = Karaktereigenschappen.ChillSpeed.Van(joekel.Dna);
			direction = joekel.getHeading() + (_random.NextSingle() - 0.5f) * 0.1f;
		} else if (_targetPosition is not null)
		{
			float distance = _targetPosition.Value.DistanceTo(joekel.Position);

			float maxVel = joekel.GetVelocityVal();

			if (distance < maxVel)
			{
				speed = (float)Math.Clamp(0.5f * (distance / maxVel), 0, 1);

			}
			else
			{
				speed = 1.0f;
			}

			direction = joekel.Position.AngleToPoint(_targetPosition.Value);
		}
		

		return new Tuple<float, float>(direction, speed);
	}

	private void _besluitPlan(Joekel joekel, double dt)
	{
		var introversie = Karaktereigenschappen.Introversie.Van(joekel.Dna);
		var trek = Karaktereigenschappen.Trek.Van(joekel.Dna) * (joekel.HungerMax - joekel.HungerMin) + joekel.HungerMin;
		var oehlalaigheid = Karaktereigenschappen.Oehlalalaigheid.Van(joekel.Dna);

		// Kijk eerst of we in een huidig plan zitten
		if (CurrentPlan == Plan.EAT || (CurrentPlan != Plan.CHILL && joekel.GetHunger() < trek))
		{
			// No need to act, only handle "end" conditions
			if (CurrentPlan == Plan.EAT && joekel.GetHunger() < joekel.HungerMin * 1.1f)
			{
				CurrentPlan = Plan.CHILL;
				_targetNode = null;
				_targetPosition = null;
			} else if (CurrentPlan == Plan.RUN_AWAY)
			{
				if (_runAwayTicks > 0)
				{
					_runAwayTicks -= 1;
				}
				else
				{
					CurrentPlan = Plan.CHILL;
					_targetPosition = null;
				}
			}
		}
		else
		{
			// Plan the next move
			(List<Joekel> joekels, List<FoodSource> foodSources) = getAllRelevantEntitiesInPerceptionRange(joekel);
			
			// Prio 1: Voedsel is heeeeeeel belangrijk
			if (CurrentPlan != Plan.EAT && joekel.GetHunger() > trek)
			{   
				FoodSource? food = foodSources.MinBy(fs => fs.Position.DistanceTo(joekel.Position) + _random.NextSingle() * 100.0);

				if (food is not null)
				{
					// Go to foodsource
					_targetPosition = food.Position;
					_targetNode = food;
					CurrentPlan = Plan.EAT;
				}
				else
				{
					// TODO: New "looking for food" behaviour?
				}
			} else if (CurrentPlan == Plan.CHILL)
			{
				// We doen niks, laten we wat gaan den

				(float drukteIndex, Vector2 target) = _compute_introversie_values(joekel, joekels, introversie);
				
				// Is het te druk?
				if (drukteIndex >_random.NextSingle())
				{
					CurrentPlan = Plan.RUN_AWAY;
					_targetPosition = target;
					_runAwayTicks = 3;
				}
				else if(oehlalaigheid > _random.NextSingle())
				{
					// Maatje vinden
					Joekel? mate = ClosestJoekel(joekel);
					if (mate != null)
					{
						CurrentPlan = Plan.REPRODUCE;
						_targetPosition = mate.Position;
						_targetNode = mate;
					}
				}
			} 
		}

		if ((CurrentPlan == Plan.EAT || CurrentPlan == Plan.REPRODUCE) && !GodotObject.IsInstanceValid(_targetNode))
		{
			CurrentPlan = Plan.CHILL;
			_targetNode = null;
			_targetPosition = null;
		} 
	}
	
	private (float drukteIndex, Vector2 target) _compute_introversie_values(Joekel joekel, List<Joekel> joekels, float introversie)
	{
		if (joekels.Count <= 1)
		{
			return (0, Vector2.Zero);
		}
		
		// Hoe "erg" drukte is
		float drukteIndex;
		Vector2 target;

		Vector2 avgLoc = joekels
			.Select(j => j.Position )
			.Aggregate(Vector2.Zero, (v1, v2) => v1 + v2) / joekels.Count; 
		
		if (introversie > 0)
		{
			// Kans op wegbewegen, schaalt met #joekels introversie
			drukteIndex = 0.5f * (joekels.Count - 1.0f) * introversie;
			
			Vector2 relPos = (avgLoc - joekel.Position);
			
			if (relPos.IsZeroApprox())
			{
				return (0, Vector2.Zero);
			}

			target = -relPos * 1_000;
		}
		else
		{
			// Kans op drukte opzoeken, meer extraversie en minder joekels maken de kans hoger om andere joekels te zoeken
			drukteIndex = Math.Abs(introversie) / joekels.Count;

			target = avgLoc;
		}



		
		return (drukteIndex, target);

	}


	public enum Plan
	{
		// Hungery
		EAT,
		// Carnivoor
		HUNT,
		// Just chilling
		CHILL,
		// Joekel hates other joekels and wants to flee
		RUN_AWAY,
		// Oe lalala
		REPRODUCE,
	}

	public bool CheckReproduce()
	{
		if (CurrentPlan == Plan.REPRODUCE)
		{
			CurrentPlan = Plan.CHILL;
			return true;
		} else
		{
			return false;
		}
	}

	private Joekel? ClosestJoekel(Joekel joekel){
		float? closestDistance = null;
		Joekel? closest = null;
		foreach (Node child in joekel.GetParent().GetChildren())
		{
			if (child is Joekel other)
			{
				float distance = joekel.GlobalPosition.DistanceTo(other.GlobalPosition);

				if ((closestDistance == null || distance < closestDistance) && other != joekel)
				{
					closestDistance = distance;
					closest = other;
				}
			}
		}
		return closest;
	}
 
	/**
	 * Takes returns Joekels and Food in the perception range of the Joekel
	 */
	private Tuple<List<Joekel>, List<FoodSource>> getAllRelevantEntitiesInPerceptionRange(Joekel joekel)
	{
		PhysicsDirectSpaceState2D space = joekel.GetParent<World>().GetWorld2D().DirectSpaceState;
		PhysicsShapeQueryParameters2D query = new PhysicsShapeQueryParameters2D();
		query.CollideWithAreas = true;
		query.CollideWithBodies = true;
		query.Shape = new CircleShape2D()
		{
			Radius = joekel._perception
		};

		query.Transform = new Transform2D(0, joekel.Position);
		Array<Dictionary> result = space.IntersectShape(query, maxResults: 2048);
		
		// Extract values from the dict, and only take the Food and Joekels
		List<Joekel> joekels = result.Select(d => d["collider"])
			.Where(c => c.Obj is not null)
			.Select(c => c.Obj)
			.OfType<Joekel>()
			.ToList<Joekel>();
		
		List<FoodSource> foodsources = result.Select(d => d["collider"])
			.Select(c => c.Obj)
			.OfType<Area2dFoodSource>()
			.Select(afs => afs.GetParent<FoodSource>())
			.Where(fs => fs != null)
			.ToList<FoodSource>();
		
		return new(joekels, foodsources);
	}
}
