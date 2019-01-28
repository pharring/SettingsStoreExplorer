// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.PlatformUI;

namespace SettingsStoreView
{
    internal sealed class SettingsStorePropertyValueConverter : ValueConverter<object, string>
    {
        protected override string Convert(object value, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                return (string)value;
            }

            if (value is byte[])
            {
                var arr = (byte[])value;
                var query = arr.Select(b => b.ToString("X2"));
                return string.Join(" ", query);
            }

            if (value is int)
            {
                return string.Format(culture, "0x{0:x8} ({0})", value);
            }

            if (value is long)
            {
                return string.Format(culture, "0x{0:x16} ({0})", value);
            }

            return "<unknown>";
        }
    }
}
