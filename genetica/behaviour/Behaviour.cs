#nullable enable
using System;
using System.Linq;
using Godot;

namespace Geemjam26ShipsumEvolve.genetica;

public class Behaviour
{
    private Random _random = new Random();
    private Plan _currentPlan = Plan.CHILL; 
    private Vector2? _targetPosition = null;
    private FoodSource? _targetFoodSource = null;
    
    

    // Besluit wat de joekel gaat doen en returnt een tuple van richting (radialen) en gewenste snelheid
    public Tuple<float, float> DoeDingen(Joekel joekel)
    {
        _besluitPlan(joekel);

        float direction = 0.0f;
        float speed = 0.0f;
        
        if (_currentPlan == Plan.CHILL)
        {
            speed = 0.1f;
            direction = (float)(_random.NextSingle() * Math.Tau);
        } else if (_targetPosition is not null)
        {
            float distance = _targetPosition.Value.DistanceTo(joekel.Position);
            if (distance < 1.0f)
            {
                speed = distance;
            }
            else
            {
                speed = 1.0f;
            }

            direction = joekel.Position.AngleToPoint(_targetPosition.Value);
        }
        

        return new Tuple<float, float>(direction, speed);
    }

    private void _besluitPlan(Joekel joekel)
    {
        var introversie = Karaktereigenschappen.Introversie.Van(joekel.Dna);
        var trek = Karaktereigenschappen.Trek.Van(joekel.Dna) * (joekel.HungerMax - joekel.HungerMin) + joekel.HungerMin;
        var oehlalaigheid = Karaktereigenschappen.Oehlalalaigheid.Van(joekel.Dna);

        if (_currentPlan == Plan.EAT && !GodotObject.IsInstanceValid(_targetFoodSource))
        {
            _currentPlan = Plan.CHILL;
            _targetFoodSource = null;
            _targetPosition = null;
        } 
        
        // Voedsel is heeeeeeel belangrijk
        if (_currentPlan != Plan.EAT && joekel.GetHunger() > trek)
        {   
            // TODO: Get a food source
            FoodSource? food = joekel.GetTree()
                .GetNodesInGroup("FoodSource")
                .Where(node => node.GetParent() is FoodSource)
                .Select(node => node.GetParent<FoodSource>())
                .FirstOrDefault(fs => fs.Position.DistanceTo(joekel.Position) <= 600.0f);

            if (food is not null)
            {
                // Go to foodsource
                _targetPosition = food.Position;
                _currentPlan = Plan.EAT;
            }

            return;
        } if (_currentPlan == Plan.EAT && joekel.GetHunger() < joekel.HungerMin * 1.1f)
        {
            _currentPlan = Plan.CHILL;
            _targetFoodSource = null;
            _targetPosition = null;
        }
        
        if (_currentPlan == Plan.CHILL)
        {
            if (_random.NextSingle() < oehlalaigheid * 0.05f)
            {
                _currentPlan = Plan.REPRODUCE;
            } else if (introversie * 0.01f > _random.NextSingle())
            {
                // TODO: Conditionz
                _currentPlan = Plan.RUN_AWAY;
                _targetPosition = new Vector2(_random.NextSingle(), _random.NextSingle()) * World.WorldSize;
            }
        }

        if (_currentPlan == Plan.RUN_AWAY && _targetPosition is not null && joekel.Position.DistanceTo(_targetPosition.Value) < 2.0f)
        {
            _currentPlan = Plan.CHILL;
            _targetPosition = null;
        }
    }


    enum Plan
    {
        // Hungery
        EAT,
        // Just chilling
        CHILL,
        // Joekel hates other joekels and wants to flee
        RUN_AWAY,
        // Oe lalala
        REPRODUCE,
    }

    public bool CheckReproduce()
    {
        if (_currentPlan == Plan.REPRODUCE)
        {
            _currentPlan = Plan.CHILL;
            return true;
        } else
        {
            return false;
        }
    }
}