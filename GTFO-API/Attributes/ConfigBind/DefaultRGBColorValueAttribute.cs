using System;
using System.ComponentModel;
using UnityEngine;

namespace GTFO.API.Attributes.ConfigBind
{
    /// <summary>
    /// Util Attribute for <see cref="DefaultValueAttribute"/>, Convert types of RGB Color Represents to Hex String
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DefaultRGBColorValueAttribute : Attribute
    {
        /// <summary>
        /// Hex String Value
        /// </summary>
        public string HexValue { get; private set; }

        /// <summary>
        /// RGB Color Value
        /// </summary>
        public Color RGBColor { get; private set; }

        /// <summary>
        /// Create Attribute Instance Using HTML String
        /// </summary>
        /// <param name="htmlString">HTML String to Convert</param>
        /// <exception cref="ArgumentException">Throws if input string is invalid</exception>
        public DefaultRGBColorValueAttribute(string htmlString)
        {
            if (!ColorUtility.TryParseHtmlString(htmlString, out var color))
            {
                throw new ArgumentException($"'{htmlString}' is not a valid HTML Color String", nameof(htmlString));
            }

            RGBColor = color;
            HexValue = ColorUtility.ToHtmlStringRGB(RGBColor);
        }

        /// <summary>
        /// Create Attribute Instance Using RGB float values [0.0~1.0]
        /// </summary>
        /// <param name="r">R channel value [0.0~1.0]</param>
        /// <param name="g">G channel value [0.0~1.0]</param>
        /// <param name="b">B channel value [0.0~1.0]</param>
        public DefaultRGBColorValueAttribute(float r, float g, float b)
        {
            RGBColor = new Color(r, g, b);
            HexValue = ColorUtility.ToHtmlStringRGB(RGBColor);
        }

        /// <summary>
        /// Create Attribute Instance Using RGB byte values [0~255]
        /// </summary>
        /// <param name="r">R channel value [0~255]</param>
        /// <param name="g">G channel value [0~255]</param>
        /// <param name="b">B channel value [0~255]</param>
        public DefaultRGBColorValueAttribute(byte r, byte g, byte b)
        {
            RGBColor = new Color(r / 255.0f, g / 255.0f, b / 255.0f);
            HexValue = ColorUtility.ToHtmlStringRGB(RGBColor);
        }
    }
}
