using System;
using Geemjam26ShipsumEvolve.genetica;

namespace Geemjam26ShipsumEvolve.neuralnets;

/**
 * SimpleNet is, as titled, the simplest "brain" we can give the joekels.
 * It uses the pre-computed values used by the normal behavior logic as input
 */
public class SimpleNet
{
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
            2, 
            DenseLayer.GenRandomWeights(INPUT_SIZE, hSize),
            null, // Totally unbiased :3
            v => v // No activation, we'll process the outputs later
        );
    }

    public SimpleNet(DenseLayer[] layers)
    {
        this.layers = layers;
    }

    public (float richtingDev, float kracht) Process(SimpleNetInputData data)
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
        return (richtingDev, kracht);
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