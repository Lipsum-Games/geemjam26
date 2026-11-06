using System;
using Geemjam26ShipsumEvolve.genetica;
using Godot;

namespace Geemjam26ShipsumEvolve.neuralnets;

/**
 * SimpleNet is, as titled, the simplest "brain" we can give the joekels.
 * It uses the pre-computed values used by the normal behavior logic as input
 */
public class SimpleNet
{
    private static readonly Plan[] plans = [Plan.NEURAL_NETWORK, Plan.REPRODUCE, Plan.HUNT];
    public static readonly int INPUT_SIZE = SimpleNetInputData.INPUT_SIZE + 0;
    
    private readonly DenseLayer[] layers = new  DenseLayer[2];
    
    public SimpleNet(int hSize)
    {
        layers[0] = new DenseLayer(
            INPUT_SIZE, 
            hSize, 
            DenseLayer.GenRandomWeights(INPUT_SIZE, hSize),
            DenseLayer.GenEmptyBiases(hSize),
            Activations.Relu
        );
        
        layers[1] = new DenseLayer(
            hSize, 
            5, 
            DenseLayer.GenRandomWeights(INPUT_SIZE, hSize),
            null, //DenseLayer.GenEmptyBiases(5),
            v => v // No activation, we'll process the outputs later
        );
    }

    public SimpleNet(DenseLayer[] layers)
    {
        this.layers = layers;
    }

    public (float richtingDev, float kracht, Plan plan) Process(SimpleNetInputData data)
    {
        float[] inp = new float[INPUT_SIZE];
        data.FillInputArray(inp);

        float[] v = inp;
        for (int i = 0; i < layers.Length; i++)
        {
            v = layers[i].Forward(v);
        }

        float richtingDev = Activations.Tanh(v[0]) * 1f;
        float kracht = Activations.Sigmoid(v[1]);
        float[] planProbs = Activations.Softmax(v[2..]);

        float val = GD.Randf();
        float sum = 0f;
        Plan plan = Plan.NEURAL_NETWORK;

        for (int i = 0; i < plans.Length; i++)
        {
            sum += planProbs[i];
            if (val < sum)
            {
                plan = plans[i];
                break;
            }
        }
        
        return (richtingDev, kracht, plan);
    }

    public SimpleNet Reproduce(SimpleNet other, ReproductionValues reproductionValues)
    {
        DenseLayer[] outLayers = new DenseLayer[2];
        for (int i = 0; i < layers.Length; i++)
        {
            outLayers[i] = layers[i].Reproduce(other.layers[i], reproductionValues);
        }

        return new SimpleNet(outLayers);
    }
}