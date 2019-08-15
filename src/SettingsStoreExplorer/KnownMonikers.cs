// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using Microsoft.VisualStudio.Imaging.Interop;
using System;

namespace SettingsStoreExplorer
{
    internal static class KnownMonikers
    {
        public static readonly Guid CustomImages = new Guid("3da9ddb5-b35b-4ed6-9d52-73aa4c30127e");
        public static readonly ImageMoniker SettingsStoreExplorer = new ImageMoniker { Guid = CustomImages, Id = 1 };
    }
}
