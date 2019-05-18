// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System;
using System.ComponentModel.Design;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft;
using Microsoft.VisualStudio.Shell.Interop;
using static SettingsStoreView.SettingsStoreCommandSet;

namespace SettingsStoreView
{
    /// <summary>
    /// Interaction logic for SettingsStoreViewToolWindowControl.
    /// </summary>
    public partial class SettingsStoreViewToolWindowControl : UserControl
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsStoreViewToolWindowControl"/> class.
        /// </summary>
        public SettingsStoreViewToolWindowControl(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();
        }

        public void InitializeViewModel(IVsSettingsManager settingsManager) => DataContext = new SettingsStoreViewModel(settingsManager);

        /// <summary>
        /// Handle right-clicks on tree view items by moving focus to the click-on node so that context menu commands apply to the
        /// correct node.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void TreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var originalSource = e.OriginalSource as DependencyObject;
            var treeViewItem = originalSource.FindVisualAncestor<TreeViewItem>();
            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                e.Handled = true;
            }
        }

        private IMenuCommandService MenuCommandService
        {
            get
            {
                var menuCommandService = (IMenuCommandService)_serviceProvider.GetService(typeof(IMenuCommandService));
                Assumes.Present(menuCommandService);
                return menuCommandService;
            }
        }

        public void InPlaceEditTreeViewItem(object item, string initialText, Action<string> onAcceptEdit)
        {
            // Find the control for the given item.
            var treeViewItem = treeView.ItemContainerGenerator.ContainerFromItemRecursive<TreeViewItem>(item);
            if (treeViewItem != null)
            {
                treeViewItem.BringIntoView();
                treeViewItem.Focus();
                treeViewItem.InPlaceEdit(initialText, onAcceptEdit);
            }
        }

        public void SetTreeViewSelection(SettingsStoreSubCollection subCollection)
        {
            var treeViewItem = treeView.ItemContainerGenerator.ContainerFromItemRecursive<TreeViewItem>(subCollection);

            if (treeViewItem == null)
            {
                // Try expanding the parent
                var parentCollection = subCollection.Parent;
                if (parentCollection != null)
                {
                    var parentTreeViewItem = treeView.ItemContainerGenerator.ContainerFromItemRecursive<TreeViewItem>(parentCollection);
                    if (parentTreeViewItem != null && !parentTreeViewItem.IsExpanded)
                    {
                        parentTreeViewItem.IsExpanded = true;

                        // Try again
#pragma warning disable VSTHRD001 // Avoid legacy thread switching APIs
                        Dispatcher.Invoke(() => SetTreeViewSelection(subCollection), DispatcherPriority.Render);
#pragma warning restore VSTHRD001 // Avoid legacy thread switching APIs

                        return;
                    }
                }
            }

            if (treeViewItem != null)
            {
                treeViewItem.BringIntoView();
                treeViewItem.Focus();
            }
        }

        public void InPlaceEditListViewItem(SettingsStoreProperty item, string initialText, Action<string> onAcceptEdit)
        {
            // Find the control for the given item.
            if (listView.ItemContainerGenerator.ContainerFromItem(item) is ListViewItem listViewItem)
            {
                listViewItem.BringIntoView();
                listViewItem.Focus();
                listViewItem.InPlaceEdit(initialText, onAcceptEdit);
            }
        }

        public void SetListViewSelection(SettingsStoreProperty item)
        {
            if (!listView.IsFocused)
            {
                listView.Focus();
            }

            if (listView.ItemContainerGenerator.ContainerFromItem(item) is ListViewItem listViewItem)
            {
                listViewItem.BringIntoView();
                listViewItem.Focus();
            }
            else
            {
                // This may be a newly-created item, so it hasn't been generated yet.
                // Try again after the next render.
#pragma warning disable VSTHRD001 // Avoid legacy thread switching APIs
                Dispatcher.Invoke(() => SetListViewSelection(item), DispatcherPriority.Render);
#pragma warning restore VSTHRD001 // Avoid legacy thread switching APIs
            }
        }

        private void TreeView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // Show the tree view item context menu.
            if (e.OriginalSource as DependencyObject is FrameworkElement frameworkElement)
            {
                var treeViewItem = frameworkElement.FindVisualAncestor<TreeViewItem>();
                if (treeViewItem != null)
                {
                    Point position;
                    if (e.CursorLeft == -1 && e.CursorTop == -1)
                    {
                        // Position the menu in the center of the selected item. But don't use dimensions of the
                        // TreeViewItem itself, because that includes the itemscontrol of any expanded items. Instead, we want
                        // just the label (TextBock) part of the header.
                        var label = treeViewItem.FindVisualDescendent<TextBlock>();
                        if (label != null)
                        {
                            position = label.PointToScreen(new Point(label.ActualWidth / 2, label.ActualHeight / 2));
                        }
                        else
                        {
                            position = treeViewItem.PointToScreen(new Point(0, 0));
                        }
                    }
                    else
                    {
                        position = frameworkElement.PointToScreen(new Point(e.CursorLeft, e.CursorTop));
                    }

                    MenuCommandService.ShowContextMenu(TreeViewItemContextMenu, (int)position.X, (int)position.Y);
                }
            }

            e.Handled = true;
        }

        private void ListView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (e.OriginalSource is FrameworkElement frameworkElement)
            {
                var listViewItem = frameworkElement.FindVisualAncestor<ListViewItem>();

                // If the selection is on an item, show the "Modify/Rename/Delete" context menu.
                // Otherwise, show the "New" context menu.

                var menuId = listViewItem != null ? ListViewItemContextMenu : ListViewContextMenu;

                var position = e.CursorLeft == -1 && e.CursorTop == -1
                    ? listViewItem.PointToScreen(new Point(listViewItem.ActualWidth / 2, listViewItem.ActualHeight / 2))
                    : frameworkElement.PointToScreen(new Point(e.CursorLeft, e.CursorTop));

                MenuCommandService.ShowContextMenu(menuId, (int)position.X, (int)position.Y);
            }

            e.Handled = true;
        }
    }
}
