// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace SettingsStoreView
{
    /// <summary>
    /// A text box with a custom adorner to show selection even when not in focus.
    /// </summary>
    internal class TextBoxWithSelectionAdorner : TextBox
    {
        /// <summary>
        /// The selection adorner object.
        /// </summary>
        private readonly Adorner _adorner;

        public TextBoxWithSelectionAdorner() => _adorner = new TextBoxSelectionAdorner(this);

        protected override void OnInitialized(EventArgs e)
        {
            AdornerLayer.GetAdornerLayer(this)?.Add(_adorner);
            base.OnInitialized(e);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);
            _adorner.InvalidateVisual();
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            _adorner.InvalidateVisual();
        }

        protected override void OnSelectionChanged(System.Windows.RoutedEventArgs e)
        {
            base.OnSelectionChanged(e);
            _adorner.InvalidateVisual();
        }

        public new void ScrollToVerticalOffset(double offset)
        {
            base.ScrollToVerticalOffset(offset);
            _adorner.InvalidateVisual();
        }
    }
}
