using System;
using System.Collections;
using System.Collections.Generic;
using Godot;
public class WorldGenerator
{
    public const float HillThreshold = 0.75f;
    public const float MountainThreshold = 0.8f;
    public const float MaxTemperature = 35;
    public const float MinTemperature = -30;
    public const float MaxRainfall = 3500;
    public const float MinRainfall = 50;

    public Vector2I WorldSize = new Vector2I(360, 180);
    public float Width;
    public float Height;
    public float WorldMult = 2f;
    public float SeaLevel = 0.6f;
    public int Seed;

    public float[,] HeightMap;
    public float[,] RainfallMap;
    public float[,] TempMap;
    public string[,] Features; // for denoting special stuff such as oasises, waterfalls, ore veins, etc
    public Dictionary<Vector2I, Vector2I> FlowDirMap;
    public float[,] HydroMap;
    public Random rng;

    public void Init()
    {
        WorldSize = new Vector2I(Mathf.RoundToInt(WorldSize.X * WorldMult), Mathf.RoundToInt(WorldSize.X * WorldMult));
        HydroMap = new float[WorldSize.X, WorldSize.Y];
        Features = new string[WorldSize.X, WorldSize.Y];
        rng = new Random(Seed);
    }
    public void GenerateWorld()
    {
        HeightMap = new HeightmapGenerator().GenerateHeightmap(this);
    }
    public static float GetUnitTemp(float value)
    {
        if (value < 0 || value > 1)
        {
            return float.NaN;
        }
        return MinTemperature + Mathf.Pow(value, 0.7f) * (MaxTemperature - MinTemperature);
    }
    public static float GetUnitRainfall(float value)
    {
        if (value < 0 || value > 1) {
            return float.NaN;
        }
        return MinRainfall + Mathf.Pow(value, 2f) * (MaxRainfall - MinRainfall);
    }
}