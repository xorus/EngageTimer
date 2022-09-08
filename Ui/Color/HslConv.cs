using System;
using System.Numerics;

/*
 * https://www.programmingalgorithms.com/algorithm/rgb-to-hsl/
 * https://www.programmingalgorithms.com/algorithm/hsl-to-rgb/
 */
namespace EngageTimer.Ui.Color;

public class HslConv
{
    public static Vector4 HslToVector4Rgb(int h, float s, float l)
    {
        var rgb = HslToRgb(new Hsl(h, s, l));
        return new Vector4(rgb.R / 255f, rgb.G / 255f, rgb.B / 255f, 1f);
    }

    public static Hsl RgbToHsl(Rgb rgb)
    {
        var hsl = new Hsl();

        var r = rgb.R / 255.0f;
        var g = rgb.G / 255.0f;
        var b = rgb.B / 255.0f;

        var min = Math.Min(Math.Min(r, g), b);
        var max = Math.Max(Math.Max(r, g), b);
        var delta = max - min;

        hsl.L = (max + min) / 2;

        if (delta == 0)
        {
            hsl.H = 0;
            hsl.S = 0.0f;
        }
        else
        {
            hsl.S = hsl.L <= 0.5 ? delta / (max + min) : delta / (2 - max - min);

            float hue;
            if (r == max) hue = (g - b) / 6 / delta;
            else if (g == max) hue = 1.0f / 3 + (b - r) / 6 / delta;
            else hue = 2.0f / 3 + (r - g) / 6 / delta;

            if (hue < 0)
                hue += 1;
            if (hue > 1)
                hue -= 1;

            hsl.H = (int)(hue * 360);
        }

        return hsl;
    }

    public static Rgb HslToRgb(Hsl hsl)
    {
        byte r = 0;
        byte g = 0;
        byte b = 0;

        if (hsl.S == 0)
        {
            r = g = b = (byte)(hsl.L * 255);
        }
        else
        {
            var hue = (float)hsl.H / 360;

            var v2 = hsl.L < 0.5 ? hsl.L * (1 + hsl.S) : hsl.L + hsl.S - hsl.L * hsl.S;
            var v1 = 2 * hsl.L - v2;

            r = (byte)(255 * HueToRgb(v1, v2, hue + 1.0f / 3));
            g = (byte)(255 * HueToRgb(v1, v2, hue));
            b = (byte)(255 * HueToRgb(v1, v2, hue - 1.0f / 3));
        }

        return new Rgb(r, g, b);
    }

    private static float HueToRgb(float v1, float v2, float vH)
    {
        if (vH < 0) vH += 1;
        if (vH > 1) vH -= 1;
        if (6 * vH < 1) return v1 + (v2 - v1) * 6 * vH;
        if (2 * vH < 1) return v2;
        if (3 * vH < 2) return v1 + (v2 - v1) * (2.0f / 3 - vH) * 6;

        return v1;
    }

    public struct Rgb
    {
        public Rgb(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
    }

    public struct Hsl
    {
        public Hsl(int h, float s, float l)
        {
            H = h;
            S = s;
            L = l;
        }

        public int H { get; set; }

        public float S { get; set; }

        public float L { get; set; }

        public Vector4 ToVector4()
        {
            return new Vector4(H, S, L, 1f);
        }
    }
}