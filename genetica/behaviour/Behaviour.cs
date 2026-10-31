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

            direction = joekel.GetPosition().AngleTo(_targetPosition.Value);
        }
        

        return new Tuple<float, float>(direction, speed);
    }

    private void _besluitPlan(Joekel joekel)
    {
        var introversie = Karaktereigenschappen.Introversie.Van(joekel.Dna);
        var trek = Karaktereigenschappen.Trek.Van(joekel.Dna);
        var oehlalaigheid = Karaktereigenschappen.Oehlalalaigheid.Van(joekel.Dna);

        // Voedsel is heeeeeeel belangrijk
        if (_currentPlan != Plan.EAT && joekel.GetHunger() > trek)
        {   
            // TODO: Get a food source
            FoodSource? food = joekel.GetParent<World>().GetChildren()
                .Where(node => node is FoodSource)
                .OfType<FoodSource>()
                .FirstOrDefault<FoodSource?>();
            
            if (food is not null)
            {
                // Go to foodsource
                _targetPosition = food.Position;
                _currentPlan =  Plan.EAT;
            }

            return;
        } if (_currentPlan == Plan.EAT && joekel.GetHunger() < joekel.HungerMin * 1.1f)
        {
            _currentPlan = Plan.CHILL;
            _targetPosition = null;
        }
        
        if (_currentPlan == Plan.CHILL)
        {
            if (_random.NextSingle() < oehlalaigheid)
            {
                _currentPlan = Plan.REPRODUCE;
            } else 
            {
                // TODO: Condition
                _currentPlan = Plan.RUN_AWAY;
                _targetPosition = new Vector2(_random.NextSingle(), _random.NextSingle()) * joekel.GetParent<World>().WorldSize;
            }
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
    
}