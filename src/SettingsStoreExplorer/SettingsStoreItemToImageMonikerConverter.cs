// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System.Globalization;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.PlatformUI;
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
                        case Scope.User:
                            return VsKnownMonikers.User;

                        case Scope.Remote:
                            return VsKnownMonikers.ServerSettings;

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
