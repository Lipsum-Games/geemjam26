using System;
using Godot;

namespace Geemjam26ShipsumEvolve.neuralnets;

/**
 * Converter for the input of the neural net
 */
public record struct SimpleNetInputData
{
    public static readonly int INPUT_SIZE = 39;
    
    // 2 inputs, 2 total
    public required Vector2 WorldPosition;
    // 2 inputs, 4 total
    public required Vector2 Velocity;
    // 2 inputs (cos(h), sin(h)), 6 total
    public required float Heading;
    // Joekel input is 1 (whether there is valid data) + 2 (relpos) + 2 (relspeed) + 9 (dna) = 14
    // 14 inputs, 20 total
    public required JoekelData? BestMate;
    // 14 inputs, 34 total
    public required JoekelData? BestPrey;
    // Food input is 3 (exists, relpos)
    // 3 inputs, 37 total
    public required FoodSourceData? BestFood;
    // 1 input, 38 total
    public required float Hunger;
    // 1 input, 39 total
    public required float Temperature;

    public void FillInputArray(float[] array)
    {
        int offset = 0;
        offset = FillWorldCoord(array, offset);
        offset = FillVelocity(array, offset);
        offset = FillHeading(array, offset);
        offset = FillJoekelData(array, offset, BestMate);
        offset = FillJoekelData(array, offset, BestPrey);
        offset = FillFoodData(array, offset, BestFood);
        offset = FillFloat(array, offset, Hunger);
        offset = FillFloat(array, offset, Temperature);

        if (offset != INPUT_SIZE)
        {
            throw new Exception("Offset should match predicted input size, but was " + offset + " != "  + INPUT_SIZE);
        }
    }

    private Vector2 OffsetAndRot(Vector2 position)
    {
        Vector2 relPos = position -  WorldPosition;
        return relPos.Rotated(-Heading);
    }

    private int FillWorldCoord(float[] array, int offset)
    {
        array[offset] = WorldPosition.X / World.WorldSize.X;
        array[offset + 1] = WorldPosition.Y / World.WorldSize.Y;
        return offset + 2;
    }
    
    private int FillVelocity(float[] array, int offset)
    {
        array[offset] = Velocity.X / 300.0f;
        array[offset + 1] = Velocity.Y / 300.0f;
        return offset + 2;
    }

    private int FillHeading(float[] array, int offset)
    {
        Vector2 heading = Vector2.FromAngle(Heading);
        array[offset] = heading.X;
        array[offset + 1] = heading.Y;
        return offset + 2;
    }

    // Joekel input is 1 (whether there is valid data) + 2 (relpos) + 2 (velocity) + 9 (dna) = 14
    private int FillJoekelData(float[] array, int offset, JoekelData? data)
    {
        if (data is null)
        {
            // "NO_DATA"
            array[offset] = -1;
        }
        else
        {
            array[offset] = 1;
            
            // TODO: Maybe rotate relative as well?
            Vector2 relPos = OffsetAndRot(data.Value.Position) / 500.0f;
            array[offset + 1] = relPos.X;
            array[offset + 2] = relPos.Y;
            
            Vector2 vel = (data.Value.Velocity.Rotated(-Heading)) / 300.0f;
            array[offset + 3] = vel.X;
            array[offset + 4] = vel.Y;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    // DNA values are already more or less normally distributed
                    array[offset + 5 + i + 3*j] = data.Value.Dna[i, j];
                }
            }
        }
        return offset + 14;
    }

    private int FillFoodData(float[] array, int offset, FoodSourceData? data)
    {
        if (data is null)
        {
            array[offset] = -1;
        }
        else
        {
            array[offset] = 1;
            
            Vector2 relPos = OffsetAndRot(data.Value.Position) / 500.0f;
            array[offset + 1] = relPos.X;
            array[offset + 2] = relPos.Y;
        }
        return offset + 3;
    }
    

    private int FillFloat(float[] array, int offset, float value)
    {
        array[offset] = value;
        return offset + 1;
    }
}