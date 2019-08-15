// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System;
using System.Globalization;

namespace SettingsStoreExplorer
{
    public abstract class IntegerToStringConverter
    {
        public abstract string ToString(object value, NumberStyles style, CultureInfo culture);
        public abstract bool TryParse(string text, NumberStyles style, CultureInfo culture, out object value);
        public object Parse(string text, NumberStyles style, CultureInfo culture)
        {
            if (TryParse(text, style, culture, out var value))
            {
                return value;
            }

            throw new FormatException("Could not parse the given text.");
        }

        protected static string FormatStringFromStyle(NumberStyles style) => style == NumberStyles.HexNumber ? "x" : "d";
    }
}
