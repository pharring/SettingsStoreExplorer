// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System.Globalization;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell.Interop;
using VsKnownMonikers = Microsoft.VisualStudio.Imaging.KnownMonikers;

namespace SettingsStoreExplorer
{
    internal class SettingsStoreItemToImageMonikerConverter : ValueConverter<SettingsStoreItem, ImageMoniker>
    {
        protected override ImageMoniker Convert(SettingsStoreItem value, object parameter, CultureInfo culture)
        {
            switch(value)
            {
                case RootSettingsStore rootItem:
                    switch (rootItem.EnclosingScope)
                    {
                        case __VsEnclosingScopes.EnclosingScopes_UserSettings:
                            return VsKnownMonikers.User;

                        default:
                            return VsKnownMonikers.Registry;
                    }

                case SettingsStoreSubCollection _:
                    return VsKnownMonikers.FolderClosed;

                default:
                    return base.Convert(value, parameter, culture);
            }
        }
    }
}
