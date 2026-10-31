using Godot;
using System;

public partial class Dna : Node
{
    public const int N = 3;
    public readonly float[,] M = new float[N, N];

    public static Dna Random()
    {
        var d = new Dna();
        for (int r = 0; r < N; r++)
            for (int c = 0; c < N; c++)
                d.M[r, c] = GD.Randfn(0f, 1f);
        return d;
    }
}
