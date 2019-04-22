// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.PlatformUI;

namespace SettingsStoreView
{
    internal class SettingsStorePropertyValueConverter : ValueConverter<object, string>
    {
        protected override string Convert(object value, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                return (string)value;
            }

            if (value is byte[] arr)
            {
                if (arr.Length == 0)
                {
                    return "(zero-length binary value)";
                }

                var query = arr.Select(b => b.ToString("X2"));
                return string.Join(" ", query);
            }

            return value is uint
                ? string.Format(culture, "0x{0:x8} ({0})", value)
                : value is ulong ? string.Format(culture, "0x{0:x16} ({0})", value) : "<unknown>";
        }
    }
}
