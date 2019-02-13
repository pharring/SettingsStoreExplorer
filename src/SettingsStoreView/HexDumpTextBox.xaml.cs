// Copyright (c) Paul Harrington.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Controls;

namespace SettingsStoreView
{
    /// <summary>
    /// Interaction logic for HexDumpTextBox.xaml
    /// </summary>
    public partial class HexDumpTextBox : UserControl
    {
        public HexDumpTextBox()
        {
            InitializeComponent();
        }

        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            // TODO: Allow the caret to go in either the ASCII side
            // or the hex values.

            // TODO: Figure out how to show an extended selection in
            // both sides.
        }

        private void OnPreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Allow only hex chars if on the left.
            // Only ASCII chars on the right.
            // For hex digits need to accept 2 digits before moving on.
            // (1st digit sets low nybble, 2nd digit shifts left, sets lo and moves caret)
            // No need to support clipboard
            // Do support replacement of selected runs
        }
    }
}
