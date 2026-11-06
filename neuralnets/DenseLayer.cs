#nullable enable
using System;
using System.Collections.Generic;
using Geemjam26ShipsumEvolve.genetica;
using Godot;

namespace Geemjam26ShipsumEvolve.neuralnets;

/**
 * A simple (and inefficient) Dense layer of a neural network.
 * Simply put, a Dense layer can be seen a layer of neurons, with every neuron connected to the input values.
 * You input a vector/array of values, and each neuron multiplies all of these with their own set of weights (one for each input),
 *  sums them together, adds an optional bias value, and applies an activation function (i.e. ReLU, TanH, Sigmoid).
 */
public class DenseLayer
{
    public readonly int InputLength;
    public readonly int Neurons;
    public readonly float[] Weights;
    public readonly float[]? Biases;
    public readonly Func<float, float> ActivationFunction;

    
    public DenseLayer(int inputLength, int neurons, float[] weights, float[]? biases, Func<float, float> activationFunction)
    {
        InputLength = inputLength;
        Neurons = neurons;
        Weights = weights;
        Biases = biases;
        ActivationFunction = activationFunction;
    }


    public float[] Forward(float[] inputs)
    {
        if (inputs.Length != InputLength)
        {
            throw new ArgumentException("Input array must have length " + InputLength + " but got " + inputs.Length);
        }

        float[] outputs = new float[Neurons];

        for (int i = 0; i < InputLength; i++)
        {
            int offset = i * Neurons;
            for (int o = 0; o < Neurons; o++)
            {
                outputs[o] += inputs[i] * Weights[offset + o];
            }
        }

        if (Biases != null)
        {
            for (int o = 0; o < Neurons; o++)
            {
                outputs[o] += Biases[o];
            }
        }

        for (int o = 0; o < Neurons; o++)
        {
            outputs[o] = ActivationFunction(outputs[o]);
        }
        return outputs;
    }

    public static float[] GenRandomWeights(int inputLength, int neurons)
    {
        // Kaiming/He init
        
        float[] weights = new float[inputLength * neurons];
        
        for (int i = 0; i < inputLength * neurons; i++)
        {
            weights[i] = (float) GD.Randfn(0, Mathf.Sqrt2 * Mathf.Sqrt(6f / (inputLength + neurons)));
        }
        return weights;
    }

    public static float[] GenEmptyBiases(int neurons)
    {
        return new float[neurons];
    }

    public DenseLayer Reproduce(DenseLayer other, ReproductionValues reproductionValues)
    {
        if ((other.Biases == null) != (Biases == null))
        {
            throw new ArgumentException("Cannot reproduce between a dense layer that has a bias and one that doesn't!");
        }
        
        if ((other.InputLength != InputLength) || (other.Neurons != Neurons))
        {
            throw new ArgumentException(
                "Cannot reproduce between Dense layers of different sizes! Got sizes " + 
                (InputLength, Neurons) + " and " + (other.InputLength, Neurons)
            );
        }
        
        // TODO: Bias
        float[] weightsOut = reproduceBetweenArrays(Weights, other.Weights, reproductionValues);
        float[]? biasesOut = null;
        if (Biases != null && other.Biases != null)
        {
            biasesOut = reproduceBetweenArrays(Biases,  other.Biases, reproductionValues);
        }
        return new DenseLayer(InputLength, Neurons, weightsOut, biasesOut, ActivationFunction);
    }

    private float[] reproduceBetweenArrays(float[] arr1, float[] arr2, ReproductionValues reproductionValues)
    {
        if (arr1.Length != arr2.Length)
        {
            throw new ArgumentException("Arrays must have same length but got " + arr1.Length + " and " + arr2.Length);
        }
        
        float[] outputs = new float[arr1.Length];
        bool flipping = GD.Randf() < reproductionValues.FlipProbability;
        
        for (int i = 0; i < arr1.Length; i++)
        {
            if (flipping && GD.Randf() < reproductionValues.FlipProbability)
            {
                outputs[i] = Weights[i] * -1.0f;
            }
            else
            {
                if (GD.Randf() < reproductionValues.FlipProbability)
                {
                    // Rare In-between value
                    float w = GD.Randf();
                    outputs[i] = arr1[i] * w +  arr2[i] * (1.0f - w);
                } else if (GD.Randf() > 0.5)
                {
                    outputs[i] = arr1[i];
                }
                else
                {
                    outputs[i] = arr2[i];
                }
            }
        }
        return outputs;
    }
}