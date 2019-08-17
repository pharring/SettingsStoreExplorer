// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

// I believe it's safe to suppress the analyzer warning about accessing the settings
// store only from the main thread.
#pragma warning disable VSTHRD010 // Invoke single-threaded types on Main thread

namespace SettingsStoreExplorer
{
    internal static class IVsSettingsStoreExtensions
    {
        public static string GetString(this IVsSettingsStore store, string collectionPath, string propertyName)
        {
            ErrorHandler.ThrowOnFailure(store.GetString(collectionPath, propertyName, out var value));
            return value;
        }

        public static uint GetUint32(this IVsSettingsStore store, string collectionPath, string propertyName)
        {
            ErrorHandler.ThrowOnFailure(store.GetUnsignedInt(collectionPath, propertyName, out var value));
            return value;
        }

        public static ulong GetUint64(this IVsSettingsStore store, string collectionPath, string propertyName)
        {
            ErrorHandler.ThrowOnFailure(store.GetUnsignedInt64(collectionPath, propertyName, out var value));
            return value;
        }

        public static byte[] GetByteArray(this IVsSettingsStore store, string collectionPath, string propertyName)
        {
            uint[] actualByteLength = { 0 };
            ErrorHandler.ThrowOnFailure(store.GetBinary(collectionPath, propertyName, 0, null, actualByteLength));
            byte[] binaryValue = new byte[actualByteLength[0]];
            ErrorHandler.ThrowOnFailure(store.GetBinary(collectionPath, propertyName, actualByteLength[0], binaryValue, actualByteLength));
            return binaryValue;
        }

        public static IEnumerable<string> GetSubCollectionNames(this IVsSettingsStore store, string collectionPath)
        {
            // Don't get the count up-front. It's essentially an enumeration which is as expensive
            // as just looping until we get an error.

            for (uint index = 0; ; index++)
            {
                string subCollectionName;

                try
                {
                    if (ErrorHandler.Failed(store.GetSubCollectionName(collectionPath, index, out subCollectionName)))
                    {
                        break;
                    }

                }
                catch (IndexOutOfRangeException)
                {
                    break;
                }

                yield return subCollectionName;
            }
        }

        public static bool CollectionExists(this IVsSettingsStore store, string collectionPath)
        {
            ErrorHandler.ThrowOnFailure(store.CollectionExists(collectionPath, out int exists));
            return exists != 0;
        }

        public static IEnumerable<string> GetPropertyNames(this IVsSettingsStore store, string collectionPath)
        {
            for (uint index = 0; ; index++)
            {
                string name;
                try
                {
                    if (ErrorHandler.Failed(store.GetPropertyName(collectionPath, index, out name)))
                    {
                        break;

                    }
                }
                catch (IndexOutOfRangeException)
                {
                    break;
                }

                yield return name;
            }
        }

        public static __VsSettingsType GetPropertyType(this IVsSettingsStore store, string collectionPath, string propertyName)
        {
            ErrorHandler.ThrowOnFailure(store.GetPropertyType(collectionPath, propertyName, out var type));
            return (__VsSettingsType)type;
        }

        public static bool PropertyExists(this IVsSettingsStore store, string collectionPath, string propertyName)
        {
            ErrorHandler.ThrowOnFailure(store.PropertyExists(collectionPath, propertyName, out int exists));
            return exists != 0;
        }
    }
}
