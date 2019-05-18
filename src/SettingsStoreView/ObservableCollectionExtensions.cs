// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SettingsStoreView
{
    internal static class ObservableCollectionExtensions
    {
        /// <summary>
        /// <see cref="ObservableCollection{T}"/> doesn't have bulk operations. So we do it naïvely.
        /// </summary>
        /// <typeparam name="T">Type of the items in the collection.</typeparam>
        /// <param name="collection">The observable collection to augment.</param>
        /// <param name="items">The items to add.</param>
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        /// <summary>
        /// <see cref="ObservableCollection{T}"/> doesn't have bulk operations. So we do it naïvely.
        /// </summary>
        /// <typeparam name="T">Type of the items in the collection.</typeparam>
        /// <param name="collection">The observable collection to modify.</param>
        /// <param name="newItems">The new items to add. Existing items will be removed.</param>
        public static void ReplaceAll<T>(this ObservableCollection<T> collection, IEnumerable<T> newItems)
        {
            collection.Clear();
            collection.AddRange(newItems);
        }
    }
}
