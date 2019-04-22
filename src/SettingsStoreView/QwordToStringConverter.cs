// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System.Globalization;

namespace SettingsStoreView
{
    internal sealed class QwordToStringConverter : IntegerToStringConverter
    {
        public static readonly QwordToStringConverter Instance = new QwordToStringConverter();

        private QwordToStringConverter()
        {
        }

        public override string ToString(object value, NumberStyles style, CultureInfo culture)
        {
            return ((ulong)value).ToString(FormatStringFromStyle(style), culture);
        }

        public override bool TryParse(string text, NumberStyles style, CultureInfo culture, out object value)
        {
            if (ulong.TryParse(text, style, culture, out var result))
            {
                value = result;
                return true;
            }

            value = null;
            return false;
        }
    }
}
