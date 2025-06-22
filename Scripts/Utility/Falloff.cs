using Godot;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
public class Falloff
{
    public static float[,] GenerateFalloffMap(int width, int height, bool includeX = true, float b = 7.2f, float a = 3){
        float[,] map = new float[width,height];
        for (int i = 0; i < width; i++){
            for (int j = 0; j < height; j++){
                float x = Mathf.Clamp((float)i / (float)width * 2 - 1, -1, 1);
                float y = Mathf.Clamp((float)j / (float)height * 2 - 1, -1, 1);
                float val = Mathf.Abs(y);
                if (includeX){
                    val = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                }
                map[i,j] = Evaluate(val, b, a);
            }
        }
        return map;
    }
    public static float Evaluate(float v, float b = 7.2f, float a = 3){
        return Mathf.Pow(v, a) / (Mathf.Pow(v, a) + Mathf.Pow(b - (b * v), a));
    }
}