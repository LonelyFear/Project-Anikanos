using System;
using Godot;
public class WorldGenerator
{
    public Vector2I worldSize = new Vector2I(360, 180);
    public float worldMult = 2f;
    public float seaLevel = 0.6f;
    public int seed;
}