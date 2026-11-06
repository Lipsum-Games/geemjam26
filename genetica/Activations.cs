using Godot;
using System;
using System.Linq;

public partial class Activations
{
	public static float Sigmoid(float x) => 1f / (1f + Mathf.Exp(-x));
	public static float Tanh(float x) => Mathf.Tanh(x);
	public static float Relu(float x) => Mathf.Max(0f, x);
	public static float Softplus(float x) => Mathf.Log(1f + Mathf.Exp(x));
	// range schaalt een getal van bijvoorbeeld 0-1 naar 20-30.
	public static float Range(float min, float max, float t) => Mathf.Lerp(min, max, t);

	// Softmax is soort van een multi-variable sigmoid.
	// Gegeven een array aan floats, maakt softmax er een array van met een som van 1,
	//	waarin elk element een waarde heeft afhankelijk van hoe hoog hun input waarde is
	public static float[] Softmax(float[] floats)
	{
		float sum = floats.Select(Mathf.Exp).Sum();
		float[] outp = new float[floats.Length];
		for (int i = 0; i < outp.Length; i++)
		{
			outp[i] = Mathf.Exp(floats[i]) / sum;
		}
		return outp;
	}
}
