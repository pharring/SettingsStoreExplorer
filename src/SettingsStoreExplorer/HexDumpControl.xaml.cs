// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SettingsStoreExplorer
{
    /// <summary>
    /// Interaction logic for HexDumpControl.xaml
    /// The data context is a byte array.
    /// The user control consists of three text boxes side-by-side with their
    /// scroll positions linked.
    /// The 1st text box shows the offset of the first byte of the row.
    /// The 2nd text box contains the hexadecimal digits for the row.
    /// The 3rd text box contains the ASCII values of the bytes.
    /// </summary>
    public partial class HexDumpControl : UserControl
    {
        private HexDumpControlDataContext _innerDataContext;

        /// <summary>
        /// The constructor.
        /// </summary>
        public HexDumpControl()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        public byte[] EditedValue => _innerDataContext.Bytes;

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ICollection<byte> bytes)
            {
                _innerDataContext = new HexDumpControlDataContext(bytes);
                asciiTextBox.DataContext = _innerDataContext;
                hexBytesTextBox.DataContext = _innerDataContext;
                addressTextBox.DataContext = _innerDataContext;
            }
        }

        /// <summary>
        /// Handler for the <see cref="ScrollViewer.ScrollChanged"/> event on
        /// any of the text boxes.
        /// </summary>
        /// <param name="sender">The scroll viewer whose scroll position has changed.</param>
        /// <param name="e">The event arguments.</param>
        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // Link the vertical scroll positions together.
            var verticalOffset = e.VerticalOffset;
            addressTextBox.ScrollToVerticalOffset(verticalOffset);
            hexBytesTextBox.ScrollToVerticalOffset(verticalOffset);
            asciiTextBox.ScrollToVerticalOffset(verticalOffset);
        }

        /// <summary>
        /// Handler for <see cref="TextBoxBase.SelectionChanged"/> event on the hex bytes text box.
        /// </summary>
        /// <param name="sender">The sender (text box)</param>
        /// <param name="e">The event arguments.</param>
        private void HexBytesTextBox_SelectionChanged(object sender, RoutedEventArgs e) 
            => asciiTextBox.InsertPointAndSelectionLength = hexBytesTextBox.InsertPointAndSelectionLength;

        /// <summary>
        /// Handler for <see cref="TextBoxBase.SelectionChanged"/> event on the ASCII text box.
        /// </summary>
        /// <param name="sender">The sender (text box)</param>
        /// <param name="e">The event arguments.</param>
        private void AsciiTextBox_SelectionChanged(object sender, RoutedEventArgs e)
            => hexBytesTextBox.InsertPointAndSelectionLength = asciiTextBox.InsertPointAndSelectionLength;
    }
}
