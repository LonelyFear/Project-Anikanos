using Godot;
using System;

public partial class SliderLabel : Label
{
    [Export] HSlider slider;

    public override void _Process(double delta)
    {
        SelfModulate = Color.Color8(255, 255, 255);
        if (slider != null)
        {
            Text = "World Size: " + slider.Value.ToString("0.0x");
            float sizeWarning = (float)(slider.Value - slider.MinValue) / (float)(slider.MaxValue - slider.MinValue);
            SelfModulate = Utility.MultiColourLerp([new Color(1, 1, 1), new Color(1, 1, 0), new Color(1, 0, 0)], sizeWarning);
            WorldGenerator.WorldMult = (float)slider.Value;
        }
        else
        {
            Text = "";
        }
    }

}
