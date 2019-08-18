// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft;

namespace SettingsStoreExplorer
{
    internal static class ControlExtensions
    {
        public static void InPlaceEdit(this Control control, string initialText, Action<bool> onEditing, Action<string> onAcceptEdit)
        {
            if (!InPlaceEditInternal(control, initialText, onEditing, onAcceptEdit))
            {
                // This can fail for virtualized controls. Retry after a layout/render.
#pragma warning disable VSTHRD001 // Avoid legacy thread switching APIs
                control.Dispatcher.Invoke(() => InPlaceEditInternal(control, initialText, onEditing, onAcceptEdit), DispatcherPriority.Render);
#pragma warning restore VSTHRD001 // Avoid legacy thread switching APIs
            }
        }

        private static bool InPlaceEditInternal(Control control, string initialText, Action<bool> onEditing, Action<string> onAcceptEdit)
        {
            var stackPanel = control.FindVisualDescendent<StackPanel>();
            if (stackPanel == null)
            {
                return false;
            }

            var label = stackPanel.FindName("label") as TextBlock;
            Assumes.NotNull(label);

            var editor = stackPanel.FindName("editor") as TextBox;
            Assumes.NotNull(editor);

            void OnPreviewKeyDown(object sender, KeyEventArgs e)
            {
                switch (e.Key)
                {
                    case Key.Escape:
                        editor.Text = initialText;
                        goto case Key.Enter;

                    case Key.Enter:
                        e.Handled = true;
                        control.Focus();
                        return;

                    case Key.Tab:
                        // Regedit "dings" here
                        e.Handled = true;
                        return;
                }
            }

            void OnLostFocus(object sender, RoutedEventArgs e)
            {
                e.Handled = true;

                editor.Visibility = Visibility.Collapsed;
                label.Visibility = Visibility.Visible;

                editor.LostFocus -= OnLostFocus;
                editor.PreviewKeyDown -= OnPreviewKeyDown;

                onEditing(false);

                if (editor.Text != initialText)
                {
                    onAcceptEdit(editor.Text);
                }
            }

            editor.Text = initialText;
            editor.PreviewKeyDown += OnPreviewKeyDown;
            editor.LostFocus += OnLostFocus;

            label.Visibility = Visibility.Collapsed;
            editor.Visibility = Visibility.Visible;

            onEditing(true);
            editor.SelectAll();
            editor.Focus();

            return true;
        }
    }
}
