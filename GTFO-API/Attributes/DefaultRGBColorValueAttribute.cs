using System;
using UnityEngine;

namespace GTFO.API.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DefaultRGBColorValueAttribute : Attribute
    {
        public string HexValue { get; private set; }
        public Color RGBColor { get; private set; }

        public DefaultRGBColorValueAttribute(float r, float g, float b)
        {
            RGBColor = new Color(r, g, b);
            HexValue = ColorUtility.ToHtmlStringRGB(RGBColor);
        }

        public DefaultRGBColorValueAttribute(byte r, byte g, byte b)
        {
            RGBColor = new Color(r / 255.0f, g / 255.0f, b / 255.0f);
            HexValue = ColorUtility.ToHtmlStringRGB(RGBColor);
        }
    }
}
