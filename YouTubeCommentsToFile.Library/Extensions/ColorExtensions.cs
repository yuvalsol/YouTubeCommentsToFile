#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Drawing;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static partial class ColorExtensions
{
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

    public static string HexA(this Color color)
    {
        return color.A.ToString("X2");
    }

    private static string ToHexRGB(this Color color)
    {
        return $"{color.HexR()}{color.HexG()}{color.HexB()}";
    }

    public static string ToHex(this Color color)
    {
        return $"#{color.ToHexRGB()}";
    }

    public static string ToAHex(this Color color)
    {
        return $"#{color.HexA()}{color.ToHexRGB()}";
    }

    public static string ToHexA(this Color color)
    {
        return $"#{color.ToHexRGB()}{color.HexA()}";
    }

    public static void GetHex(this Color color, out string hexR, out string hexG, out string hexB)
    {
        hexR = color.HexR();
        hexG = color.HexG();
        hexB = color.HexB();
    }

    public static void GetHex(this Color color, out string hexR, out string hexG, out string hexB, out string hexA)
    {
        color.GetHex(out hexR, out hexG, out hexB);
        hexA = color.HexA();
    }

    public static string ToRgb(this Color color)
    {
        return $"{color.R}, {color.G}, {color.B}";
    }

    public static string ToArgb(this Color color)
    {
        return $"{color.A}, {color.ToRgb()}";
    }

    public static string ToRgba(this Color color)
    {
        return $"{color.ToRgb()}, {color.A}";
    }

    public static string ToHsl(this Color color)
    {
        return $"{color.GetHue()}, {color.GetSaturation()}, {color.GetBrightness()}";
    }

    public static string ToAhsl(this Color color)
    {
        return $"{color.A}, {color.ToHsl()}";
    }

    public static string ToHsla(this Color color)
    {
        return $"{color.ToHsl()}, {color.A}";
    }

    public static void GetHsl(this Color color, out float hue, out float saturation, out float lightness)
    {
        hue = color.GetHue();
        saturation = color.GetSaturation();
        lightness = color.GetBrightness();
    }

    public static void GetHsl(this Color color, out float hue, out float saturation, out float lightness, out byte alpha)
    {
        color.GetHsl(out hue, out saturation, out lightness);
        alpha = color.A;
    }

    public static string ToHsv(this Color color)
    {
        color.GetHsv(out double hue, out double saturation, out double value);
        return $"{hue}, {saturation}, {value}";
    }

    public static string ToAhsv(this Color color)
    {
        return $"{color.A}, {color.ToHsv()}";
    }

    public static string ToHsva(this Color color)
    {
        return $"{color.ToHsv()}, {color.A}";
    }

    public static void GetHsv(this Color color, out double hue, out double saturation, out double value)
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

    public static void GetHsv(this Color color, out double hue, out double saturation, out double value, out byte alpha)
    {
        color.GetHsv(out hue, out saturation, out value);
        alpha = color.A;
    }
}
