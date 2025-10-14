namespace System.Drawing;

public static partial class ColorExtensions
{
    public static string HexA(this Color color)
    {
        return color.A.ToString("X2");
    }

    public static string HexR(this Color color)
    {
        return color.R.ToString("X2");
    }

    public static string HexG(this Color color)
    {
        return color.G.ToString("X2");
    }

    public static string HexB(this Color color)
    {
        return color.B.ToString("X2");
    }

    public static string ToHex(this Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    public static void ToHex(this Color color, out string hexR, out string hexG, out string hexB)
    {
        hexR = color.R.ToString("X2");
        hexG = color.G.ToString("X2");
        hexB = color.B.ToString("X2");
    }

    public static string ToArgbHex(this Color color)
    {
        return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    public static void ToHex(this Color color, out string hexA, out string hexR, out string hexG, out string hexB)
    {
        hexA = color.A.ToString("X2");
        color.ToHex(out hexR, out hexG, out hexB);
    }

    public static string ToRgb(this Color color)
    {
        return $"{color.R}, {color.G}, {color.B}";
    }

    public static string ToArgb(this Color color)
    {
        return $"{color.A}, {color.R}, {color.G}, {color.B}";
    }

    public static string ToHsl(this Color color)
    {
        color.ToHsl(out float h, out float s, out float l);
        return $"{h}, {s}, {l}";
    }

    public static void ToHsl(this Color color, out float hue, out float saturation, out float lightness)
    {
        hue = color.GetHue();
        saturation = color.GetSaturation();
        lightness = color.GetBrightness();
    }

    public static string ToHsv(this Color color)
    {
        color.ToHsv(out double h, out double s, out double v);
        return $"{h}, {s}, {v}";
    }

    public static void ToHsv(this Color color, out double hue, out double saturation, out double value)
    {
        // Normalize R, G, B values to the range 0-1
        var rNorm = color.R / 255.0;
        var gNorm = color.G / 255.0;
        var bNorm = color.B / 255.0;

        var cmax = Math.Max(rNorm, Math.Max(gNorm, bNorm)); // Maximum of R, G, B
        var cmin = Math.Min(rNorm, Math.Min(gNorm, bNorm)); // Minimum of R, G, B
        var delta = cmax - cmin;

        // Calculate Hue
        if (delta == 0)
        {
            // Achromatic (grayscale)
            hue = 0;
        }
        else if (cmax == rNorm)
        {
            hue = (gNorm - bNorm) / delta;

            // Ensure positive hue
            if (hue < 0)
                hue += 6;
        }
        else if (cmax == gNorm)
        {
            hue = (bNorm - rNorm) / delta + 2;
        }
        else // cmax == bNorm
        {
            hue = (rNorm - gNorm) / delta + 4;
        }

        // Convert to degrees (0-360)
        hue *= 60;

        // Calculate Saturation
        saturation = (cmax == 0 ? 0 : delta / cmax);

        // Calculate Value
        value = cmax;
    }
}
