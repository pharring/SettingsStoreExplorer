// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System.Globalization;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using VsKnownMonikers = Microsoft.VisualStudio.Imaging.KnownMonikers;

namespace SettingsStoreExplorer
{
    internal class SettingsStoreTypeToImageMonikerConverter : ValueConverter<__VsSettingsType, ImageMoniker>
    {
        protected override ImageMoniker Convert(__VsSettingsType value, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case __VsSettingsType.SettingsType_Int:
                case __VsSettingsType.SettingsType_Int64:
                    return VsKnownMonikers.Numeric;

                case __VsSettingsType.SettingsType_String:
                    return VsKnownMonikers.StringRegistryValue;

                case __VsSettingsType.SettingsType_Binary:
                    return VsKnownMonikers.BinaryRegistryValue;

                default:
                    return default;
            }
        }
    }
}
