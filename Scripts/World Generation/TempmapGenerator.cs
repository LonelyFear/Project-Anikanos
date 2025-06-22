using Godot;
using System;

public class TempmapGenerator
{
    WorldGenerator w;
    float[,] GenerateTempMap(WorldGenerator w, float scale){
        this.w = w;
        float[,] map = new float[w.WorldSize.X, w.WorldSize.Y];
        FastNoiseLite noise = new FastNoiseLite(w.rng.Next(-99999, 99999));
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFractalOctaves(8);
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        float averageTemp = 0;
        float[,] falloff = Falloff.GenerateFalloffMap(w.WorldSize.X, w.WorldSize.Y, false, 1, 1.1f);
        for (int x = 0; x < w.WorldSize.X; x++){
            for (int y = 0; y < w.WorldSize.Y; y++)
            {
                float noiseValue = Mathf.InverseLerp(-1, 1, noise.GetNoise(x / scale, y / scale));
                map[x, y] = Mathf.Lerp(1 - falloff[x, y], noiseValue, 0.15f);
                float heightFactor = (w.HeightMap[x, y] - w.SeaLevel) / (1f - w.SeaLevel);
                if (heightFactor > 0)
                {
                    map[x, y] -= heightFactor * 0.3f;
                }
                map[x, y] = Mathf.Clamp(map[x, y], 0, 1);
                averageTemp += WorldGenerator.GetUnitTemp(map[x, y]);
            }
        }
        GD.Print((averageTemp/(w.WorldSize.X*w.WorldSize.Y)).ToString("Average: 0.0") + " C");
        return map;
    } 
}
