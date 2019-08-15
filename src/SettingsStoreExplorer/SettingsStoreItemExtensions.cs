// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using static System.FormattableString;

namespace SettingsStoreExplorer
{
    internal static class SettingsStoreItemExtensions
    {
        public static bool TryGetWritableSettingsStore(this SettingsStoreItem settingsStoreItem, out IVsWritableSettingsStore writableSettingsStore)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!(ServiceProvider.GlobalProvider.GetService(typeof(SVsSettingsManager)) is IVsSettingsManager settingsManager))
            {
                writableSettingsStore = null;
                return false;
            }

            return ErrorHandler.Succeeded(settingsManager.GetWritableSettingsStore((uint)settingsStoreItem.Root.EnclosingScope, out writableSettingsStore));
        }

        public static SettingsStoreSubCollection GenerateNewSubCollection(this SettingsStoreSubCollection subCollection)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var settingsStore = subCollection.Root.SettingsStore;

            for (int i = 1; i < 100; i++)
            {
                var name = Invariant($"New Collection #{i}");
                var newCollection = new SettingsStoreSubCollection(subCollection, name);

                ErrorHandler.ThrowOnFailure(settingsStore.CollectionExists(newCollection.Path, out int exists));
                if (exists == 0)
                {
                    return newCollection;
                }
            }

            throw new InvalidOperationException("Could not find a unique name for the new subcollection.");
        }

        public static string GenerateNewPropertyName(this SettingsStoreSubCollection subCollection)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var settingsStore = subCollection.Root.SettingsStore;

            for (int i = 1; i < 100; i++)
            {
                var name = Invariant($"New Value #{i}");

                ErrorHandler.ThrowOnFailure(settingsStore.PropertyExists(subCollection.Path, name, out int exists));
                if (exists == 0)
                {
                    return name;
                }
            }

            throw new InvalidOperationException("Could not find a unique name for the new value.");
        }
    }
}
