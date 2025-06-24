using Godot;
using Godot.Collections;
using System;

public partial class WorldGenButton : Button
{
    public override void _Ready()
    {
        Pressed += pressed;
    }


    void pressed()
    {
        GD.Print("Button pressed");
        WorldGenerator.GenerateWorld();
    }
}
