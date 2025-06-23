using System;
using Godot;

public class RainfallMapGenerator
{
    float[,] map;

    public float[,] GenerateRainfallMap(WorldGenerator w, float scale){
        map = new float[w.WorldSize.X, w.WorldSize.Y];
        FastNoiseLite noise = new FastNoiseLite();
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFractalOctaves(8);
        noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        noise.SetSeed(w.rng.Next(-99999, 99999));
        float minVal = float.PositiveInfinity;
        for (int y = 0; y < w.WorldSize.Y; y++)
        {
            for (int x = 0; x < w.WorldSize.X; x++)
            {
                map[x, y] = Mathf.InverseLerp(-0.5f, 0.5f, noise.GetNoise(x/scale, y/scale));
                if (noise.GetNoise(x, y) < minVal)
                {
                    minVal = noise.GetNoise(x, y);
                }
            }
        }
        GD.Print("Min Rainfall Value: " + minVal);
        return map;
    }
}