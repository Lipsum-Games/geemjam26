using System;
using System.Linq;
using Geemjam26ShipsumEvolve.genetica;
using Godot;

namespace Geemjam26ShipsumEvolve.neuralnets.simplenet;

/**
 * SimpleNet is, as titled, the simplest "brain" we can give the joekels.
 * It uses the pre-computed values used by the normal behavior logic as input
 */
public class SimpleNet
{
    private static readonly Plan[] plans = [Plan.NEURAL_NETWORK, Plan.REPRODUCE, Plan.HUNT];

    public static readonly int MEMORY_SIZE = 4;
    public static readonly int INPUT_SIZE = SimpleNetInputData.INPUT_SIZE + MEMORY_SIZE + 1;
    public static readonly int OUTPUT_SIZE = 2 + plans.Length + MEMORY_SIZE;
    
    private readonly DenseLayer[] _layers = new  DenseLayer[3];
    private float[] _memory = new float[4];
    
    public SimpleNet(int hSize)
    {
        _layers[0] = new DenseLayer(
            INPUT_SIZE, 
            hSize, 
            DenseLayer.GenRandomWeights(INPUT_SIZE, hSize, gain: 0.5f),
            DenseLayer.GenEmptyBiases(hSize),
            Activations.Tanh
        );
        
        _layers[1] = new DenseLayer(
            hSize, 
            hSize, 
            DenseLayer.GenRandomWeights(hSize, hSize, gain: 0.5f),
            DenseLayer.GenEmptyBiases(hSize),
            Activations.Tanh
        );
        
        _layers[2] = new DenseLayer(
            hSize, 
            OUTPUT_SIZE, 
            DenseLayer.GenRandomWeights(INPUT_SIZE, hSize, gain: 0.5f),
            null, //DenseLayer.GenEmptyBiases(OUTPUT_SIZE),
            v => v // No activation, we'll process the outputs later
        );

        PreconditionNetwork();
    }

    private void PreconditionNetwork()
    {
       
    }

    public SimpleNet(DenseLayer[] layers)
    {
        _layers = layers;
    }

    public (float richtingDev, float kracht, Plan plan) Process(SimpleNetInputData data)
    {
        float[] inp = new float[INPUT_SIZE];
        data.FillInputArray(inp);
        int offset = SimpleNetInputData.INPUT_SIZE;
        // Give the network something of randomness to enjoy
        inp[offset] = (float) GD.Randfn(0f, 1f);
        offset += 1;

        for (int i = 0; i < MEMORY_SIZE; i++)
        {
            inp[offset + i] = _memory[i];
        }

        float[] v = inp;
        for (int i = 0; i < _layers.Length; i++)
        {
            v = _layers[i].Forward(v);
        }

        if (v.Any(val => !Mathf.IsFinite(val)))
        {
           ;
            throw new Exception("Array contains non-finite values! " + string.Join(", ", v) + "\n with weights: " +  string.Join(", ", _layers[0].Weights));
        }
        

        float richtingDev = Activations.Tanh(v[0] * 0.3f) * Mathf.Pi;
        float kracht = Activations.Sigmoid(v[1]);


        for (int i = 0; i < MEMORY_SIZE; i++)
        {
            _memory[i] = Activations.Tanh(v[2 + i]);
        }
        
        float[] planProbs = Activations.Softmax(v[(2 + MEMORY_SIZE)..]);

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
        DenseLayer[] outLayers = new DenseLayer[_layers.Length];
        for (int i = 0; i < _layers.Length; i++)
        {
            outLayers[i] = _layers[i].Reproduce(other._layers[i], reproductionValues);
        }

        return new SimpleNet(outLayers);
    }
}