using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AlkuTD
{
    public struct ColorHSL
    {
        public float H, S, L;
    }

    public static class ColorConversion
    {
        /// <summary>
        /// Converts a Color from RGB to HSL
        /// See http://en.wikipedia.org/wiki/HSL_color_space#Conversion_from_RGB_to_HSL_or_HSV for details
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static ColorHSL RGBtoHSL(Color color)
        {

            ColorHSL hsl = new ColorHSL();

            float R = (float)color.R / (float)byte.MaxValue;
            float G = (float)color.G / (float)byte.MaxValue;
            float B = (float)color.B / (float)byte.MaxValue;

            float min = Math.Min(R, G);
            min = Math.Min(min, B);

            float max = Math.Max(R, G);
            max = Math.Max(max, B);

            hsl.L = (max + min) / 2;

            if (hsl.L < 0.5f)
                hsl.S = (max - min) / (max + min);
            else if (hsl.L >= 0.5f)
                hsl.S = (max - min) / (2.0f - max - min);

            if (max == min)
                hsl.S = 0;

            if (R == max)
                hsl.H = (G - B) / (max - min);
            else if (G == max)
                hsl.H = 2.0f + (B - R) / (max - min);
            else if (B == max)
                hsl.H = 4.0f + (R - G) / (max - min);

            hsl.H *= 360;

            return hsl;
        }

        /// <summary>
        /// Converts a Color from HSL to RGB
        /// See http://en.wikipedia.org/wiki/HSL_color_space#Conversion_from_HSL_to_RGB for details
        /// </summary>
        /// <param name="hsl"></param>
        /// <returns></returns>
        public static Color HSLtoRGB(ColorHSL hsl)
        {
            if (hsl.S == 0)
                return new Color(hsl.L, hsl.L, hsl.L);

            float temp1;
            float temp2;
            float Rtemp3, Gtemp3, Btemp3;
            float H;

            if (hsl.L < 0.5f)
                temp2 = hsl.L * (1.0f + hsl.S);
            else
                temp2 = hsl.L + hsl.S - hsl.L * hsl.S;

            temp1 = 2.0f * hsl.L - temp2;

            H = hsl.H / 360;

            Rtemp3 = H + 1.0f / 3.0f;
            Gtemp3 = H;
            Btemp3 = H - 1.0f / 3.0f;

            if (Rtemp3 < 0)
                Rtemp3 += 1.0f;
            if (Rtemp3 > 1)
                Rtemp3 -= 1.0f;
            if (Gtemp3 < 0)
                Gtemp3 += 1.0f;
            if (Gtemp3 > 1)
                Gtemp3 -= 1.0f;
            if (Btemp3 < 0)
                Btemp3 += 1.0f;
            if (Btemp3 > 1)
                Btemp3 -= 1.0f;

            float R, G, B;

            if ((6.0f * Rtemp3) < 1)
                R = temp1 + (temp2 - temp1) * 6.0f * Rtemp3;
            else if (2.0f * Rtemp3 < 1)
                R = temp2;
            else if (3.0f * Rtemp3 < 2)
                R = temp1 + (temp2 - temp1) * ((2.0f / 3.0f) - Rtemp3) * 6.0f;
            else
                R = temp1;

            if ((6.0f * Gtemp3) < 1)
                G = temp1 + (temp2 - temp1) * 6.0f * Gtemp3;
            else if (2.0f * Gtemp3 < 1)
                G = temp2;
            else if (3.0f * Gtemp3 < 2)
                G = temp1 + (temp2 - temp1) * ((2.0f / 3.0f) - Gtemp3) * 6.0f;
            else
                G = temp1;

            if ((6.0f * Btemp3) < 1)
                B = temp1 + (temp2 - temp1) * 6.0f * Btemp3;
            else if (2.0f * Btemp3 < 1)
                B = temp2;
            else if (3.0f * Btemp3 < 2)
                B = temp1 + (temp2 - temp1) * ((2.0f / 3.0f) - Btemp3) * 6.0f;
            else
                B = temp1;

            return new Color(R, G, B);

        }

        /// <summary>
        /// Gets the lightness of the given color
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static float GetLightness(Color color)
        {
            return RGBtoHSL(color).L;
        }

        public static void SetLightness(ref Color color, float brightness)
        {
            ColorHSL hsl = RGBtoHSL(color);
            hsl.L = brightness;
            color = HSLtoRGB(hsl);
        }

        public static Color SetLightness(Color color, float brightness)
        {
            ColorHSL hsl = RGBtoHSL(color);
            hsl.L = brightness;
            return HSLtoRGB(hsl);
        }
    }
}
