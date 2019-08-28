// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Interop;

namespace SettingsStoreExplorer
{
    internal class RoamingSettingsStore : IVsSettingsStore
    {
        private readonly ISettingsManager _settingsManager;
        private string[] _names;

        public RoamingSettingsStore(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
        }

        /// <summary>
        /// Compute the insert point for the given search term.
        /// </summary>
        /// <param name="corpus">The sorted array of terms.</param>
        /// <param name="searchTerm">The term to search for.</param>
        /// <returns>The index where <paramref name="searchTerm"/> should be inserted into <paramref name="corpus"/> to maintain the ordering.</returns>
        private static int LowerBound<T>(T[] corpus, T searchTerm, IComparer<T> comparer)
        {
            int lo = 0, hi = corpus.Length;

            while (lo != hi)
            {
                var mid = lo + (hi - lo) / 2;

                var midValue = corpus[mid];

                if (comparer.Compare(midValue, searchTerm) < 0)
                {
                    lo = mid + 1;
                }
                else
                {
                    hi = mid;
                }
            }

            return lo;
        }

        private string[] AllNames => _names ?? (_names = InitializeNames());

        /// <summary>
        /// Retrieve all settings from the settings manager, sort them and filter out _metadata
        /// </summary>
        /// <returns>The initial set of settings.</returns>
        private string[] InitializeNames()
        {
            var names = _settingsManager.NamesStartingWith("");
            Array.Sort(names, SettingsComparer.Instance);

            // Remove "_metadata."
            var metadataStart = LowerBound(names, "_metadata.", SettingsComparer.Instance);
            int metadataLength = 0;

            for (int i = metadataStart; i < names.Length; i++, metadataLength++)
            {
                if (!names[i].StartsWith("_metadata.", StringComparison.Ordinal))
                {
                    break;
                }
            }

            if (metadataLength > 0)
            {
                int metadataEnd = metadataStart + metadataLength;
                Array.Copy(names, metadataEnd, names, metadataStart, names.Length - metadataEnd);
                Array.Resize(ref names, names.Length - metadataLength);
            }

            return names;
        }

        private IEnumerable<string> NamesStartingWith(string prefix)
        {
            var names = AllNames;
            for (var index = LowerBound(AllNames, prefix, SettingsComparer.Instance); index < names.Length; index++)
            {
                var name = names[index];
                if (!name.StartsWith(prefix, StringComparison.Ordinal))
                {
                    break;
                }

                yield return name;
            }
        }

        private IEnumerable<string> GetSubCollections(string collectionPath)
        {
            if (collectionPath.Length > 0)
            {
                collectionPath += ".";
            }

            string lastYield = null;
            var suffixStart = collectionPath.Length;

            foreach (var name in NamesStartingWith(collectionPath))
            {
                var separatorIndex = name.IndexOf('.', suffixStart);
                if (separatorIndex < 0)
                {
                    // Found a property, not a subcollection
                    continue;
                }

                var candidateLength = separatorIndex - suffixStart;
                if (lastYield == null || string.CompareOrdinal(lastYield, 0, name, suffixStart, candidateLength) != 0)
                {
                    yield return lastYield = name.Substring(suffixStart, candidateLength);
                }
            }
        }

        private IEnumerable<string> GetProperties(string collectionPath)
        {
            if (collectionPath.Length == 0)
            {
                // Properties of the root node are at the beginning of the collection.
                foreach (var name in _names)
                {
                    if (name.Contains('.'))
                    {
                        // Reached the first subcollection.
                        break;
                    }

                    yield return name;
                }
            }
            else
            {
                collectionPath += ".";
                var prefixLength = collectionPath.Length;

                foreach (var name in NamesStartingWith(collectionPath))
                {
                    if (name.IndexOf('.', prefixLength) >= 0)
                    {
                        // There is a subcollection here.
                        continue;
                    }

                    yield return name.Substring(prefixLength);
                }
            }
        }

        private static string MakeFullName(string collectionPath, string propertyName)
            => string.IsNullOrEmpty(collectionPath) ? propertyName : (collectionPath + "." + propertyName);

        public int GetBool(string collectionPath, string propertyName, out int value) => throw new NotImplementedException();
        public int GetInt(string collectionPath, string propertyName, out int value) => throw new NotImplementedException();

        public int GetUnsignedInt(string collectionPath, string propertyName, out uint value)
        {
            if (_settingsManager.TryGetValue<object>(MakeFullName(collectionPath, propertyName), out object obj) == GetValueResult.Success)
            {
                value = Convert.ToUInt32(obj);
                return VSConstants.S_OK;
            }

            value = 0;
            return VSConstants.E_FAIL;
        }

        public int GetInt64(string collectionPath, string propertyName, out long value) => throw new NotImplementedException();

        public int GetUnsignedInt64(string collectionPath, string propertyName, out ulong value)
        {
            if (_settingsManager.TryGetValue<object>(MakeFullName(collectionPath, propertyName), out object obj) == GetValueResult.Success)
            {
                value = Convert.ToUInt64(obj);
                return VSConstants.S_OK;
            }

            value = 0;
            return VSConstants.E_FAIL;
        }

        public int GetString(string collectionPath, string propertyName, out string value)
        {
            if (_settingsManager.TryGetValue<object>(MakeFullName(collectionPath, propertyName), out object obj) == GetValueResult.Success)
            {
                value = Convert.ToString(obj, CultureInfo.InvariantCulture);
                return VSConstants.S_OK;
            }

            value = null;
            return VSConstants.E_FAIL;
        }

        public int GetBinary(string collectionPath, string propertyName, uint byteLength, byte[] pBytes, uint[] actualByteLength) => throw new NotImplementedException();
        public int GetBoolOrDefault(string collectionPath, string propertyName, int defaultValue, out int value) => throw new NotImplementedException();
        public int GetIntOrDefault(string collectionPath, string propertyName, int defaultValue, out int value) => throw new NotImplementedException();
        public int GetUnsignedIntOrDefault(string collectionPath, string propertyName, uint defaultValue, out uint value) => throw new NotImplementedException();
        public int GetInt64OrDefault(string collectionPath, string propertyName, long defaultValue, out long value) => throw new NotImplementedException();
        public int GetUnsignedInt64OrDefault(string collectionPath, string propertyName, ulong defaultValue, out ulong value) => throw new NotImplementedException();
        public int GetStringOrDefault(string collectionPath, string propertyName, string defaultValue, out string value) => throw new NotImplementedException();
        public int GetPropertyType(string collectionPath, string propertyName, out uint type)
        {
            var fullName = string.IsNullOrEmpty(collectionPath) ? propertyName : (collectionPath + "." + propertyName);
            var result = _settingsManager.TryGetValue<object>(fullName, out object value);
            if (result == GetValueResult.Success)
            {
                switch (value)
                {
                    case string _:
                        type = (uint)__VsSettingsType.SettingsType_String;
                        return VSConstants.S_OK;

                    case uint _:
                    case int _:
                    case bool _:
                        type = (uint)__VsSettingsType.SettingsType_Int;
                        return VSConstants.S_OK;

                    case ulong _:
                    case long _:
                        type = (uint)__VsSettingsType.SettingsType_Int64;
                        return VSConstants.S_OK;

                    default:
                        // Treat it like a string
                        type = (uint)__VsSettingsType.SettingsType_String;
                        return VSConstants.S_OK;

                }
            }

            type = 0;
            return VSConstants.E_FAIL;
        }

        public int PropertyExists(string collectionPath, string propertyName, out int pfExists) => throw new NotImplementedException();
        public int CollectionExists(string collectionPath, out int pfExists) => throw new NotImplementedException();
        public int GetSubCollectionCount(string collectionPath, out uint subCollectionCount) => throw new NotImplementedException();
        public int GetPropertyCount(string collectionPath, out uint propertyCount) => throw new NotImplementedException();
        public int GetLastWriteTime(string collectionPath, SYSTEMTIME[] lastWriteTime) => throw new NotImplementedException();

        public int GetSubCollectionName(string collectionPath, uint index, out string subCollectionName)
        {
            foreach (var name in GetSubCollections(collectionPath))
            {
                if (index-- == 0)
                {
                    subCollectionName = name;
                    return VSConstants.S_OK;
                }
            }

            subCollectionName = null;
            return VSConstants.E_INVALIDARG;
        }

        public int GetPropertyName(string collectionPath, uint index, out string propertyName)
        {
            foreach (var name in GetProperties(collectionPath))
            {
                if (index-- == 0)
                {
                    propertyName = name;
                    return VSConstants.S_OK;
                }
            }

            propertyName = null;
            return VSConstants.E_INVALIDARG;
        }

        /// <summary>
        /// Custom comparer for sorting setting names.
        /// The period char is a separator, so it should sort ahead of any other char.
        /// A special case is for names that have no dots; they are properties
        /// of the root node.
        /// </summary>
        private class SettingsComparer : IComparer<string>
        {
            public static readonly SettingsComparer Instance = new SettingsComparer();

            private SettingsComparer() { }

            public int Compare(string x, string y)
            {
                if (x == null)
                {
                    // Null is less than everything except null
                    return y == null ? 0 : -1;
                }

                if (y == null)
                {
                    // Everything is greater than null
                    return 1;
                }

                // Special case: Root properties (with no dot) sort first.
                if (x.Contains('.'))
                {
                    if (!y.Contains('.'))
                    {
                        // y is a root property
                        return 1;
                    }
                }
                else
                {
                    // x is a root property
                    if (y.Contains('.'))
                    {
                        return -1;
                    }
                }

                for (int i = 0; i < x.Length; i++)
                {
                    if (i >= y.Length)
                    {
                        // x is longer than y
                        return 1;
                    }

                    char cx = x[i];
                    char cy = y[i];
                    if (cx != cy)
                    {
                        // dot sorts first
                        if (cx == '.')
                        {
                            return -1;
                        }

                        if (cy == '.')
                        {
                            return 1;
                        }

                        return cx.CompareTo(cy);
                    }
                }

                // x is equal to or a prefix of y
                return y.Length == x.Length ? 0 : -1;
            }
        }
    }
}
