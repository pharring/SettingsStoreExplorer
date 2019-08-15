// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System.Globalization;

namespace SettingsStoreExplorer
{
    internal sealed class DwordToStringConverter : IntegerToStringConverter
    {
        public static readonly DwordToStringConverter Instance = new DwordToStringConverter();

        private DwordToStringConverter()
        {
        }

        public override string ToString(object value, NumberStyles style, CultureInfo culture)
            => ((uint)value).ToString(FormatStringFromStyle(style), culture);

        public override bool TryParse(string text, NumberStyles style, CultureInfo culture, out object value)
        {
            if (uint.TryParse(text, style, culture, out var result))
            {
                value = result;
                return true;
            }

            value = null;
            return false;
        }
    }
}
