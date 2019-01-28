// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Imaging.Interop;
using System;

namespace SettingsStoreView
{
    internal static class KnownMonikers
    {
        public static readonly Guid CustomImages = new Guid("3da9ddb5-b35b-4ed6-9d52-73aa4c30127e");
        public static readonly ImageMoniker SettingsStoreViewToolWindow = new ImageMoniker { Guid = CustomImages, Id = 1 };
        public static readonly ImageMoniker SettingsStoreViewToolWindowCommand = new ImageMoniker { Guid = CustomImages, Id = 2 };
    }
}
