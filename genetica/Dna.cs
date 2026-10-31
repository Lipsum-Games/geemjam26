using Godot;
using System;

public partial class Dna
{
	public const int N = 3;
	public readonly float[,] M = new float[N, N];
	[Export] public float MutationFactor = 0.1F;

	public static Dna Random()
	{
		var d = new Dna();
		for (int r = 0; r < N; r++)
			for (int c = 0; c < N; c++)
				d.M[r, c] = (float)GD.RandRange(-1.0, 1.0);
		return d;
	}
	
	public static Dna Combine(Dna parent1, Dna parent2)
	{
		var d = new Dna();
		var choice = 0.0f;
		for (int r = 0; r < N; r++){
			for (int c = 0; c < N; c++){
				choice = (float) 2*GD.Randf();
				if(choice < 1.0f){
					d.M[r, c] = parent1.M[r, c];
				} else {
					d.M[r, c] = parent2.M[r, c];
				}
			}
		}
		return d.Noisified();
	}

	public Dna Noisified(){
		var d = new Dna();
		for (int r = 0; r < N; r++)
			for (int c = 0; c < N; c++)
				d.M[r, c] = (1 - MutationFactor) * M[r,c] + (float)GD.RandRange(-1.0, 1.0) * MutationFactor;
		return d;
	} 

	// d[1, 2] -> 6
	public float this[int r, int c] => M[r, c];

	// Row(0) -> 1+2+3
	public float Row(int r)
	{
		float s = 0;
		for (int c = 0; c < N; c++) s += M[r, c];
		return s;
	}

	// Col(2) -> 3+6+9
	public float Col(int c)
	{
		float s = 0;
		for (int r = 0; r < N; r++) s += M[r, c];
		return s;
	}

	// 1+5+9
	public float Diag()
	{
		float s = 0;
		for (int i = 0; i < N; i++) s += M[i, i];
		return s;
	}

	// 3+5+7
	public float AntiDiag()
	{
		float s = 0;
		for (int i = 0; i < N; i++) s += M[i, N - 1 - i];
		return s;
	}

	// Cells((0,0), (2,1)) -> 1+8
	public float Cells(params (int r, int c)[] cells)
	{
		float s = 0;
		foreach (var (r, c) in cells) s += M[r, c];
		return s;
	}
}
