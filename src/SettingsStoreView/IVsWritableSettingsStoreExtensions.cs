// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace SettingsStoreView
{
    internal static class IVsWritableSettingsStoreExtensions
    {
        public static void CopyTree(this IVsWritableSettingsStore writableSettingsStore, SettingsStoreSubCollection from, SettingsStoreSubCollection to)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            writableSettingsStore.CopyProperties(from, to);

            var fromStore = from.Root.SettingsStore;
            var fromPath = from.Path;

            for (uint index = 0; ; index++)
            {
                if (ErrorHandler.Failed(fromStore.GetSubCollectionName(fromPath, index, out var name)))
                {
                    break;
                }

                var newSubCollection = new SettingsStoreSubCollection(to, name);
                ErrorHandler.ThrowOnFailure(writableSettingsStore.CreateCollection(newSubCollection.Name));

                writableSettingsStore.CopyTree(new SettingsStoreSubCollection(from, name), newSubCollection);
            }
        }

        public static void CopyProperties(this IVsWritableSettingsStore writableSettingsStore, SettingsStoreSubCollection from, SettingsStoreSubCollection to)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var fromStore = from.Root.SettingsStore;
            var fromPath = from.Path;
            var toPath = to.Path;

            for (uint index = 0; ; index++)
            {
                if (ErrorHandler.Failed(fromStore.GetPropertyName(fromPath, index, out var name)))
                {
                    break;
                }

                if (ErrorHandler.Failed(fromStore.GetPropertyType(fromPath, name, out var type)))
                {
                    break;
                }

                switch ((__VsSettingsType)type)
                {
                    case __VsSettingsType.SettingsType_String:
                        ErrorHandler.ThrowOnFailure(fromStore.GetString(fromPath, name, out var stringValue));
                        ErrorHandler.ThrowOnFailure(writableSettingsStore.SetString(toPath, name, stringValue));
                        break;

                    case __VsSettingsType.SettingsType_Int:
                        ErrorHandler.ThrowOnFailure(fromStore.GetInt(fromPath, name, out var intValue));
                        ErrorHandler.ThrowOnFailure(writableSettingsStore.SetInt(toPath, name, intValue));
                        break;

                    case __VsSettingsType.SettingsType_Int64:
                        ErrorHandler.ThrowOnFailure(fromStore.GetInt64(fromPath, name, out var longValue));
                        ErrorHandler.ThrowOnFailure(writableSettingsStore.SetInt64(toPath, name, longValue));
                        break;

                    case __VsSettingsType.SettingsType_Binary:
                        uint[] actualByteLength = { 0 };
                        ErrorHandler.ThrowOnFailure(fromStore.GetBinary(fromPath, name, 0, null, actualByteLength));
                        byte[] bytes = new byte[actualByteLength[0]];
                        ErrorHandler.ThrowOnFailure(fromStore.GetBinary(fromPath, name, actualByteLength[0], bytes, actualByteLength));
                        ErrorHandler.ThrowOnFailure(writableSettingsStore.SetBinary(toPath, name, actualByteLength[0], bytes));
                        break;
                }
            }
        }

        public static void CopyProperty(this IVsWritableSettingsStore store, SettingsStoreProperty from, string toName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var fromName = from.Name;
            var collectionPath = from.CollectionPath;

            switch (from.Type)
            {
                case __VsSettingsType.SettingsType_String:
                    ErrorHandler.ThrowOnFailure(store.GetString(collectionPath, fromName, out var stringValue));
                    ErrorHandler.ThrowOnFailure(store.SetString(collectionPath, toName, stringValue));
                    break;

                case __VsSettingsType.SettingsType_Int:
                    ErrorHandler.ThrowOnFailure(store.GetInt(collectionPath, fromName, out var intValue));
                    ErrorHandler.ThrowOnFailure(store.SetInt(collectionPath, toName, intValue));
                    break;

                case __VsSettingsType.SettingsType_Int64:
                    ErrorHandler.ThrowOnFailure(store.GetInt64(collectionPath, fromName, out var longValue));
                    ErrorHandler.ThrowOnFailure(store.SetInt64(collectionPath, toName, longValue));
                    break;

                case __VsSettingsType.SettingsType_Binary:
                    uint[] actualByteLength = { 0 };
                    ErrorHandler.ThrowOnFailure(store.GetBinary(collectionPath, fromName, 0, null, actualByteLength));
                    byte[] bytes = new byte[actualByteLength[0]];
                    ErrorHandler.ThrowOnFailure(store.GetBinary(collectionPath, fromName, actualByteLength[0], bytes, actualByteLength));
                    ErrorHandler.ThrowOnFailure(store.SetBinary(collectionPath, toName, actualByteLength[0], bytes));
                    break;
            }
        }
    }
}
