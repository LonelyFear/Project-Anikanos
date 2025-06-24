using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Godot;
public static class WorldGenerator
{
    public const float HillThreshold = 0.75f;
    public const float MountainThreshold = 0.8f;
    public const float MaxTemperature = 35;
    public const float MinTemperature = -30;
    public const float MaxRainfall = 3500;
    public const float MinRainfall = 50;

    public static Vector2I WorldSize = new Vector2I(360, 180);
    public static float Width;
    public static float Height;
    public static float WorldMult = 2f;
    public static float SeaLevel = 0.6f;
    public static int Seed;

    public static float[,] HeightMap;
    public static float[,] RainfallMap;
    public static float[,] TempMap;
    public static Biome[,] BiomeMap;
    public static string[,] Features; // for denoting special stuff such as oasises, waterfalls, ore veins, etc
    public static Dictionary<Vector2I, Vector2I> FlowDirMap;
    public static float[,] HydroMap;
    public static Random rng;
    public static bool TempDone;
    public static bool RainfallDone;
    public static bool HeightmapDone;
    public static bool WaterDone;
    public static bool WorldExists = false;

    public static void GenerateWorld()
    {
        Init();
        Generate();
        WorldExists = true;
    }
    static void Init()
    {
        WorldExists = false;
        WorldSize = new Vector2I(Mathf.RoundToInt(WorldSize.X * WorldMult), Mathf.RoundToInt(WorldSize.X * WorldMult));
        HydroMap = new float[WorldSize.X, WorldSize.Y];
        Features = new string[WorldSize.X, WorldSize.Y];
        rng = new Random(Seed);
    }
    static void Generate()
    {
        HeightMap = new HeightmapGenerator().GenerateHeightmap();
        TempMap = new TempmapGenerator().GenerateTempMap(2f);
        RainfallMap = new RainfallMapGenerator().GenerateRainfallMap(2f);
        BiomeMap = new BiomeGenerator().GenerateBiomes();
        // TODO: Add water flow simulations
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
        if (value < 0 || value > 1)
        {
            return float.NaN;
        }
        return MinRainfall + Mathf.Pow(value, 2f) * (MaxRainfall - MinRainfall);
    }

    public static Image GetTerrainImage(bool heightmap = false)
    {
        if (!WorldExists)
        {
            return null;
        }
        Image image = Image.CreateEmpty(WorldSize.X, WorldSize.Y, false, Image.Format.Rgb8);
        for (int x = 0; x < WorldSize.X; x++)
        {
            for (int y = 0; y < WorldSize.Y; y++)
            {
                if (heightmap)
                {
                    Color lowFlatColor = Color.Color8(31, 126, 52);
                    Color lowHillColor = Color.Color8(198, 187, 114);
                    Color highHillColor = Color.Color8(95, 42, 22);
                    float hf = (HeightMap[x, y] - SeaLevel) / (1f - SeaLevel);
                    image.SetPixel(x, y, Utility.MultiColourLerp([lowFlatColor, lowHillColor, highHillColor], hf));
                }
                else
                {

                }
            }
        }
        return image;

    }
}