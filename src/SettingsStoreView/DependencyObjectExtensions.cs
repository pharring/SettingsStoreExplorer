// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Media;

namespace SettingsStoreView
{
    internal static class DependencyObjectExtensions
    {
        /// <summary>
        /// Searches up the visual tree for an ancestor of the given type.
        /// </summary>
        /// <typeparam name="T">The type of ancestor to find.</typeparam>
        /// <param name="dependencyObject">The starting object.</param>
        /// <returns>The first ancestor of the given type or null if none was found.</returns>
        public static T FindVisualAncestor<T>(this DependencyObject dependencyObject) where T : DependencyObject
        {
            switch (dependencyObject)
            {
                case Visual visual:
                    dependencyObject = VisualTreeHelper.GetParent(visual);
                    break;

                default:
                    dependencyObject = LogicalTreeHelper.GetParent(dependencyObject);
                    break;
            }

            while (true)
            {
                switch (dependencyObject)
                {
                    case null:
                        return null;

                    case T found:
                        return found;

                    case Visual visual:
                        dependencyObject = VisualTreeHelper.GetParent(visual);
                        break;

                    default:
                        dependencyObject = LogicalTreeHelper.GetParent(dependencyObject);
                        break;
                }
            }
        }

        /// <summary>
        /// Searches down the visual tree using a depth-first search to find a descendent of the given type.
        /// </summary>
        /// <typeparam name="T">The type of descendent to find.</typeparam>
        /// <param name="dependencyObject">The starting object.</param>
        /// <returns>The first descendent of the given type or null if none was found.</returns>
        public static T FindVisualDescendent<T>(this DependencyObject dependencyObject) where T : DependencyObject
        {
            var childrenCount = VisualTreeHelper.GetChildrenCount(dependencyObject);
            for (int childIndex = 0; childIndex < childrenCount; childIndex++)
            {
                var child = VisualTreeHelper.GetChild(dependencyObject, childIndex);
                if (child is T found)
                {
                    return found;
                }

                found = FindVisualDescendent<T>(child);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}
