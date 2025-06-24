using Godot;
using System;

public class TempmapGenerator
{
    float[,] map;
    public float[,] GenerateTempMap(float scale){
        map = new float[WorldGenerator.WorldSize.X, WorldGenerator.WorldSize.Y];
        FastNoiseLite noise = new FastNoiseLite(WorldGenerator.rng.Next(-99999, 99999));
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFractalOctaves(8);
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        float averageTemp = 0;
        float[,] falloff = Falloff.GenerateFalloffMap(WorldGenerator.WorldSize.X, WorldGenerator.WorldSize.Y, false, 1, 1.1f);
        for (int x = 0; x < WorldGenerator.WorldSize.X; x++){
            for (int y = 0; y < WorldGenerator.WorldSize.Y; y++)
            {
                float noiseValue = Mathf.InverseLerp(-1, 1, noise.GetNoise(x / scale, y / scale));
                map[x, y] = Mathf.Lerp(1 - falloff[x, y], noiseValue, 0.15f);
                float heightFactor = (WorldGenerator.HeightMap[x, y] - WorldGenerator.SeaLevel) / (1f - WorldGenerator.SeaLevel);
                if (heightFactor > 0)
                {
                    map[x, y] -= heightFactor * 0.3f;
                }
                map[x, y] = Mathf.Clamp(map[x, y], 0, 1);
                averageTemp += WorldGenerator.GetUnitTemp(map[x, y]);
            }
        }
        GD.Print((averageTemp/(WorldGenerator.WorldSize.X*WorldGenerator.WorldSize.Y)).ToString("Average: 0.0") + " C");
        return map;
    } 
}
