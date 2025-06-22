using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Godot;
using Vector2 = System.Numerics.Vector2;
public static class Utility
{
    private static Random rng = new Random();

    public static void Shuffle<T>(this IList<T> list, Random r = null)
    {
        if (r == null)
        {
            r = rng;
        }
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = r.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    public static T PickRandom<T>(this IList<T> array, Random r = null)
    {
        if (r == null)
        {
            r = rng;
        }
        int length = array.Count();
        return array[r.Next(0, length - 1)];
    }

    public static string[] GetAsArray(this Godot.FileAccess f)
    {
        List<string> result = new List<string>();
        while (!f.EofReached())
        {
            result.Add(f.GetLine());
        }
        f.Close();
        return result.ToArray();
    }

    public static float NextSingle(this Random rng, float minValue, float maxValue)
    {
        return Mathf.Lerp(rng.NextSingle(), minValue, maxValue);
    }
    public static float WrappedDistanceTo(this Vector2 pointA, Vector2 pointB, Vector2 worldSize)
    {
        float dx = pointA.X - pointB.X;
        float dy = pointA.Y - pointB.Y;
        if (dx > worldSize.X / 2f)
        {
            dx = worldSize.X - dx;
        }
        if (dy > worldSize.Y / 2f)
        {
            dy = worldSize.Y - dy;
        }
        return Mathf.Sqrt(Mathf.Pow(dx, 2) + Mathf.Pow(dy, 2));
    }
    public static float WrappedDistanceTo(this Vector2I pointA, Vector2I pointB, Vector2I worldSize)
    {
        float dx = Mathf.Abs(pointB.X - pointA.X);
        float dy = Mathf.Abs(pointB.Y - pointA.Y);
        if (dx > worldSize.X / 2f)
        {
            dx = worldSize.X - dx;
        }
        if (dy > worldSize.Y / 2f)
        {
            dy = worldSize.Y - dy;
        }
        return Mathf.Sqrt(Mathf.Pow(dx, 2) + Mathf.Pow(dy, 2));
    }
    public static float WrappedDistanceSquaredTo(this Vector2I pointA, Vector2I pointB, Vector2I worldSize)
    {
        float dx = Mathf.Abs(pointB.X - pointA.X);
        float dy = Mathf.Abs(pointB.Y - pointA.Y);
        if (dx > worldSize.X / 2f)
        {
            dx = worldSize.X - dx;
        }
        if (dy > worldSize.Y / 2f)
        {
            dy = worldSize.Y - dy;
        }
        return Mathf.Pow(dx, 2) + Mathf.Pow(dy, 2);
    }
    public static Vector2I WrappedDelta(this Vector2I pointA, Vector2I pointB, Vector2I worldSize)
    {
        float dx = pointB.X - pointA.X;
        if (Mathf.Abs(dx) > worldSize.X / 2f)
        {
            dx -= Math.Sign(dx) * worldSize.X;
        }
        float dy = pointB.Y - pointA.Y;
        if (Mathf.Abs(dy) > worldSize.Y / 2f)
        {
            dy -= Math.Sign(dy) * worldSize.Y;
        }
        return new Vector2I((int)dx, (int)dy);
    }
    public static Vector2I WrappedMidpoint(this Vector2I pointA, Vector2I pointB, Vector2I worldSize)
    {

        int dx = pointB.X - pointA.X;
        if (Mathf.Abs(dx) > worldSize.X / 2f)
        {
            dx -= Math.Sign(dx) * worldSize.X;
        }
        int dy = pointB.Y - pointA.Y;
        if (Mathf.Abs(dy) > worldSize.Y / 2f)
        {
            dy -= Math.Sign(dy) * worldSize.Y;
        }
        //GD.Print(dy);
        return new Vector2I(Mathf.RoundToInt(Mathf.PosMod(pointA.X + dx / 2f, worldSize.X)), Mathf.RoundToInt(Mathf.PosMod(pointA.Y + dy / 2f, worldSize.Y)));
    }
    public static float GetWrappedNoise(this FastNoiseLite noise, float x, float y, Vector2I worldSize)
    {
        float nx = y;
        float ny = Mathf.Sin(x * (Mathf.Pi * 2) / worldSize.X) / (Mathf.Pi * 2) * worldSize.X;
        float nz = Mathf.Cos(x * (Mathf.Pi * 2) / worldSize.X) / (Mathf.Pi * 2) * worldSize.X;
        return noise.GetNoise(nx, ny, nz);
    }
    public static Color MultiColourLerp(Color[] colours, float t) {

        t = Mathf.Clamp(t, 0, 1);

        float delta = 1f / (colours.Length - 1);
        int startIndex = (int)(t / delta);

        if(startIndex == colours.Length - 1) {
            return colours[colours.Length - 1];
        }

        float localT = (t % delta) / delta;

        return (colours[startIndex] * (1f - localT)) + (colours[startIndex + 1] * localT);
    }
}