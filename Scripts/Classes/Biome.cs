public class Biome
{
    public string name { get; set; }
    public string id { get; set; }
    public string type { get; set; } = "ice";
    public float plantDensity { get; set; } = 0.0f;
    public float maxElevation { get; set; } = float.PositiveInfinity;
    public float minElevation { get; set; } = float.NegativeInfinity;
    public float maxMoisture { get; set; } = float.PositiveInfinity;
    public float minMoisture { get; set; } = float.NegativeInfinity;
    public float maxTemperature { get; set; } = float.PositiveInfinity;
    public float minTemperature { get; set; } = float.NegativeInfinity;
    public float navigability { get; set; } = 0.0f;
    public float ariablity { get; set; } = 0.0f;
    public float survivability { get; set; } = 0.0f;
    public string color { get; set; } = "FFFFFF";
    public int textureX { get; set; }
    public int textureY { get; set; }
    public bool special { get; set; } = false;
}