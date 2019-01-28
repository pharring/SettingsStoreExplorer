// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System.Globalization;
using Microsoft.VisualStudio.PlatformUI;

namespace SettingsStoreView
{
    internal sealed class SettingsStorePropertyNameConverter : ValueConverter<string, string>
    {
        protected override string Convert(string value, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value) ? "(Default)" : value;
        }
    }
}
