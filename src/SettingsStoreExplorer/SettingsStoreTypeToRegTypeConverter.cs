// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System.Globalization;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;

namespace SettingsStoreExplorer
{
    internal class SettingsStoreTypeToRegTypeConverter : ValueConverter<__VsSettingsType, string>
    {
        protected override string Convert(__VsSettingsType value, object parameter, CultureInfo culture)
        {
            switch(value)
            {
                case __VsSettingsType.SettingsType_Binary:
                    return "REG_BINARY";

                case __VsSettingsType.SettingsType_Int:
                    return "REG_DWORD";

                case __VsSettingsType.SettingsType_String:
                    return "REG_SZ";

                case __VsSettingsType.SettingsType_Int64:
                    return "REG_QWORD";

                default:
                    return "Unknown";
            }
        }
    }
}
