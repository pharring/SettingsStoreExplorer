// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System.Windows.Controls;

namespace SettingsStoreExplorer
{
    internal static class ItemContainerGeneratorExtensions
    {
        /// <summary>
        /// A version of ContainerFromItem that works with HierarchicalDataTemplate
        /// </summary>
        /// <param name="containerGenerator">The container generator.</param>
        /// <param name="item">The item you're looking for.</param>
        /// <typeparam name="T">The type of items control (usually a TreeViewItem).</typeparam>
        /// <returns>The container object.</returns>
        public static T ContainerFromItemRecursive<T>(this ItemContainerGenerator containerGenerator, object item) where T : ItemsControl
        {
            if (containerGenerator.ContainerFromItem(item) is T container)
            {
                return container;
            }

            foreach (var subItem in containerGenerator.Items)
            {
                if (containerGenerator.ContainerFromItem(subItem) is ItemsControl itemsControl)
                {
                    container = itemsControl.ItemContainerGenerator.ContainerFromItemRecursive<T>(item);
                    if (container != null)
                    {
                        return container;
                    }
                }
            }

            return null;
        }
    }
}
