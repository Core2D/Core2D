﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Core2D.Style
{
    /// <summary>
    /// Color definition using alpha, red, green and blue channels.
    /// </summary>
    public class ArgbColor : ObservableObject
    {
        private byte _a;
        private byte _r;
        private byte _g;
        private byte _b;

        /// <summary>
        /// Alpha color channel.
        /// </summary>
        public byte A
        {
            get { return _a; }
            set { Update(ref _a, value); }
        }

        /// <summary>
        /// Red color channel.
        /// </summary>
        public byte R
        {
            get { return _r; }
            set { Update(ref _r, value); }
        }

        /// <summary>
        /// Green color channel.
        /// </summary>
        public byte G
        {
            get { return _g; }
            set { Update(ref _g, value); }
        }

        /// <summary>
        /// Blue color channel.
        /// </summary>
        public byte B
        {
            get { return _b; }
            set { Update(ref _b, value); }
        }

        /// <summary>
        /// Creates a new <see cref="ArgbColor"/> instance.
        /// </summary>
        /// <param name="a">The alpha color channel.</param>
        /// <param name="r">The red color channel.</param>
        /// <param name="g">The green color channel.</param>
        /// <param name="b">The blue color channel.</param>
        /// <returns>The new instance of the <see cref="ArgbColor"/> class.</returns>
        public static ArgbColor Create(byte a = 0xFF, byte r = 0x00, byte g = 0x00, byte b = 0x00)
        {
            return new ArgbColor()
            {
                A = a,
                R = r,
                G = g,
                B = b
            };
        }

        /// <summary>
        /// Creates a <see cref="ArgbColor"/> from an integer.
        /// </summary>
        /// <param name="value">The integer value.</param>
        /// <returns>The color.</returns>
        public static ArgbColor FromUInt32(uint value)
        {
            return new ArgbColor
            {
                A = (byte)((value >> 24) & 0xff),
                R = (byte)((value >> 16) & 0xff),
                G = (byte)((value >> 8) & 0xff),
                B = (byte)(value & 0xff),
            };
        }

        /// <summary>
        /// Parses a color string.
        /// </summary>
        /// <param name="s">The color string.</param>
        /// <returns>The <see cref="ArgbColor"/>.</returns>
        public static ArgbColor Parse(string s)
        {
            if (s[0] == '#')
            {
                var or = 0u;

                if (s.Length == 7)
                {
                    or = 0xff000000;
                }
                else if (s.Length != 9)
                {
                    throw new FormatException($"Invalid color string: '{s}'.");
                }

                return FromUInt32(uint.Parse(s.Substring(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture) | or);
            }
            else
            {
                var upper = s.ToUpperInvariant();
                var member = typeof(Colors).GetTypeInfo().DeclaredProperties.FirstOrDefault(x => x.Name.ToUpperInvariant() == upper);
                if (member != null)
                {
                    return (ArgbColor)member.GetValue(null);
                }
                else
                {
                    throw new FormatException($"Invalid color string: '{s}'.");
                }
            }
        }

        /// <summary>
        /// Converts a color to string.
        /// </summary>
        /// <param name="c">The color instance.</param>
        /// <returns>The color string.</returns>
        public static string ToHtml(ArgbColor c)
        {
            return string.Concat('#', c.A.ToString("X2"), c.R.ToString("X2"), c.G.ToString("X2"), c.B.ToString("X2"));
        }
    }
}
