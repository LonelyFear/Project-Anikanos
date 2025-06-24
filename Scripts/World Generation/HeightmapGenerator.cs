using Godot;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;

public class HeightmapGenerator
{
    public static float oceanDepth = 0.45f;
    float[,] heightmap;
    bool[,] midpoints;
    int gridSizeX = 16;
    int gridSizeY = 16;
    int ppcx;
    int ppcy;
    TerrainTile[,] tiles;
    List<Vector2I> offshore = new List<Vector2I>();
    List<Plate> plates = new List<Plate>();
    List<VoronoiRegion> continentalRegions = new List<VoronoiRegion>();
    List<VoronoiRegion> voronoiRegions = new List<VoronoiRegion>();
    Vector2I worldSize;
    float worldMult;
    Dictionary<Vector2I, VoronoiRegion> points;
    static Random rng = new Random();

    // Todo: Make World Generation Manager
    public float[,] GenerateHeightmap()
    {
        rng = new Random(WorldGenerator.Seed);
        worldSize = WorldGenerator.WorldSize;
        worldMult = WorldGenerator.WorldMult;
        heightmap = new float[worldSize.X, worldSize.Y];
        midpoints = new bool[worldSize.X, worldSize.Y];
        tiles = new TerrainTile[worldSize.X, worldSize.Y];
        points = GeneratePoints();
        GenerateRegions(6);
        GenerateContinents();
        GetDistances();
        try
        {
            GeneratePlates(10);
        }
        catch (Exception e)
        {
            GD.PushError(e);
        }
        GetTectonicPressure();
        AdjustHeightMap();
        TectonicEffects();
        GD.Print("Offshore tiles: " + offshore.Count());
        return heightmap;
    }

    public void TectonicEffects()
    {
        FastNoiseLite widthNoise = new FastNoiseLite(rng.Next(-99999, 99999));
        widthNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        widthNoise.SetFractalOctaves(8);
        widthNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        FastNoiseLite heightNoise = new FastNoiseLite(rng.Next(-99999, 99999));
        heightNoise.SetFractalType(FastNoiseLite.FractalType.Ridged);
        heightNoise.SetFractalOctaves(8);
        heightNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        for (int x = 0; x < worldSize.X; x++)
        {
            for (int y = 0; y < worldSize.Y; y++)
            {
                float minWidth = 6f + (widthNoise.GetNoise(x,y) * 3f);
                TerrainTile tile = tiles[x, y];
                if (tile.boundaryDist <= minWidth)
                {
                    float noiseValue = Mathf.InverseLerp(-1, 1, heightNoise.GetNoise(x, y));
                    float boundaryFactor = 1f - (tile.boundaryDist / minWidth);
                    TerrainTile boundary = tile.nearestBoundary;
                    if (boundary.region.continental)
                    {
                        if (boundary.collisionContinental)
                        {
                            if (boundary.pressure > 0)
                            {
                                heightmap[x, y] += 0.3f * Mathf.Pow(boundaryFactor, 2) * Mathf.Clamp(boundary.pressure, 0, 1) * Mathf.Lerp(0.5f, 1f, noiseValue);
                            }
                        }
                        else
                        {        
                            // TODO                               
                        }
                    }
                    else
                    {
                        // TODO
                    }
                }
            }
        }        
    }
    public void GetTectonicPressure()
    {
        for (int x = 0; x < worldSize.X; x++)
        {
            for (int y = 0; y < worldSize.Y; y++)
            {
                TerrainTile tile = tiles[x, y];
                if (!tile.fault)
                {
                    continue;
                }
                int otherTiles = 0;
                for (int dx = -3; dx < 4; dx++)
                {
                    for (int dy = -3; dy < 4; dy++)
                    {
                        Vector2I testPos = new Vector2I(Mathf.PosMod(x + dx, worldSize.X), Mathf.PosMod(y + dy, worldSize.Y));
                        TerrainTile next = tiles[testPos.X, testPos.Y];
                        if (next.region.plate != tile.region.plate)
                        {
                            otherTiles++;
                            Vector2 relativeVel = tile.region.plate.dir - next.region.plate.dir;
                            if (relativeVel.Length() * relativeVel.Normalized().Dot(testPos - new Vector2I(x, y)) < 0)
                            {
                                tile.pressure += 0.5f * relativeVel.Length();
                            }
                            else
                            {
                                tile.pressure += -0.5f * relativeVel.Length();
                            }
                            tile.collisionContinental = next.region.continental;
                        }
                    }
                }
                tile.pressure /= otherTiles;
                if (tile.pressure >= 1)
                {
                    tile.convergent = true;
                }
            }
        }
    }
    public void GeneratePlates(int amount)
    {
        int nonPlateRegions = voronoiRegions.Count();
        int platesToGenerate = Mathf.Clamp(amount, 0, gridSizeX * gridSizeY);
        int attempts = 99999;
        while (platesToGenerate > 0)
        {
            VoronoiRegion region = voronoiRegions.PickRandom(rng);
            if (region.continental && region.plate == null)
            {
                Plate plate = new Plate()
                {
                    dir = new Vector2(Mathf.Lerp(-2, 2,rng.NextSingle()), Mathf.Lerp(-2, 2,rng.NextSingle()))
                };
                region.plate = plate;
                plates.Add(plate);
                nonPlateRegions--;
                platesToGenerate--;
            }
        }
        GD.Print("Plates generated");
        attempts = 5000;
        while (nonPlateRegions > 0 && attempts > 0)
        {
            attempts--;
            foreach (VoronoiRegion region in voronoiRegions)
            {
                if (region.plate == null)
                {
                    continue;
                }
                VoronoiRegion border = region.borderingRegions.PickRandom(rng);
                if (border.plate == null)
                {
                    border.plate = region.plate;
                    nonPlateRegions--;
                }
            }
        }
        GD.Print("Plates grown");
        // Checks if tiles are on a plate border
        for (int x = 0; x < worldSize.X; x++)
        {
            for (int y = 0; y < worldSize.Y; y++)
            {
                VoronoiRegion region = tiles[x, y].region;
                Vector2I pos = new Vector2I(x, y);
                for (int dx = -1; dx < 2; dx++)
                {
                    for (int dy = -1; dy < 2; dy++)
                    {
                        if ((dx == 0 && dy == 0) || tiles[x, y].fault)
                        {
                            continue;
                        }
                        Vector2I next = new Vector2I(Mathf.PosMod(pos.X + dx, worldSize.X), Mathf.PosMod(pos.Y + dy, worldSize.Y));
                        VoronoiRegion neighbor = tiles[next.X, next.Y].region;
                        if (neighbor.plate != region.plate)
                        {
                            tiles[x, y].fault = true;
                            region.boundaryTiles.Add(pos);
                        }
                    }
                }
            }
        }
        // Gets Distance From Nearest Boundary
        for (int x = 0; x < worldSize.X; x++)
        {
            for (int y = 0; y < worldSize.Y; y++)
            {
                List<Vector2I> tilesToCheck = [.. tiles[x, y].region.boundaryTiles];
                foreach (VoronoiRegion r in tiles[x, y].region.borderingRegions) {
                    tilesToCheck.AddRange(r.boundaryTiles);
                }
                TerrainTile tile = tiles[x, y];
                Vector2I pos = new Vector2I(x, y);
                PriorityQueue<Vector2I, float> distances = new PriorityQueue<Vector2I, float>();
                if (tilesToCheck.Count > 0)
                {
                    foreach (Vector2I next in tilesToCheck)
                    {
                        distances.Enqueue(next, pos.WrappedDistanceSquaredTo(next, worldSize));
                    }
                    Vector2I closestPos = distances.Dequeue();
                    tile.nearestBoundary = tiles[closestPos.X, closestPos.Y];
                    tile.boundaryDist = pos.WrappedDistanceTo(closestPos, worldSize);                    
                }

            }
        }   
    }
    public void GenerateContinents()
    {
        int attempts = 2000;
        float continentalRegionPercentage = 0.4f;
        while (continentalRegions.Count < Mathf.RoundToInt(voronoiRegions.Count * continentalRegionPercentage) && attempts > 0)
        {
            attempts--;
            foreach (VoronoiRegion region in continentalRegions.ToArray())
            {
                VoronoiRegion border = region.borderingRegions[rng.Next(0, region.borderingRegions.Count - 1)];
                if (continentalRegions.Count < Mathf.RoundToInt(voronoiRegions.Count * continentalRegionPercentage))
                {
                    SetRegionContinental(true, border);
                }
            }
        }
    }

    public void AdjustHeightMap()
    {
        FastNoiseLite heightNoise = new FastNoiseLite(rng.Next(-99999, 99999));
        heightNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        heightNoise.SetFractalOctaves(8);
        heightNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        FastNoiseLite erosion = new FastNoiseLite(rng.Next(-99999, 99999));
        erosion.SetFractalType(FastNoiseLite.FractalType.Ridged);
        erosion.SetFractalOctaves(4);
        erosion.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        float scale = 2f;
        float erosionScale = 0.5f;
        for (int x = 0; x < worldSize.X; x++)
        {
            for (int y = 0; y < worldSize.Y; y++)
            {
                if (tiles[x, y].region.continental)
                {
                    float noiseValue = Mathf.InverseLerp(-0.8f, 1f, erosion.GetNoise(x / erosionScale, y / erosionScale));
                    float coastMultiplier = Mathf.Clamp(tiles[x, y].coastDist / (10f * worldMult * Mathf.Lerp(0.2f, 7f, noiseValue)), 0f, 1f);
                    heightmap[x, y] = 0.6f + (Mathf.InverseLerp(-0.8f, 0.8f, heightNoise.GetNoise(x / scale, y / scale)) * 0.3f * Mathf.Clamp(Mathf.Log((9f * coastMultiplier) + 1), 0, 1));
                    heightmap[x, y] = Mathf.Clamp(heightmap[x, y], 0.6f, 1f);
                    //heightmap[x, y] = Mathf.Clamp(tiles[x, y].boundaryDist/10f, 0.6f, 1f);
                }
            }
        }
        for (int i = 0; i < gridSizeX; i++)
        {
            for (int j = 0; j < gridSizeY; j++)
            {
                Vector2I pos = points[new Vector2I(i, j)].seed;
            }
        }
    }
    Dictionary<Vector2I, VoronoiRegion> GeneratePoints()
    {
        ppcx = Mathf.RoundToInt(worldSize.X / (float)gridSizeX);
        ppcy = Mathf.RoundToInt(worldSize.Y / (float)gridSizeY);
        Dictionary<Vector2I, VoronoiRegion> point = new Dictionary<Vector2I, VoronoiRegion>();
        for (int i = 0; i < gridSizeX; i++) {
            for (int j = 0; j < gridSizeY; j++)
            {
                VoronoiRegion region = new VoronoiRegion();
                region.seed = new Vector2I(i * ppcx + rng.Next(0, ppcx), j * ppcy + rng.Next(0, ppcy));
                point.Add(new Vector2I(i, j), region);
                voronoiRegions.Add(region);
            }
        }
        return point;
    }
    public void GenerateRegions(int landCount)
    {
        int addedLand = 0;
        int attempts = 5000;
        while (addedLand < landCount && attempts > 0)
        {
            attempts--;
            VoronoiRegion region = voronoiRegions[rng.Next(0, voronoiRegions.Count)];
            if (!region.continental)
            {
                addedLand += 1;
                SetRegionContinental(true, region);
            }
        }

        // Assigns tiles to their region
        FastNoiseLite xNoise = new FastNoiseLite(rng.Next(-99999, 99999));
        xNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        xNoise.SetFractalOctaves(8);
        xNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        FastNoiseLite yNoise = new FastNoiseLite(rng.Next(-99999, 99999));
        yNoise.SetFractalType(FastNoiseLite.FractalType.FBm);
        yNoise.SetFractalOctaves(8);
        yNoise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        float scale = 2;
        GD.Print(new Vector2I(-3, 2).WrappedMidpoint(new Vector2I(5, 2), worldSize));
        for (int x = 0; x < worldSize.X; x++)
        {
            for (int y = 0; y < worldSize.Y; y++)
            {

                TerrainTile tile = new TerrainTile();
                // Domain warping
                int fx = (int)Mathf.PosMod(x + (xNoise.GetWrappedNoise(x / scale, y / scale, worldSize) * 50), worldSize.X);
                int fy = (int)Mathf.PosMod(y + (yNoise.GetWrappedNoise(x / scale, y / scale, worldSize) * 50), worldSize.Y);

                Vector2I pos = new Vector2I(fx, fy);
                VoronoiRegion region = null;
                // Loops through the points
                int gx = fx / ppcx;
                int gy = fy / ppcy;
                try
                {
                    PriorityQueue<VoronoiRegion, float> distances = new PriorityQueue<VoronoiRegion, float>();
                    for (int i = -1; i < 2; i++)
                    {
                        for (int j = -1; j < 2; j++)
                        {
                            int gridX = Mathf.PosMod(gx - i, gridSizeX);
                            int gridY = Mathf.PosMod(gy - j, gridSizeY);
                            float dist = pos.WrappedDistanceSquaredTo(points[new Vector2I(gridX, gridY)].seed, worldSize);
                            distances.Enqueue(points[new Vector2I(gridX, gridY)], dist);
                        }
                    }
                    region = distances.Dequeue();
                    tile.region = region;
                    tiles[x, y] = tile;
                }
                catch (Exception e)
                {
                    GD.PushError(e);
                }


            }
        }

        // Gets Region Borders
        for (int x = 0; x < worldSize.X; x++)
        {
            for (int y = 0; y < worldSize.Y; y++)
            {
                VoronoiRegion region = tiles[x, y].region;
                Vector2I pos = new Vector2I(x, y);
                for (int dx = -1; dx < 2; dx++)
                {
                    for (int dy = -1; dy < 2; dy++)
                    {
                        if (dx == 0 && dy == 0)
                        {
                            continue;
                        }
                        Vector2I next = new Vector2I(Mathf.PosMod(pos.X + dx, worldSize.X), Mathf.PosMod(pos.Y + dy, worldSize.Y));
                        VoronoiRegion neighbor = tiles[next.X, next.Y].region;
                        if (neighbor != region)
                        {
                            tiles[x, y].border = true;
                            if (!region.borderingRegions.Contains(neighbor))
                            {
                                region.borderingRegions.Add(neighbor);
                            }
                        }

                    }
                }
            }
        }
    }

    void GetDistances()
    {
        for (int x = 0; x < worldSize.X; x++)
        {
            for (int y = 0; y < worldSize.Y; y++)
            {
                VoronoiRegion region = tiles[x, y].region;
                Vector2I pos = new Vector2I(x, y);
                for (int dx = -1; dx < 2; dx++)
                {
                    for (int dy = -1; dy < 2; dy++)
                    {
                        if (dx == 0 && dy == 0)
                        {
                            continue;
                        }
                        Vector2I next = new Vector2I(Mathf.PosMod(pos.X + dx, worldSize.X), Mathf.PosMod(pos.Y + dy, worldSize.Y));
                        VoronoiRegion neighbor = tiles[next.X, next.Y].region;
                        if (neighbor != region)
                        {
                            tiles[x, y].border = true;
                            if (!region.borderingRegions.Contains(neighbor))
                            {
                                region.borderingRegions.Add(neighbor);
                            }
                            if (!neighbor.continental && region.continental)
                            {
                                region.coastal = true;
                                region.coastalTiles.Add(pos);
                            }
                        }

                    }
                }
            }
        }
        for (int x = 0; x < worldSize.X; x++)
        {
            for (int y = 0; y < worldSize.Y; y++)
            {
                if (!tiles[x, y].region.continental)
                {
                    continue;
                }
                List<Vector2I> tilesToCheck = [.. tiles[x, y].region.coastalTiles];
                foreach (VoronoiRegion r in tiles[x, y].region.borderingRegions) {
                    if (r.coastal)
                    {
                        tilesToCheck.AddRange(r.coastalTiles);
                    }
                }
                TerrainTile tile = tiles[x, y];
                Vector2I pos = new Vector2I(x, y);
                PriorityQueue<Vector2I, float> distances = new PriorityQueue<Vector2I, float>();
                if (tilesToCheck.Count > 0)
                {
                    foreach (Vector2I next in tilesToCheck)
                    {
                        distances.Enqueue(next, pos.WrappedDistanceSquaredTo(next, worldSize));
                    }
                    tile.coastDist = pos.WrappedDistanceTo(distances.Dequeue(), worldSize);                    
                }
            }
        }        
    }

    void SetRegionContinental(bool value, VoronoiRegion region) {

        if (value == true)
        {
            region.continental = true;
            continentalRegions.Add(region);
        }
        else
        {
            region.continental = false;
            continentalRegions.Remove(region);
        }
    }
}
internal class VoronoiRegion
{
    public Vector2I seed;
    public bool continental = false;
    public bool coastal = false;
    public Plate plate;
    public List<Vector2I> coastalTiles = new List<Vector2I>();
    public List<Vector2I> boundaryTiles = new List<Vector2I>();
    public List<VoronoiRegion> borderingRegions = new List<VoronoiRegion>();
}
internal class TerrainTile
{
    public VoronoiRegion region;
    public float coastDist = Mathf.Inf;
    public float boundaryDist = Mathf.Inf;
    public TerrainTile nearestBoundary = null;
    public float pressure = 0f;
    public bool collisionContinental = false;
    public bool convergent;
    public bool coastal;
    public bool border;
    public bool fault;
    public bool offshore;
}
internal class Plate
{
    public List<VoronoiRegion> regions;
    public Vector2 dir;
}
