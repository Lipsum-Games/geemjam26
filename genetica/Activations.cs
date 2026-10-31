using Godot;
using System;

public partial class Activations
{
	public static float Sigmoid(float x) => 1f / (1f + Mathf.Exp(-x));
	public static float Tanh(float x) => Mathf.Tanh(x);
	public static float Relu(float x) => Mathf.Max(0f, x);
	public static float Softplus(float x) => Mathf.Log(1f + Mathf.Exp(x));
	// range schaalt een getal van bijvoorbeeld 0-1 naar 20-30.
    public static float Range(float min, float max, float t) => Mathf.Lerp(min, max, t);
}
